#!/usr/bin/env python3
"""
Translate Grammar.pm (Perl) to Grammar.cs (C#).
Reads the Perl grammar rules and generates equivalent C# methods.
"""

import re
import sys

def read_file(path):
    with open(path, 'r', encoding='utf-8') as f:
        return f.read()

def perl_chr_to_csharp(s):
    """Convert Perl chr literal to C# char literal."""
    s = s.strip()
    # Handle \x{XXXX} format
    m = re.match(r'^"\\x\{([0-9a-fA-F]+)\}"$', s)
    if m:
        code = int(m.group(1), 16)
        if code <= 0xFFFF:
            return f"'\\u{code:04X}'"
        else:
            # Surrogate pair territory - use a special method
            return f"0x{code:X}" # Will need RngHigh
    # Handle regular string like '"X"' or "'X'"
    m = re.match(r'^"(.)"$', s)
    if m:
        ch = m.group(1)
        if ch == '\\':
            return "'\\\\'";
        elif ch == "'":
            return "'\\''"
        elif ch == '"':
            return "'\"'"
        elif ch == '\t':
            return "'\\t'"
        elif ch == '\n':
            return "'\\n'"
        elif ch == '\r':
            return "'\\r'"
        return f"'{ch}'"
    m = re.match(r"^\"(.*)\"$", s)
    if m:
        ch = m.group(1)
        if ch == "\\\\":
            return "'\\\\'";
        if ch == "\\'":
            return "'\\''"
        if ch == '\\"':
            return "'\"'"
        return f"'{ch}'"
    m = re.match(r"^'(.)'$", s)
    if m:
        ch = m.group(1)
        if ch == "'":
            return "'\\''"
        elif ch == '\\':
            return "'\\\\'";
        return f"'{ch}'"
    return s

def parse_rules(content):
    """Parse Grammar.pm and extract rule definitions."""
    rules = []
    # Find all rule blocks
    pattern = r"rule\s+'(\d+)',\s+(\w+)\s+=>\s+sub\s*\{(.*?)^\};"
    for m in re.finditer(pattern, content, re.DOTALL | re.MULTILINE):
        num = m.group(1)
        name = m.group(2)
        body = m.group(3)
        rules.append((num, name, body))
    return rules

def translate_body(body, name):
    """Translate Perl rule body to C# method body."""
    # Remove parameter declarations
    body = re.sub(r'my\s*\(\$self(?:,\s*\$(\w+))?\)\s*=\s*@_;?\s*\n?', '', body)
    body = re.sub(r'my\s*\(\$self,\s*\$(\w+),\s*\$(\w+)\)\s*=\s*@_;?\s*\n?', '', body)
    body = re.sub(r'my\s*\(\$self,\s*\$(\w+),\s*\$(\w+),\s*\$(\w+)\)\s*=\s*@_;?\s*\n?', '', body)
    # Remove debug_rule lines
    body = re.sub(r'\s*debug_rule\([^)]*\)\s+if\s+DEBUG;\s*\n?', '', body)
    return body.strip()

def detect_params(body_orig):
    """Detect parameters from the original Perl body."""
    m = re.match(r'\s*my\s*\(\$self\)\s*=', body_orig)
    if m:
        return []
    m = re.match(r'\s*my\s*\(\$self,\s*\$(\w+)\)\s*=', body_orig)
    if m:
        return [m.group(1)]
    m = re.match(r'\s*my\s*\(\$self,\s*\$(\w+),\s*\$(\w+)\)\s*=', body_orig)
    if m:
        return [m.group(1), m.group(2)]
    m = re.match(r'\s*my\s*\(\$self,\s*\$(\w+),\s*\$(\w+),\s*\$(\w+)\)\s*=', body_orig)
    if m:
        return [m.group(1), m.group(2), m.group(3)]
    return []

def get_param_types(params, name):
    """Get C# type for each parameter."""
    types = []
    for p in params:
        if p == 'n':
            types.append('int')
        elif p == 'c':
            types.append('string')
        elif p == 't':
            types.append('string')
        elif p == 'm':
            types.append('int')
        else:
            types.append('int')  # default
    return types

