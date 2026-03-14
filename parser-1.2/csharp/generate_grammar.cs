// Translate Grammar.pm (Perl) to Grammar.cs (C#).
// Usage: dotnet run generate_grammar.cs <input-grammar.pm> <output-grammar.cs>

using System.Text;
using System.Text.RegularExpressions;

if (args.Length < 2)
{
    Console.Error.WriteLine("Usage: dotnet run generate_grammar.cs <input.pm> <output.cs>");
    return 1;
}

var inputPath = args[0];
var outputPath = args[1];

var content = File.ReadAllText(inputPath, Encoding.UTF8);
var rules = ParseRules(content);

var sb = new StringBuilder();
sb.AppendLine("namespace YamlParser;");
sb.AppendLine();
sb.AppendLine("/// <summary>");
sb.AppendLine("/// Generated from https://yaml.org/spec/1.2/spec.html");
sb.AppendLine("/// All 211 YAML 1.2 grammar rules.");
sb.AppendLine("/// </summary>");
sb.AppendLine("public partial class Grammar");
sb.AppendLine("{");

foreach (var (num, name, body) in rules)
{
    var prms = DetectParams(body);
    var paramTypes = GetParamTypes(prms);
    var paramStr = string.Join(", ", prms.Zip(paramTypes, (p, t) => $"{t} {p}"));

    sb.AppendLine($"    // [{num}]");

    // Special rules with custom translation
    if (name == "TOP")
    {
        sb.AppendLine($"    public Func {name}()");
        sb.AppendLine("    {");
        sb.AppendLine("        return GetFunc(\"l_yaml_stream\");");
        sb.AppendLine("    }");
        sb.AppendLine();
        continue;
    }

    if (name == "l_block_sequence")
    {
        sb.AppendLine("    public Func l_block_sequence(int n)");
        sb.AppendLine("    {");
        sb.AppendLine("        var m = AutoDetectIndent(n);");
        sb.AppendLine("        if (m == 0) return new Func(new System.Func<bool>(() => false), \"l_block_sequence\");");
        sb.AppendLine("        var nm = AddNum(n, m);");
        sb.AppendLine("        return All(");
        sb.AppendLine("            Rep(1, null,");
        sb.AppendLine("                All(");
        sb.AppendLine("                    new object?[] { GetFunc(\"s_indent\"), nm },");
        sb.AppendLine("                    new object?[] { GetFunc(\"c_l_block_seq_entry\"), nm }");
        sb.AppendLine("                ))");
        sb.AppendLine("        );");
        sb.AppendLine("    }");
        sb.AppendLine();
        continue;
    }

    if (name == "s_l_block_indented")
    {
        sb.AppendLine("    public Func s_l_block_indented(int n, string c)");
        sb.AppendLine("    {");
        sb.AppendLine("        var m = AutoDetectIndent(n);");
        sb.AppendLine("        return Any(");
        sb.AppendLine("            All(");
        sb.AppendLine("                new object?[] { GetFunc(\"s_indent\"), m },");
        sb.AppendLine("                Any(");
        sb.AppendLine("                    new object?[] { GetFunc(\"ns_l_compact_sequence\"), AddNum(n, AddNum(1, m)) },");
        sb.AppendLine("                    new object?[] { GetFunc(\"ns_l_compact_mapping\"), AddNum(n, AddNum(1, m)) }");
        sb.AppendLine("                )");
        sb.AppendLine("            ),");
        sb.AppendLine("            new object?[] { GetFunc(\"s_l_block_node\"), n, c },");
        sb.AppendLine("            All(");
        sb.AppendLine("                GetFunc(\"e_node\"),");
        sb.AppendLine("                GetFunc(\"s_l_comments\")");
        sb.AppendLine("            )");
        sb.AppendLine("        );");
        sb.AppendLine("    }");
        sb.AppendLine();
        continue;
    }

    if (name == "l_block_mapping")
    {
        sb.AppendLine("    public Func l_block_mapping(int n)");
        sb.AppendLine("    {");
        sb.AppendLine("        var m = AutoDetectIndent(n);");
        sb.AppendLine("        if (m == 0) return new Func(new System.Func<bool>(() => false), \"l_block_mapping\");");
        sb.AppendLine("        var nm = AddNum(n, m);");
        sb.AppendLine("        return All(");
        sb.AppendLine("            Rep(1, null,");
        sb.AppendLine("                All(");
        sb.AppendLine("                    new object?[] { GetFunc(\"s_indent\"), nm },");
        sb.AppendLine("                    new object?[] { GetFunc(\"ns_l_block_map_entry\"), nm }");
        sb.AppendLine("                ))");
        sb.AppendLine("        );");
        sb.AppendLine("    }");
        sb.AppendLine();
        continue;
    }

    // Generic rule translation
    var csBody = TranslateRuleBody(body, name, prms);
    sb.AppendLine($"    public Func {name}({paramStr})");
    sb.AppendLine("    {");
    sb.AppendLine($"        return {csBody};");
    sb.AppendLine("    }");
    sb.AppendLine();
}

