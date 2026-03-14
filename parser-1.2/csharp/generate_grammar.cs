// YAML Grammar Generator for C#
// Reads the YAML spec YAML file and generates Grammar.cs
// Usage: dotnet run generate_grammar.cs -- --from <spec.yaml> [--rule <top-rule>]

#:package YamlDotNet@16.3.0

using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;

// ─── Entry Point ─────────────────────────────────────────────────────

var (specFile, topRule) = ParseCliArgs(args);
var specText = File.ReadAllText(specFile, Encoding.UTF8);
var comments = GetComments(specText);
var spec = LoadSpec(specText);
var nums = BuildNums(spec);
var ruleNames = spec.Keys.Where(k => !k.StartsWith(':')).ToList();

var sb = new StringBuilder();
sb.Append(GenGrammarHead(topRule, ruleNames));
foreach (var name in ruleNames)
    sb.Append(GenRule(spec, nums, comments, name));
sb.AppendLine("}");

Console.Write(sb.ToString());
return 0;

// ─── CLI Argument Parsing ────────────────────────────────────────────

static (string File, string Rule) ParseCliArgs(string[] args)
{
    string? file = null;
    string rule = "l-yaml-stream";
    for (int i = 0; i < args.Length; i++)
    {
        if (args[i] == "--from" && i + 1 < args.Length) file = args[++i];
        else if (args[i].StartsWith("--from=")) file = args[i]["--from=".Length..];
        else if (args[i] == "--rule" && i + 1 < args.Length) rule = args[++i];
        else if (args[i].StartsWith("--rule=")) rule = args[i]["--rule=".Length..];
    }
    if (file == null) { Console.Error.WriteLine("Usage: dotnet run generate_grammar.cs -- --from <spec.yaml> [--rule <top-rule>]"); Environment.Exit(1); }
    return (file!, rule);
}

// ─── YAML Spec Loading ───────────────────────────────────────────────

static Dictionary<string, object?> LoadSpec(string specText)
{
    // YamlDotNet can't parse keys like `:002` (colon-prefixed plain scalars).
    // Quote them to work around this.
    specText = Regex.Replace(specText, @"(?m)^(:\d+):", "\"$1\":");
    var yaml = new YamlStream();
    yaml.Load(new StringReader(specText));
    var root = (YamlMappingNode)yaml.Documents[0].RootNode;
    return ConvertMapping(root);
}

static Dictionary<string, object?> ConvertMapping(YamlMappingNode node)
{
    var result = new Dictionary<string, object?>();
    foreach (var (key, value) in node.Children)
        result[((YamlScalarNode)key).Value!] = ConvertNode(value);
    return result;
}

static object? ConvertNode(YamlNode node)
{
    if (node is YamlScalarNode s)
    {
        // Only treat explicit null keyword as null, not '~' (which is a valid char in the grammar)
        if (s.Value is null or "null") return null;
        return s.Value;
    }
    if (node is YamlSequenceNode seq) return seq.Children.Select(ConvertNode).ToList();
    if (node is YamlMappingNode map) return ConvertMapping(map);
    return null;
}

// ─── Comment Extraction ──────────────────────────────────────────────

static Dictionary<string, string> GetComments(string specText)
{
    var comments = new Dictionary<string, string>();
    string? currentNum = null;
    string currentComment = "";

    foreach (var line in specText.Split('\n'))
    {
        var m = Regex.Match(line, @"^:(\d+):.*");
        if (m.Success)
        {
            if (currentNum != null) comments[currentNum] = currentComment;
            currentNum = m.Groups[1].Value;
            currentComment = $"    // [{currentNum}]\n";
        }
        else if (currentNum != null && line.StartsWith("# "))
        {
            var text = line[2..].Replace("\\", "\\\\");
            currentComment += $"    // {text}\n";
        }
        else if (currentNum != null)
        {
            comments[currentNum] = currentComment;
            currentNum = null;
        }
    }
    if (currentNum != null) comments[currentNum] = currentComment;
    return comments;
}

// ─── Build Nums Map ──────────────────────────────────────────────────

static Dictionary<string, string> BuildNums(Dictionary<string, object?> spec)
{
    var nums = new Dictionary<string, string>();
    foreach (var (k, v) in spec)
        if (k.StartsWith(':') && v is string name)
            nums[name] = k;
    return nums;
}

// ─── Code Generation State ───────────────────────────────────────────

// (GenState class at end of file)

// ─── Grammar Head ────────────────────────────────────────────────────

