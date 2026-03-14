namespace YamlParser;

using System.Reflection;
using System.Text.RegularExpressions;

/// <summary>
/// Parser engine with all parsing primitives.
/// Extends Grammar (which defines the 211 YAML 1.2 spec rules).
/// </summary>
public partial class Parser : Grammar
{
    private static readonly bool TRACE = Environment.GetEnvironmentVariable("TRACE") != null
        && Environment.GetEnvironmentVariable("TRACE") != "";

    public Receiver ReceiverObj { get; }
    public string Input { get; set; } = "";
    public int Pos { get; set; }
    public int End { get; set; }
    public List<ParserState> State { get; } = new();

    private int _traceNum;
    private int _traceLine;
    private bool _traceOn = true;
    private int _traceOff;
    private string[] _traceInfo = ["", "", ""];
    private int _traceInfoNum;

    // Cache for Func objects per method name
    private readonly Dictionary<string, Func> _funcCache = new();
    // Cache for receiver methods
    private readonly Dictionary<string, Action<Receiver, MatchInfo>?> _receiverMethodCache = new();

    public Parser(Receiver receiver)
    {
        ReceiverObj = receiver;
        receiver.Parser = this;
    }

    public bool Parse(string input)
    {
        if (input.Length > 0 && !input.EndsWith("\n"))
            input += "\n";

        Input = input;
        End = Input.Length;

        if (TRACE) _traceOn = string.IsNullOrEmpty(TraceStart());

        bool ok;
        try
        {
            ok = Call(GetFunc("TOP"));
            TraceFlush();
        }
        catch
        {
            TraceFlush();
            throw;
        }

        if (!ok) throw new Exception("Parser failed");
        if (Pos < End) throw new Exception("Parser finished before end of input");

        return true;
    }

    public ParserState StateCurr()
    {
        if (State.Count == 0)
            return new ParserState
            {
                Name = null,
                Doc = false,
                Lvl = 0,
                Beg = 0,
                End = 0,
                M = null,
                T = null,
            };
        return State[^1];
    }

    public ParserState? StatePrev()
    {
        if (State.Count < 2) return null;
        return State[^2];
    }

    public void StatePush(string name)
    {
        var curr = StateCurr();
        State.Add(new ParserState
        {
            Name = name,
            Doc = curr.Doc,
            Lvl = curr.Lvl + 1,
            Beg = Pos,
            End = null,
            M = curr.M,
            T = curr.T,
        });
    }

    public void StatePop()
    {
        if (State.Count == 0) return;
        var child = State[^1];
        State.RemoveAt(State.Count - 1);
        if (State.Count == 0) return;
        var curr = State[^1];
        curr.Beg = child.Beg;
        curr.End = Pos;
    }