sb.AppendLine("}");

File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
Console.WriteLine($"Generated {rules.Count} rules to {outputPath}");
return 0;

// ─── Rule Parsing ────────────────────────────────────────────────────

static List<(string Num, string Name, string Body)> ParseRules(string content)
{
    var rules = new List<(string, string, string)>();
    var pattern = new Regex(@"rule\s+'(\d+)',\s+(\w+)\s+=>\s+sub\s*\{(.*?)^\};",
        RegexOptions.Singleline | RegexOptions.Multiline);
    foreach (Match m in pattern.Matches(content))
        rules.Add((m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value));
    return rules;
}

static List<string> DetectParams(string body)
{
    Match m;
    m = Regex.Match(body, @"^\s*my\s*\(\$self\)\s*=");
    if (m.Success) return [];

    m = Regex.Match(body, @"^\s*my\s*\(\$self,\s*\$(\w+)\)\s*=");
    if (m.Success) return [m.Groups[1].Value];

    m = Regex.Match(body, @"^\s*my\s*\(\$self,\s*\$(\w+),\s*\$(\w+)\)\s*=");
    if (m.Success) return [m.Groups[1].Value, m.Groups[2].Value];

    m = Regex.Match(body, @"^\s*my\s*\(\$self,\s*\$(\w+),\s*\$(\w+),\s*\$(\w+)\)\s*=");
    if (m.Success) return [m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value];

    return [];
}

static List<string> GetParamTypes(List<string> prms)
{
    return prms.Select(p => p switch
    {
        "n" or "m" => "int",
        "c" or "t" => "string",
        _ => "int"
    }).ToList();
}

// ─── Body Translation ────────────────────────────────────────────────

static string TranslateRuleBody(string body, string name, List<string> prms)
{
    body = body.Trim();
    body = Regex.Replace(body, @"my\s*\([^)]*\)\s*=\s*@_;\s*\n?", "");
    body = Regex.Replace(body, @"\s*debug_rule\([^)]*\)\s+if\s+DEBUG;\s*\n?", "");
    body = body.Trim();
    if (body.EndsWith(';')) body = body[..^1].Trim();
    return TranslateExpr(body);
}

// ─── Expression Translation ──────────────────────────────────────────

static string TranslateExpr(string expr)
{
    expr = expr.Trim();
    while (expr.EndsWith(';')) expr = expr[..^1].Trim();

    // $self->func('name')
    var m = Regex.Match(expr, @"^\$self->func\('(\w+)'\)$");
    if (m.Success) return $"GetFunc(\"{m.Groups[1].Value}\")";

    // $self->chr(X)
    m = Regex.Match(expr, @"^\$self->chr\((.+)\)$");
    if (m.Success) return $"Chr({PerlChrToCSharp(m.Groups[1].Value.Trim())})";

    // $self->rng(X, Y)
    m = Regex.Match(expr, @"^\$self->rng\((.+?),\s*(.+?)\)$");
    if (m.Success)
    {
        var low = PerlChrToCSharp(m.Groups[1].Value.Trim());
        var high = PerlChrToCSharp(m.Groups[2].Value.Trim());
        if (low.StartsWith("0x") || high.StartsWith("0x"))
        {
            var lowInt = low.StartsWith("0x") ? low : $"(int){low}";
            var highInt = high.StartsWith("0x") ? high : $"(int){high}";
            return $"RngHigh({lowInt}, {highInt})";
        }
        return $"Rng({low}, {high})";
    }

    return TranslateComplex(expr);
}

static string TranslateComplex(string expr)
{
    expr = expr.Trim();

    // $self->method(args)
    var m = Regex.Match(expr, @"^\$self->(\w+)\((.*)\)$", RegexOptions.Singleline);
    if (m.Success) return TranslateMethodCall(m.Groups[1].Value, m.Groups[2].Value.Trim());

    // [ ... ] array form
    m = Regex.Match(expr, @"^\[\s*(.+)\s*\]$", RegexOptions.Singleline);
    if (m.Success)
    {
        var parts = SplitArgs(m.Groups[1].Value.Trim());
        var translated = parts.Select(TranslateExpr);
        return "new object?[] { " + string.Join(", ", translated) + " }";
    }

    // Plain variable $var
    m = Regex.Match(expr, @"^\$(\w+)$");
    if (m.Success) return m.Groups[1].Value;

    // String literal
    if (Regex.IsMatch(expr, "^\"[^\"]*\"$")) return expr;

    // Number
    if (Regex.IsMatch(expr, @"^-?\d+$")) return expr;

    if (expr == "undef") return "null";
    if (expr == "true") return "true";
    if (expr == "false") return "false";

    return $"/* TODO: {expr} */";
}