static string GenGrammarHead(string topRule, List<string> ruleNames)
{
    var name = RuleName(topRule);
    var sb = new StringBuilder();
    sb.AppendLine("namespace YamlParser;");
    sb.AppendLine();
    sb.AppendLine("/// <summary>");
    sb.AppendLine("/// Generated from https://yaml.org/spec/1.2/spec.html");
    sb.AppendLine("/// All 211 YAML 1.2 grammar rules.");
    sb.AppendLine("/// </summary>");
    sb.AppendLine("public abstract partial class Grammar");
    sb.AppendLine("{");
    sb.AppendLine("    // Abstract methods implemented by Parser");
    sb.AppendLine("    public abstract Func GetFunc(string name);");
    sb.AppendLine("    public abstract Func All(params object[] funcs);");
    sb.AppendLine("    public abstract Func Any(params object[] funcs);");
    sb.AppendLine("    public abstract Func May(object func);");
    sb.AppendLine("    public abstract Func Rep(int min, int? max, object func);");
    sb.AppendLine("    public abstract Func Rep2(int min, int? max, object func);");
    sb.AppendLine("    public abstract Func Chr(char ch);");
    sb.AppendLine("    public abstract Func Rng(char low, char high);");
    sb.AppendLine("    public abstract Func RngHigh(int low, int high);");
    sb.AppendLine("    public abstract Func But(params object[] funcs);");
    sb.AppendLine("    public abstract Func Chk(string type, object expr);");
    sb.AppendLine("    public abstract Func Case(string var_, Dictionary<string, object> map);");
    sb.AppendLine("    public abstract object Flip(string var_, Dictionary<string, object> map);");
    sb.AppendLine("    public abstract Func Set(string var_, object expr);");
    sb.AppendLine("    public abstract Func If(object test, object doIfTrue);");
    sb.AppendLine("    public abstract Func Max(int max);");
    sb.AppendLine("    public abstract Func Exclude(Func rule);");
    sb.AppendLine("    public abstract Func Lt(object x, object y);");
    sb.AppendLine("    public abstract Func Le(object x, object y);");
    sb.AppendLine("    public abstract object AddNum(int x, object y);");
    sb.AppendLine("    public abstract object SubNum(int x, object y);");
    sb.AppendLine("    public abstract object GetM();");
    sb.AppendLine("    public abstract object GetT();");
    sb.AppendLine("    public abstract int AutoDetectIndent(int n);");
    sb.AppendLine("    public abstract object Match();");
    sb.AppendLine("    public abstract object Len(object str);");
    sb.AppendLine("    public abstract object Ord(object str);");
    sb.AppendLine();
    sb.AppendLine($"    // [000]");
    sb.AppendLine($"    public Func TOP()");
    sb.AppendLine("    {");
    sb.AppendLine($"        return GetFunc(\"{name}\");");
    sb.AppendLine("    }");
    sb.AppendLine();
    return sb.ToString();
}

// ─── Rule Generation ─────────────────────────────────────────────────

static string GenRule(Dictionary<string, object?> spec, Dictionary<string, string> nums,
    Dictionary<string, string> comments, string name)
{
    var rule = spec[name];
    var state = new GenState();

    // Get rule number
    if (nums.TryGetValue(name, out var numKey))
        state.Num = numKey[1..]; // strip leading ':'

    // Get args
    var ruleArgs = GetRuleArgs(rule);
    state.Args = ruleArgs;

    // Handle (->m) and (m>0)
    string setM = "";
    if (rule is Dictionary<string, object?> ruleMap)
    {
        if (ruleMap.ContainsKey("(->m)"))
        {
            setM = "let";
            rule = new Dictionary<string, object?>(ruleMap);
            ((Dictionary<string, object?>)rule).Remove("(->m)");
        }
        else if (ruleMap.ContainsKey("(m>0)"))
        {
            setM = "if-let";
            rule = new Dictionary<string, object?>(ruleMap);
            ((Dictionary<string, object?>)rule).Remove("(m>0)");
        }

        // Remove (...) from rule
        if (rule is Dictionary<string, object?> rm && rm.ContainsKey("(...)"))
        {
            rule = new Dictionary<string, object?>(rm);
            ((Dictionary<string, object?>)rule).Remove("(...)");
        }
    }
    state.SetM = setM;

    var ruleName = RuleName(name);
    var paramStr = GenParamStr(ruleArgs);
    var comment = comments.GetValueOrDefault(state.Num, $"    // [{state.Num}]\n");
    var body = Gen(rule, state);

    var sb = new StringBuilder();
    sb.Append(comment);

    // Special return types for delegation/value rules
    var returnType = GetReturnType(name, rule);

    if (setM == "if-let")
    {
        // (m>0): auto-detect-indent → m must be > 0 or return false
        sb.AppendLine($"    public {returnType} {ruleName}({paramStr})");
        sb.AppendLine("    {");
        sb.AppendLine("        var m = AutoDetectIndent(n);");
        sb.AppendLine($"        if (m == 0) return new Func(new System.Func<bool>(() => false), \"{ruleName}\");");
        sb.AppendLine($"        var nm = AddNum(n, m);");
        sb.AppendLine($"        return {RewriteAutoDetectBody(body, "nm")};");
        sb.AppendLine("    }");
    }
    else if (setM == "let")
    {
        // (->m): auto-detect-indent → m is set, can be 0
        sb.AppendLine($"    public {returnType} {ruleName}({paramStr})");
        sb.AppendLine("    {");
        sb.AppendLine("        var m = AutoDetectIndent(n);");
        sb.AppendLine($"        return {body};");
        sb.AppendLine("    }");
    }
    else
    {
        sb.AppendLine($"    public {returnType} {ruleName}({paramStr})");
        sb.AppendLine("    {");
        sb.AppendLine($"        return {body};");
        sb.AppendLine("    }");
    }
    sb.AppendLine();
    return sb.ToString();
}