def main():
    input_path = sys.argv[1]
    output_path = sys.argv[2]

    content = read_file(input_path)
    rules = parse_rules(content)

    lines = []
    lines.append("namespace YamlParser;")
    lines.append("")
    lines.append("/// <summary>")
    lines.append("/// Generated from https://yaml.org/spec/1.2/spec.html")
    lines.append("/// All 211 YAML 1.2 grammar rules.")
    lines.append("/// </summary>")
    lines.append("public partial class Grammar")
    lines.append("{")

    for num, name, body in rules:
        params = detect_params(body)
        param_types = get_param_types(params, name)
        param_str = ", ".join(f"{t} {p}" for t, p in zip(param_types, params))

        lines.append(f"    // [{num}]")

        # Handle special rules that need custom translation
        if name == "TOP":
            lines.append(f"    public Func {name}()")
            lines.append("    {")
            lines.append('        return GetFunc("l_yaml_stream");')
            lines.append("    }")
            lines.append("")
            continue

        # Special cases for rules with inline auto-detect
        if name == "l_block_sequence":
            lines.append(f"    public Func l_block_sequence(int n)")
            lines.append("    {")
            lines.append("        var m = AutoDetectIndent(n);")
            lines.append("        if (m == 0) return new Func(new System.Func<bool>(() => false), \"l_block_sequence\");")
            lines.append("        var nm = AddNum(n, m);")
            lines.append("        return All(")
            lines.append("            Rep(1, null,")
            lines.append("                All(")
            lines.append('                    new object?[] { GetFunc("s_indent"), nm },')
            lines.append('                    new object?[] { GetFunc("c_l_block_seq_entry"), nm }')
            lines.append("                ))")
            lines.append("        );")
            lines.append("    }")
            lines.append("")
            continue

        if name == "s_l_block_indented":
            lines.append(f"    public Func s_l_block_indented(int n, string c)")
            lines.append("    {")
            lines.append("        var m = AutoDetectIndent(n);")
            lines.append("        return Any(")
            lines.append("            All(")
            lines.append('                new object?[] { GetFunc("s_indent"), m },')
            lines.append("                Any(")
            lines.append('                    new object?[] { GetFunc("ns_l_compact_sequence"), AddNum(n, AddNum(1, m)) },')
            lines.append('                    new object?[] { GetFunc("ns_l_compact_mapping"), AddNum(n, AddNum(1, m)) }')
            lines.append("                )")
            lines.append("            ),")
            lines.append('            new object?[] { GetFunc("s_l_block_node"), n, c },')
            lines.append("            All(")
            lines.append('                GetFunc("e_node"),')
            lines.append('                GetFunc("s_l_comments")')
            lines.append("            )")
            lines.append("        );")
            lines.append("    }")
            lines.append("")
            continue

        if name == "l_block_mapping":
            lines.append(f"    public Func l_block_mapping(int n)")
            lines.append("    {")
            lines.append("        var m = AutoDetectIndent(n);")
            lines.append("        if (m == 0) return new Func(new System.Func<bool>(() => false), \"l_block_mapping\");")
            lines.append("        var nm = AddNum(n, m);")
            lines.append("        return All(")
            lines.append("            Rep(1, null,")
            lines.append("                All(")
            lines.append('                    new object?[] { GetFunc("s_indent"), nm },')
            lines.append('                    new object?[] { GetFunc("ns_l_block_map_entry"), nm }')
            lines.append("                ))")
            lines.append("        );")
            lines.append("    }")
            lines.append("")
            continue

        # Translate the body
        cs_body = translate_rule_body(body, name, params)

        lines.append(f"    public Func {name}({param_str})")
        lines.append("    {")
        lines.append(f"        return {cs_body};")
        lines.append("    }")
        lines.append("")

    lines.append("}")

    with open(output_path, 'w', encoding='utf-8') as f:
        f.write('\n'.join(lines))

    print(f"Generated {len(rules)} rules to {output_path}")


def translate_rule_body(body, name, params):
    """Translate a Perl rule body to C# expression."""
    body = body.strip()
    # Remove my ($self) = @_; and debug_rule
    body = re.sub(r'my\s*\([^)]*\)\s*=\s*@_;\s*\n?', '', body)
    body = re.sub(r'\s*debug_rule\([^)]*\)\s+if\s+DEBUG;\s*\n?', '', body)
    body = body.strip()

    # Remove trailing semicolon
    if body.endswith(';'):
        body = body[:-1].strip()

    return translate_expr(body)


def translate_expr(expr):
    """Recursively translate a Perl expression to C#."""
    expr = expr.strip()

    # Remove trailing semicolons
    while expr.endswith(';'):
        expr = expr[:-1].strip()

    # $self->func('name')
    m = re.match(r"^\$self->func\('(\w+)'\)$", expr)
    if m:
        return f'GetFunc("{m.group(1)}")'

    # $self->chr(X)
    m = re.match(r"^\$self->chr\((.+)\)$", expr)
    if m:
        arg = m.group(1).strip()
        return f"Chr({perl_chr_to_csharp(arg)})"

    # $self->rng(X, Y)
    m = re.match(r"^\$self->rng\((.+?),\s*(.+?)\)$", expr)
    if m:
        low = perl_chr_to_csharp(m.group(1).strip())
        high = perl_chr_to_csharp(m.group(2).strip())
        # Handle surrogate pair ranges
        if low.startswith('0x') or high.startswith('0x'):
            low_int = low if low.startswith('0x') else f"(int){low}"
            high_int = high if high.startswith('0x') else f"(int){high}"
            return f"RngHigh({low_int}, {high_int})"
        return f"Rng({low}, {high})"

    # Handle complex nested expressions by parsing them properly
    return translate_complex(expr)