// ─── Method Call Translation ─────────────────────────────────────────

static string TranslateMethodCall(string method, string argsStr)
{
    switch (method)
    {
        case "func":
            return $"GetFunc(\"{argsStr.Trim().Trim('\'', '\"')}\")";

        case "chr":
            return $"Chr({PerlChrToCSharp(argsStr.Trim())})";

        case "rng":
        {
            var parts = SplitArgs(argsStr);
            var low = PerlChrToCSharp(parts[0].Trim());
            var high = PerlChrToCSharp(parts[1].Trim());
            if (low.StartsWith("0x") || high.StartsWith("0x"))
                return $"RngHigh({low}, {high})";
            return $"Rng({low}, {high})";
        }

        case "all":
        {
            var parts = SplitArgs(argsStr);
            var inner = string.Join(",\n            ", parts.Select(TranslateExpr));
            return $"All(\n            {inner}\n        )";
        }

        case "any":
        {
            var parts = SplitArgs(argsStr);
            var inner = string.Join(",\n            ", parts.Select(TranslateExpr));
            return $"Any(\n            {inner}\n        )";
        }

        case "rep":
        {
            var a = SplitArgs(argsStr);
            return $"Rep({TranslateExpr(a[0])}, {TranslateExpr(a[1])}, {TranslateExpr(a[2])})";
        }

        case "rep2":
        {
            var a = SplitArgs(argsStr);
            return $"Rep2({TranslateExpr(a[0])}, {TranslateExpr(a[1])}, {TranslateExpr(a[2])})";
        }

        case "but":
        {
            var parts = SplitArgs(argsStr);
            var inner = string.Join(",\n            ", parts.Select(TranslateExpr));
            return $"But(\n            {inner}\n        )";
        }

        case "may":
            return $"May({TranslateExpr(argsStr)})";

        case "chk":
        {
            var a = SplitArgs(argsStr);
            var typeVal = a[0].Trim().Trim('\'', '"');
            return $"Chk(\"{typeVal}\", {TranslateExpr(a[1])})";
        }

        case "case":
        {
            var a = SplitArgs(argsStr);
            return TranslateCaseMap("Case", TranslateExpr(a[0]), a[1].Trim());
        }

        case "flip":
        {
            var a = SplitArgs(argsStr);
            return TranslateCaseMap("Flip", TranslateExpr(a[0]), a[1].Trim());
        }

        case "set":
        {
            var a = SplitArgs(argsStr);
            var varName = a[0].Trim().Trim('\'', '"');
            return $"Set(\"{varName}\", {TranslateExpr(a[1])})";
        }

        case "if":
        {
            var a = SplitArgs(argsStr);
            return $"If({TranslateExpr(a[0])}, {TranslateExpr(a[1])})";
        }

        case "max":
            return $"Max({argsStr.Trim()})";

        case "exclude":
            return $"Exclude({TranslateExpr(argsStr)})";

        case "lt":
        {
            var a = SplitArgs(argsStr);
            return $"Lt({TranslateExpr(a[0])}, {TranslateExpr(a[1])})";
        }

        case "le":
        {
            var a = SplitArgs(argsStr);
            return $"Le({TranslateExpr(a[0])}, {TranslateExpr(a[1])})";
        }

        case "add":
        {
            var a = SplitArgs(argsStr);
            return $"AddNum({TranslateExpr(a[0])}, {TranslateExpr(a[1])})";
        }

        case "sub":
        {
            var a = SplitArgs(argsStr);
            return $"SubNum({TranslateExpr(a[0])}, {TranslateExpr(a[1])})";
        }

        case "len":
            return $"Len({TranslateExpr(argsStr)})";

        case "ord":
            return $"Ord({TranslateExpr(argsStr)})";

        case "m":
            return "GetM()";

        case "t":
            return "GetT()";

        case "match":
            return "Match()";

        default:
            return $"/* TODO: $self->{method}({argsStr}) */";
    }
}

// ─── Case/Flip Map Translation ───────────────────────────────────────

static string TranslateCaseMap(string funcName, string varVal, string mapStr)
{
    var entries = ParseHash(mapStr);
    var parts = entries.Select(e =>
        $"        {{ \"{e.Key}\", {TranslateExpr(e.Value)} }}");
    var inner = string.Join(",\n", parts);
    return $"{funcName}({varVal}, new Dictionary<string, object>\n    {{\n{inner}\n    }})";
}