static string GetReturnType(string name, object? rule)
{
    // These rules return values (not Func) - they are delegation/value rules
    if (name == "in-flow" || name == "seq-spaces")
        return "object";
    if (name == "ns-flow-yaml-content" || name == "s-block-line-prefix")
        return "object";
    return "Func";
}

static string RewriteAutoDetectBody(string body, string nmVar)
{
    // For (m>0) rules, the body uses AddNum(n, m) which we precompute as nm
    return body.Replace("AddNum(n, m)", nmVar);
}

static List<string> GetRuleArgs(object? rule)
{
    if (rule is not Dictionary<string, object?> map) return [];
    if (!map.TryGetValue("(...)", out var argsVal)) return [];
    if (argsVal is string s) return [s];
    if (argsVal is List<object?> list) return list.Select(x => x?.ToString() ?? "").ToList();
    return [];
}

static string GenParamStr(List<string> args)
{
    if (args.Count == 0) return "";
    return string.Join(", ", args.Select(a => a switch
    {
        "n" or "m" => $"int {a}",
        "c" or "t" => $"string {a}",
        _ => $"int {a}"
    }));
}

// ─── Recursive Code Generation ───────────────────────────────────────

static string Gen(object? rule, GenState state)
{
    return rule switch
    {
        Dictionary<string, object?> map => GenFromHash(map, state),
        List<object?> list => GenFromArray(list, state),
        string s => GenFromString(s, state),
        int n => n.ToString(),
        long n => n.ToString(),
        null => "null",
        _ => throw new Exception($"[{state.Num}] Unknown rule type: {rule.GetType()}")
    };
}

static string GenArg(object? arg, GenState state)
{
    var saved = state.IsArg;
    state.IsArg = true;
    var result = Gen(arg, state);
    state.IsArg = saved;
    return result;
}

// ─── String Rule Generation ──────────────────────────────────────────

static string GenFromString(string rule, GenState state)
{
    // Hex character: x0A → Chr('\u000A')
    var m = Regex.Match(rule, @"^x([0-9A-F]+)$");
    if (m.Success) return $"Chr({GenHexCode(m.Groups[1].Value)})";

    // Rule reference: b-char, ns-plain, etc.
    if (Regex.IsMatch(rule, @"^(?:b|c|e|l|nb|ns|s)(?:[-+][a-z0-9]+)+$"))
        return GenMethodRef(rule);

    // Single character
    if (rule.Length == 1)
    {
        return rule switch
        {
            "'" => "Chr('\\'')",
            "\\" => "Chr('\\\\')",
            "\"" => "Chr('\"')",
            "{" => "Chr('\\u007B')",
            "}" => "Chr('\\u007D')",
            "#" => "Chr('\\u0023')",
            "@" => "Chr('\\u0040')",
            "`" => "Chr('\\u0060')",
            _ when state.IsArg && (rule == "m" || rule == "t") &&
                   !state.Args.Contains(rule) && string.IsNullOrEmpty(state.SetM) =>
                rule == "m" ? "GetM()" : "GetT()",
            _ when state.IsArg => rule, // variable reference
            _ => $"Chr('{rule}')"
        };
    }

    // Context values
    if (Regex.IsMatch(rule, @"^(flow|block)-(in|out|key)$") ||
        rule is "auto-detect" or "strip" or "clip" or "keep")
        return $"\"{rule}\"";

    // Special rule names: <rule>
    m = Regex.Match(rule, @"^<(\w.+)>$");
    if (m.Success)
    {
        var name = m.Groups[1].Value;
        return name switch
        {
            "start-of-line" => "GetFunc(\"start_of_line\")",
            "end-of-stream" => "GetFunc(\"end_of_stream\")",
            "empty" => "GetFunc(\"empty\")",
            "auto-detect-indent" => "AutoDetectIndent(n)",
            _ => $"GetFunc(\"{RuleName(name)}\")"
        };
    }

    // Match reference
    if (rule == "(match)") return "Match()";

    // Number (including negative)
    if (Regex.IsMatch(rule, @"^-?\d+$")) return rule;

    throw new Exception($"[{state.Num}] Unknown string rule: {rule}");
}