def translate_complex(expr):
    """Handle complex Perl expressions."""
    expr = expr.strip()

    # Try to match method calls: $self->method(args)
    # We need a proper parser for this since args can be nested

    # $self->all(\n  ... \n)
    m = re.match(r"^\$self->(\w+)\((.*)\)$", expr, re.DOTALL)
    if m:
        method = m.group(1)
        args_str = m.group(2).strip()
        return translate_method_call(method, args_str)

    # [ $self->func('name'), arg1, arg2, ... ] - array form (func with args)
    m = re.match(r"^\[\s*(.+)\s*\]$", expr, re.DOTALL)
    if m:
        inner = m.group(1).strip()
        parts = split_args(inner)
        translated = [translate_expr(p) for p in parts]
        return "new object?[] { " + ", ".join(translated) + " }"

    # Plain variable references
    if re.match(r'^\$(\w+)$', expr):
        var = expr[1:]
        return var

    # String literal
    if re.match(r'^"[^"]*"$', expr):
        return expr  # Keep as-is, C# uses same syntax

    # Number
    if re.match(r'^-?\d+$', expr):
        return expr

    # undef
    if expr == 'undef':
        return 'null'

    # true/false
    if expr == 'true':
        return 'true'
    if expr == 'false':
        return 'false'

    # Default: return as-is with a warning comment
    return f"/* TODO: {expr} */"


def translate_method_call(method, args_str):
    """Translate a $self->method(args) call."""

    if method == 'func':
        # $self->func('name')
        name = args_str.strip().strip("'\"")
        return f'GetFunc("{name}")'

    if method == 'chr':
        return f"Chr({perl_chr_to_csharp(args_str.strip())})"

    if method == 'rng':
        parts = split_args(args_str)
        low = perl_chr_to_csharp(parts[0].strip())
        high = perl_chr_to_csharp(parts[1].strip())
        if low.startswith('0x') or high.startswith('0x'):
            return f"RngHigh({low}, {high})"
        return f"Rng({low}, {high})"

    if method == 'all':
        args = split_args(args_str)
        translated = [translate_expr(a) for a in args]
        inner = ",\n            ".join(translated)
        return f"All(\n            {inner}\n        )"

    if method == 'any':
        args = split_args(args_str)
        translated = [translate_expr(a) for a in args]
        inner = ",\n            ".join(translated)
        return f"Any(\n            {inner}\n        )"

    if method == 'rep':
        args = split_args(args_str)
        min_val = translate_expr(args[0])
        max_val = translate_expr(args[1])
        func_val = translate_expr(args[2])
        return f"Rep({min_val}, {max_val}, {func_val})"

    if method == 'rep2':
        args = split_args(args_str)
        min_val = translate_expr(args[0])
        max_val = translate_expr(args[1])
        func_val = translate_expr(args[2])
        return f"Rep2({min_val}, {max_val}, {func_val})"

    if method == 'but':
        args = split_args(args_str)
        translated = [translate_expr(a) for a in args]
        inner = ",\n            ".join(translated)
        return f"But(\n            {inner}\n        )"

    if method == 'may':
        func_val = translate_expr(args_str)
        return f"May({func_val})"

    if method == 'chk':
        args = split_args(args_str)
        type_val = args[0].strip().strip("'\"")
        expr_val = translate_expr(args[1])
        return f'Chk("{type_val}", {expr_val})'

    if method == 'case':
        args = split_args(args_str)
        var_val = translate_expr(args[0])
        map_str = args[1].strip()
        return translate_case_map(var_val, map_str)

    if method == 'flip':
        args = split_args(args_str)
        var_val = translate_expr(args[0])
        map_str = args[1].strip()
        return translate_flip_map(var_val, map_str)

    if method == 'set':
        args = split_args(args_str)
        var_name = args[0].strip().strip("'\"")
        expr_val = translate_expr(args[1])
        return f'Set("{var_name}", {expr_val})'

    if method == 'if':
        args = split_args(args_str)
        test = translate_expr(args[0])
        action = translate_expr(args[1])
        return f"If({test}, {action})"

    if method == 'max':
        return f"Max({args_str.strip()})"

    if method == 'exclude':
        func_val = translate_expr(args_str)
        return f"Exclude({func_val})"

    if method == 'lt':
        args = split_args(args_str)
        x = translate_expr(args[0])
        y = translate_expr(args[1])
        return f"Lt({x}, {y})"

    if method == 'le':
        args = split_args(args_str)
        x = translate_expr(args[0])
        y = translate_expr(args[1])
        return f"Le({x}, {y})"

    if method == 'add':
        args = split_args(args_str)
        x = translate_expr(args[0])
        y = translate_expr(args[1])
        return f"AddNum({x}, {y})"

    if method == 'sub':
        args = split_args(args_str)
        x = translate_expr(args[0])
        y = translate_expr(args[1])
        return f"SubNum({x}, {y})"

    if method == 'len':
        func_val = translate_expr(args_str)
        return f"Len({func_val})"

    if method == 'ord':
        func_val = translate_expr(args_str)
        return f"Ord({func_val})"

    if method == 'm':
        return "GetM()"

    if method == 't':
        return "GetT()"

    if method == 'match':
        return "Match()"

    # Unknown method
    return f"/* TODO: $self->{method}({args_str}) */"