    private System.Reflection.MethodInfo? FindMethod(string name)
    {
        var method = GetType().GetMethod(name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null)
        {
            // Try PascalCase conversion for snake_case built-in names
            var pascalName = string.Concat(name.Split('_').Select(s =>
                s.Length > 0 ? char.ToUpperInvariant(s[0]) + s[1..] : s));
            method = GetType().GetMethod(pascalName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
        return method;
    }

    public override Func GetFunc(string name)
    {
        if (_funcCache.TryGetValue(name, out var cached))
            return cached;

        var method = FindMethod(name);
        if (method == null)
            throw new Exception($"Can't find parser function '{name}'");

        Delegate del;
        var paramCount = method.GetParameters().Length;
        if (paramCount == 0 && method.ReturnType == typeof(Func))
        {
            del = Delegate.CreateDelegate(typeof(System.Func<Func>), this, method);
        }
        else if (paramCount == 0 && method.ReturnType == typeof(bool))
        {
            del = Delegate.CreateDelegate(typeof(System.Func<bool>), this, method);
        }
        else
        {
            // Parameterized or non-standard return type methods - called via reflection in Call
            del = new System.Func<bool>(() =>
                throw new InvalidOperationException($"'{name}' requires arguments - use Call with args"));
        }

        var func = new Func(del, name);
        _funcCache[name] = func;
        return func;
    }

    /// <summary>
    /// Call a rule function. Returns true/false for boolean rules, or a value for 'any' type.
    /// </summary>
    public bool Call(Func func, string type = "boolean")
    {
        var trace = func.Trace;
        StatePush(trace);

        _traceNum++;
        if (TRACE) Trace("?", trace);

        if (func.Name == "l_bare_document")
            StateCurr().Doc = true;

        var pos = Pos;
        Receive(func, "try", pos);

        // Invoke the function
        bool value;
        var del = func.Del;
        if (del is System.Func<Func> ruleFunc)
        {
            var result = ruleFunc();
            value = Call(result);
        }
        else if (del is System.Func<bool> boolFunc)
        {
            value = boolFunc();
        }
        else
        {
            throw new Exception($"Bad delegate type for '{trace}'");
        }

        _traceNum++;
        if (value)
        {
            if (TRACE) Trace("+", trace);
            Receive(func, "got", pos);
        }
        else
        {
            if (TRACE) Trace("x", trace);
            Receive(func, "not", pos);
        }

        StatePop();
        return value;
    }

    /// <summary>
    /// Call a rule function that takes arguments (e.g., [func, arg1, arg2])
    /// </summary>
    public bool Call(object?[] funcWithArgs, string type = "boolean")
    {
        if (funcWithArgs.Length == 0)
            throw new Exception("Empty function array");

        var func = funcWithArgs[0] as Func;
        if (func == null)
            throw new Exception($"First element is not a Func: {funcWithArgs[0]}");

        var args = new object?[funcWithArgs.Length - 1];
        Array.Copy(funcWithArgs, 1, args, 0, args.Length);

        // Resolve lazy arguments
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] is object?[] innerArray)
            {
                args[i] = CallAny(innerArray);
            }
            else if (args[i] is System.Func<int> lazyInt)
            {
                args[i] = lazyInt();
            }
            else if (args[i] is System.Func<string> lazyStr)
            {
                args[i] = lazyStr();
            }
        }

        var trace = func.Trace;
        StatePush(trace);

        _traceNum++;
        if (TRACE) TraceWithArgs("?", trace, args);

        if (func.Name == "l_bare_document")
            StateCurr().Doc = true;

        var pos = Pos;
        Receive(func, "try", pos);

        // Invoke the rule method via reflection with args
        bool value;

        var method = FindMethod(func.Name);

        if (method == null)
            throw new Exception($"Can't find method '{func.Name}'");

        var paramCount = method.GetParameters().Length;
        var callArgs = new object?[paramCount];
        for (int i = 0; i < paramCount && i < args.Length; i++)
        {
            var p = method.GetParameters()[i];
            if (args[i] == null)
            {
                // Null for value types: use default (0 for int, etc.)
                callArgs[i] = p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null;
            }
            else if (p.ParameterType.IsAssignableFrom(args[i]!.GetType()))
            {
                callArgs[i] = args[i];
            }
            else
            {
                callArgs[i] = Convert.ChangeType(args[i], p.ParameterType);
            }
        }

        var result = method.Invoke(this, callArgs);
        if (result is Func resultFunc)
            value = Call(resultFunc);
        else if (result is object?[] resultArr)
            value = Call(resultArr);
        else if (result is bool boolResult)
            value = boolResult;
        else
            throw new Exception($"Unexpected return type from '{func.Name}': {result?.GetType()}");

        _traceNum++;
        if (value)
        {
            if (TRACE) Trace("+", trace);
            Receive(func, "got", pos);
        }
        else
        {
            if (TRACE) Trace("x", trace);
            Receive(func, "not", pos);
        }