// ─── Array Rule Generation ───────────────────────────────────────────

static string GenFromArray(List<object?> rule, GenState state)
{
    if (rule.Count == 2 && rule[0] is string x && x.StartsWith('x') &&
        rule[1] is string y && y.StartsWith('x'))
    {
        // Range: [x20, x7E] → Rng(...)
        var low = GenHexCode(x[1..]);
        var high = GenHexCode(y[1..]);
        if (low.StartsWith("0x") || high.StartsWith("0x"))
            return $"RngHigh({(low.StartsWith("0x") ? low : $"(int){low}")}, {(high.StartsWith("0x") ? high : $"(int){high}")})";
        return $"Rng({low}, {high})";
    }
    throw new Exception($"[{state.Num}] Unknown array rule type");
}

// ─── Hash Rule Generation ────────────────────────────────────────────

static string GenFromHash(Dictionary<string, object?> rule, GenState state)
{
    // Check for (set) in the rule (used with (if))
    var hasSet = rule.ContainsKey("(set)");
    var keys = rule.Keys.ToList();

    if (keys.Count > 1 && !hasSet)
    {
        if (!rule.ContainsKey("(...)") && !rule.ContainsKey("(->m)") && !rule.ContainsKey("(m>0)"))
            throw new Exception($"[{state.Num}] Unknown keys: {string.Join(", ", keys)}");
    }

    var key = keys[0];
    var val = rule[key];

    // Fixed repetition: ({X})
    var m = Regex.Match(key, @"^\(\{(.)\}\)$");
    if (m.Success)
    {
        var n = m.Groups[1].Value;
        return GenRep(val, n, n, state);
    }

    // Rule call with args
    if (Regex.IsMatch(key, @"^(([bcls]|n[bs]|in|seq)[-+][-+a-z0-9]+)$"))
        return GenCall(key, val, state);
    if (key == "auto-detect")
        return GenCall(key, val, state);

    return key switch
    {
        "(any)" => GenGroup(val, "Any", state),
        "(all)" => GenGroup(val, "All", state),
        "(---)" => GenGroup(val, "But", state),
        "(+++)" => GenRep(val, "1", "null", state),
        "(***)" => GenRep(val, "0", "null", state),
        "(???)" => GenRep(val, "0", "1", state),
        "(<<<)" => $"May({Gen(val, state)})",
        "(===)" => $"Chk(\"=\", {Gen(val, state)})",
        "(!==)" => $"Chk(\"!\", {Gen(val, state)})",
        "(<==)" => $"Chk(\"<=\", {Gen(val, state)})",
        "(+)" => GenBinOp("AddNum", val, state),
        "(-)" => GenBinOp("SubNum", val, state),
        "(<)" => GenBinOpArg("Lt", val, state),
        "(<=)" => GenBinOpArg("Le", val, state),
        "(case)" => GenCase(val, state),
        "(flip)" => GenFlip(val, state),
        "(max)" => $"Max({val})",
        "(if)" => GenIf(val, rule.GetValueOrDefault("(set)"), state),
        "(set)" => GenSet(val, state),
        "(ord)" => $"Ord({Gen(val, state)})",
        "(len)" => $"Len({Gen(val, state)})",
        "(exclude)" => $"Exclude({Gen(val, state)})",
        _ => throw new Exception($"[{state.Num}] Unknown hash rule key: {key}")
    };
}

// ─── Compound Generators ─────────────────────────────────────────────

static string GenGroup(object? items, string kind, GenState state)
{
    if (items is not List<object?> list) throw new Exception($"[{state.Num}] Group items not a list");
    var savedGroup = state.IsGroup;
    var savedRep = state.RepCount;
    state.IsGroup = true;
    state.RepCount = 0;
    var parts = list.Select(item => Gen(item, state)).ToList();
    state.IsGroup = savedGroup;
    state.RepCount = savedRep;
    var inner = string.Join(",\n            ", parts);
    return $"{kind}(\n            {inner}\n        )";
}