def translate_case_map(var_val, map_str):
    """Translate a Perl hash to a C# Dictionary for Case()."""
    entries = parse_hash(map_str)
    parts = []
    for key, value in entries:
        cs_key = f'"{key}"'
        cs_val = translate_expr(value)
        parts.append(f'        {{ {cs_key}, {cs_val} }}')
    inner = ",\n".join(parts)
    return f"Case({var_val}, new Dictionary<string, object>\n    {{\n{inner}\n    }})"

def translate_flip_map(var_val, map_str):
    """Translate a Perl hash to a C# Dictionary for Flip()."""
    entries = parse_hash(map_str)
    parts = []
    for key, value in entries:
        cs_key = f'"{key}"'
        cs_val = translate_expr(value)
        parts.append(f'        {{ {cs_key}, {cs_val} }}')
    inner = ",\n".join(parts)
    return f"Flip({var_val}, new Dictionary<string, object>\n    {{\n{inner}\n    }})"


def parse_hash(s):
    """Parse a Perl hash literal { 'key' => value, ... }"""
    s = s.strip()
    if s.startswith('{'):
        s = s[1:]
    if s.endswith('}'):
        s = s[:-1]
    s = s.strip()

    entries = []
    while s:
        s = s.strip()
        if not s or s == ',':
            break

        # Match key
        m = re.match(r"'([^']+)'\s*=>\s*", s)
        if not m:
            break
        key = m.group(1)
        s = s[m.end():]

        # Match value (could be complex)
        value, s = extract_value(s)
        entries.append((key, value))

        s = s.strip()
        if s.startswith(','):
            s = s[1:]

    return entries


def extract_value(s):
    """Extract a value from the beginning of s, handling nested parens/brackets."""
    s = s.strip()

    # String literal
    m = re.match(r'^"([^"]*)"', s)
    if m:
        return f'"{m.group(1)}"', s[m.end():]

    # Array ref [ ... ]
    if s.startswith('['):
        depth = 0
        for i, c in enumerate(s):
            if c == '[':
                depth += 1
            elif c == ']':
                depth -= 1
                if depth == 0:
                    return s[:i+1], s[i+1:]

    # Method call $self->...
    if s.startswith('$self->'):
        # Find the end of the method call
        end = find_method_end(s)
        return s[:end], s[end:]

    # Number
    m = re.match(r'^(-?\d+)', s)
    if m:
        return m.group(1), s[m.end():]

    # Variable
    m = re.match(r'^(\$\w+)', s)
    if m:
        return m.group(1), s[m.end():]

    return s, ""


def find_method_end(s):
    """Find the end of a $self->method(...) call."""
    # Skip $self->method_name
    m = re.match(r'\$self->(\w+)', s)
    if not m:
        return len(s)
    pos = m.end()
    if pos >= len(s) or s[pos] != '(':
        return pos

    # Find matching paren
    depth = 0
    for i in range(pos, len(s)):
        if s[i] == '(':
            depth += 1
        elif s[i] == ')':
            depth -= 1
            if depth == 0:
                return i + 1
    return len(s)


def split_args(s):
    """Split comma-separated args, respecting nested parens/brackets/braces."""
    s = s.strip()
    args = []
    depth = 0
    current = []

    i = 0
    while i < len(s):
        c = s[i]
        if c in '([{':
            depth += 1
            current.append(c)
        elif c in ')]}':
            depth -= 1
            current.append(c)
        elif c == ',' and depth == 0:
            args.append(''.join(current).strip())
            current = []
        elif c == '#' and depth == 0:
            # Skip rest of line (comment)
            while i < len(s) and s[i] != '\n':
                i += 1
        else:
            current.append(c)
        i += 1

    remainder = ''.join(current).strip()
    if remainder:
        args.append(remainder)

    return args


if __name__ == '__main__':
    main()