        StatePop();
        return value;
    }

    /// <summary>
    /// Call a function returning any type (number, string, bool).
    /// </summary>
    public object? CallAny(object?[] funcWithArgs)
    {
        if (funcWithArgs.Length == 0) return null;
        var func = funcWithArgs[0] as Func;
        if (func == null) return null;

        var args = new object?[funcWithArgs.Length - 1];
        Array.Copy(funcWithArgs, 1, args, 0, args.Length);

        // Resolve lazy arguments
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] is object?[] innerArray)
            {
                args[i] = CallAny(innerArray);
            }
            else if (args[i] is System.Func<int> lazyInt)
            {
                args[i] = lazyInt();
            }
            else if (args[i] is System.Func<string> lazyStr)
            {
                args[i] = lazyStr();
            }
        }

        var method = FindMethod(func.Name);
        if (method == null)
            throw new Exception($"Can't find method '{func.Name}'");

        var paramCount = method.GetParameters().Length;
        var callArgs = new object?[paramCount];
        for (int i = 0; i < paramCount && i < args.Length; i++)
        {
            var p = method.GetParameters()[i];
            if (args[i] == null)
            {
                callArgs[i] = p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null;
            }
            else if (p.ParameterType.IsAssignableFrom(args[i]!.GetType()))
            {
                callArgs[i] = args[i];
            }
            else
            {
                callArgs[i] = Convert.ChangeType(args[i], p.ParameterType);
            }
        }

        return method.Invoke(this, callArgs);
    }

    public void Receive(Func func, string type, int pos)
    {
        func.Receivers ??= MakeReceivers();
        var receiver = func.Receivers.GetValueOrDefault(type);
        if (receiver == null) return;

        if (TRACE) Console.Error.WriteLine($"  RECV: {type} for {func.Trace}");
        receiver(ReceiverObj, new MatchInfo
        {
            Text = Input[pos..Pos],
            State = StateCurr(),
            Start = pos,
        });
    }

    public Dictionary<string, Action<Receiver, MatchInfo>?> MakeReceivers()
    {
        var i = State.Count;
        var names = new List<string>();
        string? n = null;

        while (i > 0)
        {
            n = State[--i].Name;
            if (n == null) break;
            if (n.Contains('_'))
            {
                break;
            }
            // Convert chr(x) to hex representation
            if (n.Length >= 5 && n.StartsWith("chr(") && n.EndsWith(")"))
            {
                var ch = n[4..^1];
                if (ch.Length == 1)
                    n = $"x{((int)ch[0]):x}";
                else
                    n = ch;
            }
            else
            {
                var parenIdx = n.IndexOf('(');
                if (parenIdx >= 0)
                    n = n[..parenIdx];
            }
            names.Insert(0, n);
        }

        var fullName = n != null ? string.Join("__", new[] { n }.Concat(names)) : "";

        return new Dictionary<string, Action<Receiver, MatchInfo>?>
        {
            ["try"] = FindReceiverMethod($"try__{fullName}"),
            ["got"] = FindReceiverMethod($"got__{fullName}"),
            ["not"] = FindReceiverMethod($"not__{fullName}"),
        };
    }

    private Action<Receiver, MatchInfo>? FindReceiverMethod(string name)
    {
        if (_receiverMethodCache.TryGetValue(name, out var cached))
            return cached;

        var method = ReceiverObj.GetType().GetMethod(name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        Action<Receiver, MatchInfo>? result = null;
        if (method != null)
        {
            result = (receiver, info) => method.Invoke(receiver, new object[] { info });
        }

        _receiverMethodCache[name] = result;
        return result;
    }

    // ===== Parsing Primitives =====

    public bool TheEnd()
    {
        return Pos >= End || (
            StateCurr().Doc &&
            StartOfLine() &&
            Pos + 3 <= End &&
            (Input[Pos..].StartsWith("---") || Input[Pos..].StartsWith("...")) &&
            (Pos + 3 >= End || Input[Pos + 3] == ' ' || Input[Pos + 3] == '\t' ||
             Input[Pos + 3] == '\n' || Input[Pos + 3] == '\r')
        );
    }

    /// <summary>Match all sub-rules in sequence</summary>
    public override Func All(params object[] funcs)
    {
        return new Func(new System.Func<bool>(() =>
        {
            var pos = Pos;
            foreach (var f in funcs)
            {
                if (f == null)
                    throw new Exception("Missing function in All group");

                bool result;
                if (f is Func func)
                    result = Call(func);
                else if (f is object?[] arr)
                    result = Call(arr);
                else
                    throw new Exception($"Bad type in All: {f.GetType()}");

                if (!result)
                {
                    Pos = pos;
                    return false;
                }
            }
            return true;
        }), "all");
    }

    /// <summary>Match any of the sub-rules (first match wins)</summary>
    public override Func Any(params object[] funcs)
    {
        return new Func(new System.Func<bool>(() =>
        {
            foreach (var f in funcs)
            {
                bool result;
                if (f is Func func)
                    result = Call(func);
                else if (f is object?[] arr)
                    result = Call(arr);
                else
                    throw new Exception($"Bad type in Any: {f.GetType()}");

                if (result)
                    return true;
            }
            return false;
        }), "any");
    }

    /// <summary>Optional match (always succeeds)</summary>
    public override Func May(object func)
    {
        return new Func(new System.Func<bool>(() =>
        {
            if (func is Func f)
                Call(f);
            else if (func is object?[] arr)
                Call(arr);
            return true;
        }), "may");
    }

    /// <summary>Repeat a rule min to max times</summary>
    public override Func Rep(int min, int? max, object func)
    {
        return new Func(new System.Func<bool>(() =>
        {
            if (max.HasValue && max.Value < 0) return false;
            int count = 0;
            int pos = Pos;
            int posStart = pos;
            while (!max.HasValue || count < max.Value)
            {
                bool ok;
                if (func is Func f)
                    ok = Call(f);
                else if (func is object?[] arr)
                    ok = Call(arr);
                else
                    break;

                if (!ok) break;
                if (Pos == pos) break;
                count++;
                pos = Pos;
            }
            if (count >= min && (!max.HasValue || count <= max.Value))
                return true;
            Pos = posStart;
            return false;
        }), $"rep({min},{max?.ToString() ?? "null"})");
    }

    /// <summary>Rep2 - identical to Rep but with different trace name</summary>
    public override Func Rep2(int min, int? max, object func)
    {
        var f = Rep(min, max, func);
        f.Trace = $"rep2({min},{max?.ToString() ?? "null"})";
        f.Name = "rep2";
        return f;
    }

    /// <summary>Match depending on context variable</summary>
    public override Func Case(string var_, Dictionary<string, object> map)
    {
        return new Func(new System.Func<bool>(() =>
        {
            if (!map.TryGetValue(var_, out var rule))
                throw new Exception($"Can't find '{var_}' in case map");

            if (rule is Func f)
                return Call(f);
            else if (rule is object?[] arr)
                return Call(arr);
            else
                throw new Exception($"Bad case rule type: {rule?.GetType()}");
        }), $"case({var_})");
    }

    /// <summary>Flip returns a value depending on context variable</summary>
    public override object Flip(string var_, Dictionary<string, object> map)
    {
        if (!map.TryGetValue(var_, out var value))
            throw new Exception($"Can't find '{var_}' in flip map");
        if (value is System.Func<int> fi) return fi();
        if (value is System.Func<string> fs) return fs();
        if (value is Func f) return CallNumber(f);
        return value;
    }

    private int CallNumber(Func func)
    {
        var method = FindMethod(func.Name);
        if (method == null)
            throw new Exception($"Can't find method '{func.Name}'");
        var result = method.Invoke(this, null);
        if (result is int i) return i;
        if (result is Func f) return CallNumber(f);
        throw new Exception($"Expected number from '{func.Name}', got {result?.GetType()}");
    }

    /// <summary>Match a single character</summary>
    public override Func Chr(char ch)
    {
        var name = ch switch
        {
            '\t' => "chr(\\t)",
            '\n' => "chr(\\n)",
            '\r' => "chr(\\r)",
            _ when ch < ' ' || ch > '~' => $"chr(x{(int)ch:X})",
            _ => $"chr({ch})"
        };

        return new Func(new System.Func<bool>(() =>
        {
            if (TheEnd()) return false;
            if (Pos >= End) return false;
            if (Input[Pos] == ch)
            {
                Pos++;
                return true;
            }
            return false;
        }), name);
    }

    /// <summary>Match a character in range [low..high] for codepoints above 0xFFFF</summary>
    public override Func RngHigh(int low, int high)
    {
        return new Func(new System.Func<bool>(() =>
        {
            if (TheEnd()) return false;
            if (Pos >= End) return false;
            int cp;
            if (char.IsHighSurrogate(Input[Pos]) && Pos + 1 < End && char.IsLowSurrogate(Input[Pos + 1]))
            {
                cp = char.ConvertToUtf32(Input[Pos], Input[Pos + 1]);
                if (cp >= low && cp <= high)
                {
                    Pos += 2;
                    return true;
                }
            }
            else
            {
                cp = Input[Pos];
                if (cp >= low && cp <= high)
                {
                    Pos++;
                    return true;
                }
            }
            return false;
        }), $"rng({low:X},{high:X})");
    }

    /// <summary>Match a character in range [low..high]</summary>
    public override Func Rng(char low, char high)
    {
        return new Func(new System.Func<bool>(() =>
        {
            if (TheEnd()) return false;
            if (Pos >= End) return false;
            var ch = Input[Pos];
            if (ch >= low && ch <= high)
            {
                Pos++;
                return true;
            }
            return false;
        }), $"rng({(int)low:X},{(int)high:X})");
    }

    /// <summary>Must match first rule but none of the others</summary>
    public override Func But(params object[] funcs)
    {
        return new Func(new System.Func<bool>(() =>
        {
            if (TheEnd()) return false;
            var pos1 = Pos;

            bool ok;
            if (funcs[0] is Func f0)
                ok = Call(f0);
            else if (funcs[0] is object?[] arr0)
                ok = Call(arr0);
            else
                return false;

            if (!ok) return false;
            var pos2 = Pos;
            Pos = pos1;

            for (int i = 1; i < funcs.Length; i++)
            {
                bool excludeOk;
                if (funcs[i] is Func fi)
                    excludeOk = Call(fi);
                else if (funcs[i] is object?[] arri)
                    excludeOk = Call(arri);
                else
                    continue;

                if (excludeOk)
                {
                    Pos = pos1;
                    return false;
                }
            }

            Pos = pos2;
            return true;
        }), "but");
    }

    /// <summary>Look-ahead/behind check</summary>
    public override Func Chk(string type, object expr)
    {
        return new Func(new System.Func<bool>(() =>
        {
            var pos = Pos;
            if (type == "<=") Pos = Math.Max(0, Pos - 1);

            bool ok;
            if (expr is Func f)
                ok = Call(f);
            else if (expr is object?[] arr)
                ok = Call(arr);
            else
                ok = false;

            Pos = pos;
            return type == "!" ? !ok : ok;
        }), $"chk({type})");
    }

    /// <summary>Set a state variable</summary>
    public override Func Set(string var_, object expr)
    {
        return new Func(new System.Func<bool>(() =>
        {
            object? value;
            if (expr is Func f)
            {
                // Call and get the value
                var method = FindMethod(f.Name);
                if (method != null)
                    value = method.Invoke(this, null);
                else
                    value = null;
            }
            else if (expr is object?[] arr)
            {
                value = CallAny(arr);
            }
            else if (expr is string s)
            {
                value = s;
            }
            else if (expr is int i)
            {
                value = i;
            }
            else
            {
                value = expr;
            }

            if (value is int intVal && intVal == -1) return false;

            if (value is string sv && sv == "auto-detect")
            {
                value = AutoDetect();
            }

            var state = StatePrev();
            if (state == null) return false;

            SetStateVar(state, var_, value);

            if (state.Name != "all")
            {
                var size = State.Count;
                for (int idx = 3; idx < size; idx++)
                {
                    if (idx > size - 2)
                        throw new Exception("failed to traverse state stack in 'set'");
                    state = State[^idx];
                    SetStateVar(state, var_, value);
                    if (state.Name == "s_l_block_scalar")
                        break;
                }
            }
            return true;
        }), $"set({var_})");
    }

    private void SetStateVar(ParserState state, string var_, object? value)
    {
        // Resolve lazy values
        if (value is System.Func<int> lazyInt) value = lazyInt();
        if (value is System.Func<string> lazyStr) value = lazyStr();

        switch (var_)
        {
            case "m":
                state.M = value switch
                {
                    int i => i,
                    string s when int.TryParse(s, out var v) => v,
                    _ => throw new Exception($"Can't set 'm' to {value}")
                };
                break;
            case "t":
                state.T = value?.ToString();
                break;
            default:
                throw new Exception($"Unknown state variable '{var_}'");
        }
    }

    public override Func Max(int max)
    {
        return new Func(new System.Func<bool>(() => true), "max");
    }

    public override Func Exclude(Func rule)
    {
        return new Func(new System.Func<bool>(() => true), "exclude");
    }

    public Func Add(object x, object y)
    {
        return new Func(new System.Func<bool>(() =>
        {
            // This shouldn't be called as boolean
            throw new Exception("Add should not be called as boolean");
        }), "add")
        {
            // We'll handle this specially in the grammar
        };
    }

    /// <summary>Returns a lazy int representing x + y</summary>
    public override object AddNum(int x, object y)
    {
        return new System.Func<int>(() =>
        {
            int yv = ResolveInt(y);
            return x + yv;
        });
    }

    /// <summary>Returns a lazy int representing x - y</summary>
    public override object SubNum(int x, object y)
    {
        return new System.Func<int>(() =>
        {
            int yv = ResolveInt(y);
            return x - yv;
        });
    }

    /// <summary>Returns a lazy reference to the current match text</summary>
    public override object Match()
    {
        // Return a lazy delegate; resolved at parse time by Len/Ord/ResolveString
        return new System.Func<string>(() =>
        {
            var i = State.Count - 1;
            while (i > 0 && State[i].End == null)
            {
                if (i == 1) throw new Exception("Can't find match");
                i--;
            }
            var beg = State[i].Beg;
            var end = State[i].End!.Value;
            return Input[beg..end];
        });
    }

    private string ResolveString(object str)
    {
        if (str is string s) return s;
        if (str is System.Func<string> f) return f();
        throw new Exception($"Can't resolve to string: {str}");
    }

    /// <summary>Returns a lazy int representing the length of str</summary>
    public override object Len(object str)
    {
        return new System.Func<int>(() => ResolveString(str).Length);
    }

    /// <summary>Returns a lazy int representing the ordinal digit value of str</summary>
    public override object Ord(object str)
    {
        return new System.Func<int>(() =>
        {
            var s = ResolveString(str);
            return (int)s[0] - 48;
        });
    }

    public override Func If(object test, object doIfTrue)
    {
        return new Func(new System.Func<bool>(() =>
        {
            bool testResult;
            if (test is Func tf)
                testResult = Call(tf);
            else if (test is bool tb)
                testResult = tb;
            else
                testResult = false;

            if (testResult)
            {
                if (doIfTrue is Func df)
                    Call(df);
                else if (doIfTrue is object?[] arr)
                    Call(arr);
                return true;
            }
            return false;
        }), "if");
    }

    public override Func Lt(object x, object y)
    {
        return new Func(new System.Func<bool>(() =>
        {
            int xv = ResolveInt(x);
            int yv = ResolveInt(y);
            return xv < yv;
        }), "lt");
    }

    public override Func Le(object x, object y)
    {
        return new Func(new System.Func<bool>(() =>
        {
            int xv = ResolveInt(x);
            int yv = ResolveInt(y);
            return xv <= yv;
        }), "le");
    }

    private int ResolveInt(object x)
    {
        if (x is int i) return i;
        if (x is System.Func<int> fi) return fi();
        if (x is Func f) return CallNumber(f);
        if (x is string s && int.TryParse(s, out var v)) return v;
        throw new Exception($"Can't resolve to int: {x} ({x?.GetType()})");
    }

    /// <summary>Return lazy reference to current m value</summary>
    public override object GetM()
    {
        return new System.Func<int>(() => StateCurr().M ?? 0);
    }

    /// <summary>Return lazy reference to current t value</summary>
    public override object GetT()
    {
        return new System.Func<string>(() => StateCurr().T ?? "clip");
    }

    // ===== Special grammar rules =====

    public bool StartOfLine()
    {
        return Pos == 0 || Pos >= End || Input[Pos - 1] == '\n';
    }

    public bool EndOfStream()
    {
        return Pos >= End;
    }

    public bool Empty() => true;

    public override int AutoDetectIndent(int n)
    {
        var pos = Pos;
        var inSeq = pos > 0 && (Input[pos - 1] == '-' || Input[pos - 1] == '?' || Input[pos - 1] == ':');

        var rest = Input[pos..];
        var match = Regex.Match(rest, @"^((?:\ *(?:\#.*)?\n)*)(\ *)");
        if (!match.Success)
            throw new Exception("auto_detect_indent");

        var pre = match.Groups[1].Value;
        var m = match.Groups[2].Value.Length;

        if (inSeq && pre.Length == 0)
        {
            if (n == -1) m++;
        }
        else
        {
            m -= n;
        }

        if (m < 0) m = 0;
        return m;
    }

    public int AutoDetect()
    {
        var n = StateCurr().M ?? 0;
        return auto_detect(n);
    }

    // Snake_case wrappers for GetFunc reflection lookup
    public bool start_of_line() => StartOfLine();
    public bool end_of_stream() => EndOfStream();
    public bool empty() => Empty();
    public int auto_detect(int n)
    {
        // Find the indent of the next non-empty line
        var rest = Input[Pos..];
        var match = Regex.Match(rest, @"^.*\n((?:\ *\n)*)(\ *)(.?)");
        if (!match.Success)
            return 1;

        var pre = match.Groups[1].Value;
        int m;
        if (match.Groups[3].Value.Length > 0)
        {
            m = match.Groups[2].Value.Length - n;
        }
        else
        {
            m = 0;
            while (Regex.IsMatch(pre, @"\ {" + m + "}"))
            {
                m++;
            }
            m = m - n - 1;
        }

        // Check for spaces after indent
        var totalIndent = n + m;
        if (m > 0 && Regex.IsMatch(pre, @"^.{" + totalIndent + @"}\ ", RegexOptions.Multiline))
        {
            throw new Exception("Spaces found after indent in auto-detect (5LLU)");
        }

        return m == 0 ? 1 : m;
    }

    // ===== Trace support =====
    private string TraceStart() => Environment.GetEnvironmentVariable("TRACE_START") ?? "";

    private void Trace(string type, string call, object?[]? args = null)
    {
        // Simplified trace - just write to stderr when TRACE is on
        if (!_traceOn && call != TraceStart()) return;

        var level = StateCurr().Lvl;
        var indent = new string(' ', level);
        if (level > 0)
        {
            var ls = level.ToString();
            indent = ls + indent[ls.Length..];
        }

        var input = Pos < Input.Length ? Input[Pos..] : "";
        if (input.Length > 30) input = input[..30] + "…";
        input = input.Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n");

        var callStr = args != null && args.Length > 0
            ? $"{call}({string.Join(",", args)})"
            : call;

        Console.Error.WriteLine($"{indent}{type} {callStr,-40}  {Pos,4} '{input}'");

        if (call == TraceStart())
            _traceOn = !_traceOn;
    }

    private void TraceWithArgs(string type, string call, object?[] args) =>
        Trace(type, call, args);

    private void TraceFlush()
    {
        // No-op for simplified trace
    }
}