static string GenRep(object? rule, string min, string max, GenState state)
{
    state.RepCount++;
    var repFn = state.RepCount > 1 ? "Rep2" : "Rep";
    var body = Gen(rule, state);
    return $"{repFn}({min}, {max}, {body})";
}

static string GenCall(string callName, object? callArgs, GenState state)
{
    var args = callArgs is List<object?> list ? list : new List<object?> { callArgs };
    var argsStr = string.Join(", ", args.Select(a => GenArg(a, state)));
    var name = RuleName(callName);
    return $"new object?[] {{ GetFunc(\"{name}\"), {argsStr} }}";
}

static string GenBinOp(string op, object? val, GenState state)
{
    if (val is not List<object?> list || list.Count != 2)
        throw new Exception($"[{state.Num}] {op} requires 2 args");
    return $"{op}({Gen(list[0], state)}, {Gen(list[1], state)})";
}

static string GenBinOpArg(string op, object? val, GenState state)
{
    if (val is not List<object?> list || list.Count != 2)
        throw new Exception($"[{state.Num}] {op} requires 2 args");
    return $"{op}({GenArg(list[0], state)}, {GenArg(list[1], state)})";
}

static string GenCase(object? val, GenState state)
{
    if (val is not Dictionary<string, object?> map) throw new Exception($"[{state.Num}] case needs map");
    var varName = map["var"]?.ToString() ?? "";
    var entries = map.Where(kv => kv.Key != "var")
        .Select(kv => $"        {{ \"{kv.Key}\", {GenInline(kv.Value, state)} }}")
        .ToList();
    var inner = string.Join(",\n", entries);
    return $"Case({varName}, new Dictionary<string, object>\n    {{\n{inner}\n    }})";
}

static string GenFlip(object? val, GenState state)
{
    if (val is not Dictionary<string, object?> map) throw new Exception($"[{state.Num}] flip needs map");
    var varName = map["var"]?.ToString() ?? "";
    var entries = map.Where(kv => kv.Key != "var")
        .Select(kv => $"        {{ \"{kv.Key}\", {GenArg(kv.Value, state)} }}")
        .ToList();
    var inner = string.Join(",\n", entries);
    return $"Flip({varName}, new Dictionary<string, object>\n    {{\n{inner}\n    }})";
}

static string GenIf(object? condVal, object? setVal, GenState state)
{
    var cond = Gen(condVal, state);
    var set = setVal != null ? GenSet(setVal, state) : "null";
    return $"If({cond}, {set})";
}

static string GenSet(object? val, GenState state)
{
    if (val is not List<object?> list || list.Count != 2)
        throw new Exception($"[{state.Num}] set requires [var, expr]");
    var varName = list[0]?.ToString() ?? "";
    return $"Set(\"{varName}\", {Gen(list[1], state)})";
}

static string GenInline(object? val, GenState state)
{
    // Generate and collapse whitespace for inline use (case values)
    var result = Gen(val, state);
    result = Regex.Replace(result, @"\s+", " ");
    result = Regex.Replace(result, @",\s*\)", " )");
    return result;
}

// ─── Helpers ─────────────────────────────────────────────────────────

static string RuleName(string name) => name.Replace('-', '_').Replace('+', '_');

static string GenMethodRef(string name)
{
    var csName = RuleName(name);
    return csName switch
    {
        "s_indent" or "s_separate" or "s_white" or "b_char" or "b_break" or "ns_char"
        or "b_non_content" or "b_as_line_feed" or "e_node" or "e_scalar" or "s_l_comments"
        or "s_separate_in_line" or "c_forbidden" or "c_directives_end" or "c_document_end"
        or "l_comment" or "l_directive" or "l_document_suffix" or "l_document_prefix"
        or "l_any_document" or "l_explicit_document" or "l_bare_document"
        or "l_directive_document" or "l_yaml_stream"
            => $"GetFunc(\"{csName}\")",
        _ => $"GetFunc(\"{csName}\")"
    };
}

static string GenHexCode(string hex)
{
    var codePoint = Convert.ToInt32(hex, 16);
    if (codePoint > 0xFFFF) return $"0x{hex}";
    return $"'\\u{codePoint:X4}'";
}

// ─── Types (must be after top-level statements) ──────────────────────

class GenState
{
    public string Num = "0";
    public bool IsArg;
    public bool IsGroup;
    public List<string> Args = [];
    public string SetM = "";
    public int RepCount;
}