static List<(string Key, string Value)> ParseHash(string s)
{
    s = s.Trim();
    if (s.StartsWith('{')) s = s[1..];
    if (s.EndsWith('}')) s = s[..^1];
    s = s.Trim();

    var entries = new List<(string, string)>();
    while (!string.IsNullOrEmpty(s) && s != ",")
    {
        s = s.Trim();
        var m = Regex.Match(s, @"^'([^']+)'\s*=>\s*");
        if (!m.Success) break;
        var key = m.Groups[1].Value;
        s = s[m.Length..];

        var (value, rest) = ExtractValue(s);
        entries.Add((key, value));
        s = rest.Trim();
        if (s.StartsWith(',')) s = s[1..];
    }
    return entries;
}

// ─── Value Extraction ────────────────────────────────────────────────

static (string Value, string Remaining) ExtractValue(string s)
{
    s = s.Trim();

    // String literal
    var m = Regex.Match(s, "^\"([^\"]*)\"");
    if (m.Success) return ($"\"{m.Groups[1].Value}\"", s[m.Length..]);

    // Array ref [ ... ]
    if (s.StartsWith('['))
    {
        var depth = 0;
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] == '[') depth++;
            else if (s[i] == ']') { depth--; if (depth == 0) return (s[..(i + 1)], s[(i + 1)..]); }
        }
    }

    // Method call $self->...
    if (s.StartsWith("$self->"))
    {
        var end = FindMethodEnd(s);
        return (s[..end], s[end..]);
    }

    // Number
    m = Regex.Match(s, @"^(-?\d+)");
    if (m.Success) return (m.Groups[1].Value, s[m.Length..]);

    // Variable
    m = Regex.Match(s, @"^(\$\w+)");
    if (m.Success) return (m.Groups[1].Value, s[m.Length..]);

    return (s, "");
}

static int FindMethodEnd(string s)
{
    var m = Regex.Match(s, @"^\$self->(\w+)");
    if (!m.Success) return s.Length;
    var pos = m.Length;
    if (pos >= s.Length || s[pos] != '(') return pos;

    var depth = 0;
    for (var i = pos; i < s.Length; i++)
    {
        if (s[i] == '(') depth++;
        else if (s[i] == ')') { depth--; if (depth == 0) return i + 1; }
    }
    return s.Length;
}

// ─── Argument Splitting ──────────────────────────────────────────────

static List<string> SplitArgs(string s)
{
    s = s.Trim();
    var args = new List<string>();
    var depth = 0;
    var current = new StringBuilder();

    for (var i = 0; i < s.Length; i++)
    {
        var c = s[i];
        if (c is '(' or '[' or '{')
        {
            depth++;
            current.Append(c);
        }
        else if (c is ')' or ']' or '}')
        {
            depth--;
            current.Append(c);
        }
        else if (c == ',' && depth == 0)
        {
            args.Add(current.ToString().Trim());
            current.Clear();
        }
        else if (c == '#' && depth == 0)
        {
            // Skip comment to end of line
            while (i < s.Length && s[i] != '\n') i++;
        }
        else
        {
            current.Append(c);
        }
    }

    var remainder = current.ToString().Trim();
    if (remainder.Length > 0) args.Add(remainder);
    return args;
}

// ─── Character Literal Conversion ────────────────────────────────────

static string PerlChrToCSharp(string s)
{
    s = s.Trim();

    // Handle \x{XXXX} format
    var m = Regex.Match(s, @"^""\\x\{([0-9a-fA-F]+)\}""$");
    if (m.Success)
    {
        var code = Convert.ToInt32(m.Groups[1].Value, 16);
        return code <= 0xFFFF ? $"'\\u{code:X4}'" : $"0x{code:X}";
    }

    // Handle "X" single char string
    m = Regex.Match(s, "^\"(.)\"$");
    if (m.Success) return EscapeCharLiteral(m.Groups[1].Value[0]);

    // Handle "\\X" escaped char
    m = Regex.Match(s, "^\"(.*)\"$");
    if (m.Success)
    {
        var ch = m.Groups[1].Value;
        return ch switch
        {
            "\\\\" => "'\\\\'",
            "\\'" => "'\\''",
            "\\\"" => "'\"'",
            _ => $"'{ch}'"
        };
    }

    // Handle 'X' single-quoted char
    m = Regex.Match(s, "^'(.)'$");
    if (m.Success) return EscapeCharLiteral(m.Groups[1].Value[0]);

    return s;
}

static string EscapeCharLiteral(char ch)
{
    return ch switch
    {
        '\\' => "'\\\\'",
        '\'' => "'\\''",
        '"' => "'\"'",
        '\t' => "'\\t'",
        '\n' => "'\\n'",
        '\r' => "'\\r'",
        _ => $"'{ch}'"
    };
}
