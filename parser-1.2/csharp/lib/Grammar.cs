namespace YamlParser;

/// <summary>
/// Generated from https://yaml.org/spec/1.2/spec.html
/// All 211 YAML 1.2 grammar rules.
/// </summary>
public abstract partial class Grammar
{
    // Abstract methods implemented by Parser
    public abstract Func GetFunc(string name);
    public abstract Func All(params object[] funcs);
    public abstract Func Any(params object[] funcs);
    public abstract Func May(object func);
    public abstract Func Rep(int min, int? max, object func);
    public abstract Func Rep2(int min, int? max, object func);
    public abstract Func Chr(char ch);
    public abstract Func Rng(char low, char high);
    public abstract Func RngHigh(int low, int high);
    public abstract Func But(params object[] funcs);
    public abstract Func Chk(string type, object expr);
    public abstract Func Case(string var_, Dictionary<string, object> map);
    public abstract object Flip(string var_, Dictionary<string, object> map);
    public abstract Func Set(string var_, object expr);
    public abstract Func If(object test, object doIfTrue);
    public abstract Func Max(int max);
    public abstract Func Exclude(Func rule);
    public abstract Func Lt(object x, object y);
    public abstract Func Le(object x, object y);
    public abstract object AddNum(int x, object y);
    public abstract object SubNum(int x, object y);
    public abstract object GetM();
    public abstract object GetT();
    public abstract int AutoDetectIndent(int n);
    public abstract object Match();
    public abstract object Len(object str);
    public abstract object Ord(object str);

    // [000]
    public Func TOP()
    {
        return GetFunc("l_yaml_stream");
    }

    // [001]
    // c-printable ::=
    //   x:9 | x:A | x:D | [x:20-x:7E]
    //   | x:85 | [x:A0-x:D7FF] | [x:E000-x:FFFD]
    //   | [x:10000-x:10FFFF]
    public Func c_printable()
    {
        return Any(
            Chr('\u0009'),
            Chr('\u000A'),
            Chr('\u000D'),
            Rng('\u0020', '\u007E'),
            Chr('\u0085'),
            Rng('\u00A0', '\uD7FF'),
            Rng('\uE000', '\uFFFD'),
            RngHigh(0x010000, 0x10FFFF)
        );
    }

    // [002]
    // nb-json ::=
    //   x:9 | [x:20-x:10FFFF]
    public Func nb_json()
    {
        return Any(
            Chr('\u0009'),
            RngHigh((int)'\u0020', 0x10FFFF)
        );
    }

    // [003]
    // c-byte-order-mark ::=
    //   x:FEFF
    public Func c_byte_order_mark()
    {
        return Chr('\uFEFF');
    }

    // [004]
    // c-sequence-entry ::=
    //   '-'
    public Func c_sequence_entry()
    {
        return Chr('-');
    }

    // [005]
    // c-mapping-key ::=
    //   '?'
    public Func c_mapping_key()
    {
        return Chr('?');
    }

    // [006]
    // c-mapping-value ::=
    //   ':'
    public Func c_mapping_value()
    {
        return Chr(':');
    }

    // [007]
    // c-collect-entry ::=
    //   ','
    public Func c_collect_entry()
    {
        return Chr(',');
    }

    // [008]
    // c-sequence-start ::=
    //   '['
    public Func c_sequence_start()
    {
        return Chr('[');
    }

    // [009]
    // c-sequence-end ::=
    //   ']'
    public Func c_sequence_end()
    {
        return Chr(']');
    }

    // [010]
    // c-mapping-start ::=
    //   '{'
    public Func c_mapping_start()
    {
        return Chr('\u007B');
    }

    // [011]
    // c-mapping-end ::=
    //   '}'
    public Func c_mapping_end()
    {
        return Chr('\u007D');
    }

    // [012]
    // c-comment ::=
    //   '#'
    public Func c_comment()
    {
        return Chr('\u0023');
    }

    // [013]
    // c-anchor ::=
    //   '&'
    public Func c_anchor()
    {
        return Chr('&');
    }

    // [014]
    // c-alias ::=
    //   '*'
    public Func c_alias()
    {
        return Chr('*');
    }

    // [015]
    // c-tag ::=
    //   '!'
    public Func c_tag()
    {
        return Chr('!');
    }

    // [016]
    // c-literal ::=
    //   '|'
    public Func c_literal()
    {
        return Chr('|');
    }

    // [017]
    // c-folded ::=
    //   '>'
    public Func c_folded()
    {
        return Chr('>');
    }

    // [018]
    // c-single-quote ::=
    //   '''
    public Func c_single_quote()
    {
        return Chr('\'');
    }

    // [019]
    // c-double-quote ::=
    //   '"'
    public Func c_double_quote()
    {
        return Chr('"');
    }

    // [020]
    // c-directive ::=
    //   '%'
    public Func c_directive()
    {
        return Chr('%');
    }

    // [021]
    // c-reserved ::=
    //   '@' | '`'
    public Func c_reserved()
    {
        return Any(
            Chr('\u0040'),
            Chr('\u0060')
        );
    }

    // [022]
    // c-indicator ::=
    //   '-' | '?' | ':' | ',' | '[' | ']' | '{' | '}'
    //   | '#' | '&' | '*' | '!' | '|' | '>' | ''' | '"'
    //   | '%' | '@' | '`'
    public Func c_indicator()
    {
        return Any(
            Chr('-'),
            Chr('?'),
            Chr(':'),
            Chr(','),
            Chr('['),
            Chr(']'),
            Chr('\u007B'),
            Chr('\u007D'),
            Chr('\u0023'),
            Chr('&'),
            Chr('*'),
            Chr('!'),
            Chr('|'),
            Chr('>'),
            Chr('\''),
            Chr('"'),
            Chr('%'),
            Chr('\u0040'),
            Chr('\u0060')
        );
    }

    // [023]
    // c-flow-indicator ::=
    //   ',' | '[' | ']' | '{' | '}'
    public Func c_flow_indicator()
    {
        return Any(
            Chr(','),
            Chr('['),
            Chr(']'),
            Chr('\u007B'),
            Chr('\u007D')
        );
    }

    // [024]
    // b-line-feed ::=
    //   x:A
    public Func b_line_feed()
    {
        return Chr('\u000A');
    }

    // [025]
    // b-carriage-return ::=
    //   x:D
    public Func b_carriage_return()
    {
        return Chr('\u000D');
    }

    // [026]
    // b-char ::=
    //   b-line-feed | b-carriage-return
    public Func b_char()
    {
        return Any(
            GetFunc("b_line_feed"),
            GetFunc("b_carriage_return")
        );
    }

    // [027]
    // nb-char ::=
    //   c-printable - b-char - c-byte-order-mark
    public Func nb_char()
    {
        return But(
            GetFunc("c_printable"),
            GetFunc("b_char"),
            GetFunc("c_byte_order_mark")
        );
    }

    // [028]
    // b-break ::=
    //   ( b-carriage-return b-line-feed )
    //   | b-carriage-return
    //   | b-line-feed
    public Func b_break()
    {
        return Any(
            All(
            GetFunc("b_carriage_return"),
            GetFunc("b_line_feed")
        ),
            GetFunc("b_carriage_return"),
            GetFunc("b_line_feed")
        );
    }

    // [029]
    // b-as-line-feed ::=
    //   b-break
    public Func b_as_line_feed()
    {
        return GetFunc("b_break");
    }

    // [030]
    // b-non-content ::=
    //   b-break
    public Func b_non_content()
    {
        return GetFunc("b_break");
    }

    // [031]
    // s-space ::=
    //   x:20
    public Func s_space()
    {
        return Chr('\u0020');
    }

    // [032]
    // s-tab ::=
    //   x:9
    public Func s_tab()
    {
        return Chr('\u0009');
    }

    // [033]
    // s-white ::=
    //   s-space | s-tab
    public Func s_white()
    {
        return Any(
            GetFunc("s_space"),
            GetFunc("s_tab")
        );
    }

    // [034]
    // ns-char ::=
    //   nb-char - s-white
    public Func ns_char()
    {
        return But(
            GetFunc("nb_char"),
            GetFunc("s_white")
        );
    }

    // [035]
    // ns-dec-digit ::=
    //   [x:30-x:39]
    public Func ns_dec_digit()
    {
        return Rng('\u0030', '\u0039');
    }

    // [036]
    // ns-hex-digit ::=
    //   ns-dec-digit
    //   | [x:41-x:46] | [x:61-x:66]
    public Func ns_hex_digit()
    {
        return Any(
            GetFunc("ns_dec_digit"),
            Rng('\u0041', '\u0046'),
            Rng('\u0061', '\u0066')
        );
    }

    // [037]
    // ns-ascii-letter ::=
    //   [x:41-x:5A] | [x:61-x:7A]
    public Func ns_ascii_letter()
    {
        return Any(
            Rng('\u0041', '\u005A'),
            Rng('\u0061', '\u007A')
        );
    }

    // [038]
    // ns-word-char ::=
    //   ns-dec-digit | ns-ascii-letter | '-'
    public Func ns_word_char()
    {
        return Any(
            GetFunc("ns_dec_digit"),
            GetFunc("ns_ascii_letter"),
            Chr('-')
        );
    }

    // [039]
    // ns-uri-char ::=
    //   '%' ns-hex-digit ns-hex-digit | ns-word-char | '#'
    //   | ';' | '/' | '?' | ':' | '@' | '&' | '=' | '+' | '$' | ','
    //   | '_' | '.' | '!' | '~' | '*' | ''' | '(' | ')' | '[' | ']'
    public Func ns_uri_char()
    {
        return Any(
            All(
            Chr('%'),
            GetFunc("ns_hex_digit"),
            GetFunc("ns_hex_digit")
        ),
            GetFunc("ns_word_char"),
            Chr('\u0023'),
            Chr(';'),
            Chr('/'),
            Chr('?'),
            Chr(':'),
            Chr('\u0040'),
            Chr('&'),
            Chr('='),
            Chr('+'),
            Chr('$'),
            Chr(','),
            Chr('_'),
            Chr('.'),
            Chr('!'),
            Chr('~'),
            Chr('*'),
            Chr('\''),
            Chr('('),
            Chr(')'),
            Chr('['),
            Chr(']')
        );
    }

    // [040]
    // ns-tag-char ::=
    //   ns-uri-char - '!' - c-flow-indicator
    public Func ns_tag_char()
    {
        return But(
            GetFunc("ns_uri_char"),
            Chr('!'),
            GetFunc("c_flow_indicator")
        );
    }

    // [041]
    // c-escape ::=
    //   '\\'
    public Func c_escape()
    {
        return Chr('\\');
    }

    // [042]
    // ns-esc-null ::=
    //   '0'
    public Func ns_esc_null()
    {
        return Chr('0');
    }

    // [043]
    // ns-esc-bell ::=
    //   'a'
    public Func ns_esc_bell()
    {
        return Chr('a');
    }

    // [044]
    // ns-esc-backspace ::=
    //   'b'
    public Func ns_esc_backspace()
    {
        return Chr('b');
    }

    // [045]
    // ns-esc-horizontal-tab ::=
    //   't' | x:9
    public Func ns_esc_horizontal_tab()
    {
        return Any(
            Chr('t'),
            Chr('\u0009')
        );
    }

    // [046]
    // ns-esc-line-feed ::=
    //   'n'
    public Func ns_esc_line_feed()
    {
        return Chr('n');
    }

    // [047]
    // ns-esc-vertical-tab ::=
    //   'v'
    public Func ns_esc_vertical_tab()
    {
        return Chr('v');
    }

    // [048]
    // ns-esc-form-feed ::=
    //   'f'
    public Func ns_esc_form_feed()
    {
        return Chr('f');
    }

    // [049]
    // ns-esc-carriage-return ::=
    //   'r'
    public Func ns_esc_carriage_return()
    {
        return Chr('r');
    }

    // [050]
    // ns-esc-escape ::=
    //   'e'
    public Func ns_esc_escape()
    {
        return Chr('e');
    }

    // [051]
    // ns-esc-space ::=
    //   x:20
    public Func ns_esc_space()
    {
        return Chr('\u0020');
    }

    // [052]
    // ns-esc-double-quote ::=
    //   '"'
    public Func ns_esc_double_quote()
    {
        return Chr('"');
    }

    // [053]
    // ns-esc-slash ::=
    //   '/'
    public Func ns_esc_slash()
    {
        return Chr('/');
    }

    // [054]
    // ns-esc-backslash ::=
    //   '\\'
    public Func ns_esc_backslash()
    {
        return Chr('\\');
    }

    // [055]
    // ns-esc-next-line ::=
    //   'N'
    public Func ns_esc_next_line()
    {
        return Chr('N');
    }

    // [056]
    // ns-esc-non-breaking-space ::=
    //   '_'
    public Func ns_esc_non_breaking_space()
    {
        return Chr('_');
    }

    // [057]
    // ns-esc-line-separator ::=
    //   'L'
    public Func ns_esc_line_separator()
    {
        return Chr('L');
    }

    // [058]
    // ns-esc-paragraph-separator ::=
    //   'P'
    public Func ns_esc_paragraph_separator()
    {
        return Chr('P');
    }

    // [059]
    // ns-esc-8-bit ::=
    //   'x'
    //   ( ns-hex-digit{2} )
    public Func ns_esc_8_bit()
    {
        return All(
            Chr('x'),
            Rep(2, 2, GetFunc("ns_hex_digit"))
        );
    }

    // [060]
    // ns-esc-16-bit ::=
    //   'u'
    //   ( ns-hex-digit{4} )
    public Func ns_esc_16_bit()
    {
        return All(
            Chr('u'),
            Rep(4, 4, GetFunc("ns_hex_digit"))
        );
    }

    // [061]
    // ns-esc-32-bit ::=
    //   'U'
    //   ( ns-hex-digit{8} )
    public Func ns_esc_32_bit()
    {
        return All(
            Chr('U'),
            Rep(8, 8, GetFunc("ns_hex_digit"))
        );
    }

    // [062]
    // c-ns-esc-char ::=
    //   '\\'
    //   ( ns-esc-null | ns-esc-bell | ns-esc-backspace
    //   | ns-esc-horizontal-tab | ns-esc-line-feed
    //   | ns-esc-vertical-tab | ns-esc-form-feed
    //   | ns-esc-carriage-return | ns-esc-escape | ns-esc-space
    //   | ns-esc-double-quote | ns-esc-slash | ns-esc-backslash
    //   | ns-esc-next-line | ns-esc-non-breaking-space
    //   | ns-esc-line-separator | ns-esc-paragraph-separator
    //   | ns-esc-8-bit | ns-esc-16-bit | ns-esc-32-bit )
    public Func c_ns_esc_char()
    {
        return All(
            Chr('\\'),
            Any(
            GetFunc("ns_esc_null"),
            GetFunc("ns_esc_bell"),
            GetFunc("ns_esc_backspace"),
            GetFunc("ns_esc_horizontal_tab"),
            GetFunc("ns_esc_line_feed"),
            GetFunc("ns_esc_vertical_tab"),
            GetFunc("ns_esc_form_feed"),
            GetFunc("ns_esc_carriage_return"),
            GetFunc("ns_esc_escape"),
            GetFunc("ns_esc_space"),
            GetFunc("ns_esc_double_quote"),
            GetFunc("ns_esc_slash"),
            GetFunc("ns_esc_backslash"),
            GetFunc("ns_esc_next_line"),
            GetFunc("ns_esc_non_breaking_space"),
            GetFunc("ns_esc_line_separator"),
            GetFunc("ns_esc_paragraph_separator"),
            GetFunc("ns_esc_8_bit"),
            GetFunc("ns_esc_16_bit"),
            GetFunc("ns_esc_32_bit")
        )
        );
    }

    // [063]
    // s-indent(n) ::=
    //   s-space{n}
    public Func s_indent(int n)
    {
        return Rep(n, n, GetFunc("s_space"));
    }

    // [064]
    // s-indent(<n) ::=
    //   s-space{m} <where_m_<_n>
    public Func s_indent_lt(int n)
    {
        return May(All(
            Rep(0, null, GetFunc("s_space")),
            Lt(Len(Match()), n)
        ));
    }

    // [065]
    // s-indent(<=n) ::=
    //   s-space{m} <where_m_<=_n>
    public Func s_indent_le(int n)
    {
        return May(All(
            Rep(0, null, GetFunc("s_space")),
            Le(Len(Match()), n)
        ));
    }

    // [066]
    // s-separate-in-line ::=
    //   s-white+ | <start_of_line>
    public Func s_separate_in_line()
    {
        return Any(
            Rep(1, null, GetFunc("s_white")),
            GetFunc("start_of_line")
        );
    }

    // [067]
    // s-line-prefix(n,c) ::=
    //   ( c = block-out => s-block-line-prefix(n) )
    //   ( c = block-in => s-block-line-prefix(n) )
    //   ( c = flow-out => s-flow-line-prefix(n) )
    //   ( c = flow-in => s-flow-line-prefix(n) )
    public Func s_line_prefix(int n, string c)
    {
        return Case(c, new Dictionary<string, object>
    {
        { "block-in", new object?[] { GetFunc("s_block_line_prefix"), n } },
        { "block-out", new object?[] { GetFunc("s_block_line_prefix"), n } },
        { "flow-in", new object?[] { GetFunc("s_flow_line_prefix"), n } },
        { "flow-out", new object?[] { GetFunc("s_flow_line_prefix"), n } }
    });
    }

    // [068]
    // s-block-line-prefix(n) ::=
    //   s-indent(n)
    public object s_block_line_prefix(int n)
    {
        return new object?[] { GetFunc("s_indent"), n };
    }

    // [069]
    // s-flow-line-prefix(n) ::=
    //   s-indent(n)
    //   s-separate-in-line?
    public Func s_flow_line_prefix(int n)
    {
        return All(
            new object?[] { GetFunc("s_indent"), n },
            Rep(0, 1, GetFunc("s_separate_in_line"))
        );
    }

    // [070]
    // l-empty(n,c) ::=
    //   ( s-line-prefix(n,c) | s-indent(<n) )
    //   b-as-line-feed
    public Func l_empty(int n, string c)
    {
        return All(
            Any(
            new object?[] { GetFunc("s_line_prefix"), n, c },
            new object?[] { GetFunc("s_indent_lt"), n }
        ),
            GetFunc("b_as_line_feed")
        );
    }

    // [071]
    // b-l-trimmed(n,c) ::=
    //   b-non-content l-empty(n,c)+
    public Func b_l_trimmed(int n, string c)
    {
        return All(
            GetFunc("b_non_content"),
            Rep(1, null, new object?[] { GetFunc("l_empty"), n, c })
        );
    }

    // [072]
    // b-as-space ::=
    //   b-break
    public Func b_as_space()
    {
        return GetFunc("b_break");
    }

    // [073]
    // b-l-folded(n,c) ::=
    //   b-l-trimmed(n,c) | b-as-space
    public Func b_l_folded(int n, string c)
    {
        return Any(
            new object?[] { GetFunc("b_l_trimmed"), n, c },
            GetFunc("b_as_space")
        );
    }

    // [074]
    // s-flow-folded(n) ::=
    //   s-separate-in-line?
    //   b-l-folded(n,flow-in)
    //   s-flow-line-prefix(n)
    public Func s_flow_folded(int n)
    {
        return All(
            Rep(0, 1, GetFunc("s_separate_in_line")),
            new object?[] { GetFunc("b_l_folded"), n, "flow-in" },
            new object?[] { GetFunc("s_flow_line_prefix"), n }
        );
    }

    // [075]
    // c-nb-comment-text ::=
    //   '#' nb-char*
    public Func c_nb_comment_text()
    {
        return All(
            Chr('\u0023'),
            Rep(0, null, GetFunc("nb_char"))
        );
    }

    // [076]
    // b-comment ::=
    //   b-non-content | <end_of_file>
    public Func b_comment()
    {
        return Any(
            GetFunc("b_non_content"),
            GetFunc("end_of_stream")
        );
    }

    // [077]
    // s-b-comment ::=
    //   ( s-separate-in-line
    //   c-nb-comment-text? )?
    //   b-comment
    public Func s_b_comment()
    {
        return All(
            Rep(0, 1, All(
            GetFunc("s_separate_in_line"),
            Rep(0, 1, GetFunc("c_nb_comment_text"))
        )),
            GetFunc("b_comment")
        );
    }

    // [078]
    // l-comment ::=
    //   s-separate-in-line c-nb-comment-text?
    //   b-comment
    public Func l_comment()
    {
        return All(
            GetFunc("s_separate_in_line"),
            Rep(0, 1, GetFunc("c_nb_comment_text")),
            GetFunc("b_comment")
        );
    }

    // [079]
    // s-l-comments ::=
    //   ( s-b-comment | <start_of_line> )
    //   l-comment*
    public Func s_l_comments()
    {
        return All(
            Any(
            GetFunc("s_b_comment"),
            GetFunc("start_of_line")
        ),
            Rep(0, null, GetFunc("l_comment"))
        );
    }

    // [080]
    // s-separate(n,c) ::=
    //   ( c = block-out => s-separate-lines(n) )
    //   ( c = block-in => s-separate-lines(n) )
    //   ( c = flow-out => s-separate-lines(n) )
    //   ( c = flow-in => s-separate-lines(n) )
    //   ( c = block-key => s-separate-in-line )
    //   ( c = flow-key => s-separate-in-line )
    public Func s_separate(int n, string c)
    {
        return Case(c, new Dictionary<string, object>
    {
        { "block-in", new object?[] { GetFunc("s_separate_lines"), n } },
        { "block-key", GetFunc("s_separate_in_line") },
        { "block-out", new object?[] { GetFunc("s_separate_lines"), n } },
        { "flow-in", new object?[] { GetFunc("s_separate_lines"), n } },
        { "flow-key", GetFunc("s_separate_in_line") },
        { "flow-out", new object?[] { GetFunc("s_separate_lines"), n } }
    });
    }

    // [081]
    // s-separate-lines(n) ::=
    //   ( s-l-comments
    //   s-flow-line-prefix(n) )
    //   | s-separate-in-line
    public Func s_separate_lines(int n)
    {
        return Any(
            All(
            GetFunc("s_l_comments"),
            new object?[] { GetFunc("s_flow_line_prefix"), n }
        ),
            GetFunc("s_separate_in_line")
        );
    }

    // [082]
    // l-directive ::=
    //   '%'
    //   ( ns-yaml-directive
    //   | ns-tag-directive
    //   | ns-reserved-directive )
    //   s-l-comments
    public Func l_directive()
    {
        return All(
            Chr('%'),
            Any(
            GetFunc("ns_yaml_directive"),
            GetFunc("ns_tag_directive"),
            GetFunc("ns_reserved_directive")
        ),
            GetFunc("s_l_comments")
        );
    }

    // [083]
    // ns-reserved-directive ::=
    //   ns-directive-name
    //   ( s-separate-in-line ns-directive-parameter )*
    public Func ns_reserved_directive()
    {
        return All(
            GetFunc("ns_directive_name"),
            Rep(0, null, All(
            GetFunc("s_separate_in_line"),
            GetFunc("ns_directive_parameter")
        ))
        );
    }

    // [084]
    // ns-directive-name ::=
    //   ns-char+
    public Func ns_directive_name()
    {
        return Rep(1, null, GetFunc("ns_char"));
    }

    // [085]
    // ns-directive-parameter ::=
    //   ns-char+
    public Func ns_directive_parameter()
    {
        return Rep(1, null, GetFunc("ns_char"));
    }

    // [086]
    // ns-yaml-directive ::=
    //   'Y' 'A' 'M' 'L'
    //   s-separate-in-line ns-yaml-version
    public Func ns_yaml_directive()
    {
        return All(
            Chr('Y'),
            Chr('A'),
            Chr('M'),
            Chr('L'),
            GetFunc("s_separate_in_line"),
            GetFunc("ns_yaml_version")
        );
    }

    // [087]
    // ns-yaml-version ::=
    //   ns-dec-digit+ '.' ns-dec-digit+
    public Func ns_yaml_version()
    {
        return All(
            Rep(1, null, GetFunc("ns_dec_digit")),
            Chr('.'),
            Rep2(1, null, GetFunc("ns_dec_digit"))
        );
    }

    // [088]
    // ns-tag-directive ::=
    //   'T' 'A' 'G'
    //   s-separate-in-line c-tag-handle
    //   s-separate-in-line ns-tag-prefix
    public Func ns_tag_directive()
    {
        return All(
            Chr('T'),
            Chr('A'),
            Chr('G'),
            GetFunc("s_separate_in_line"),
            GetFunc("c_tag_handle"),
            GetFunc("s_separate_in_line"),
            GetFunc("ns_tag_prefix")
        );
    }

    // [089]
    // c-tag-handle ::=
    //   c-named-tag-handle
    //   | c-secondary-tag-handle
    //   | c-primary-tag-handle
    public Func c_tag_handle()
    {
        return Any(
            GetFunc("c_named_tag_handle"),
            GetFunc("c_secondary_tag_handle"),
            GetFunc("c_primary_tag_handle")
        );
    }

    // [090]
    // c-primary-tag-handle ::=
    //   '!'
    public Func c_primary_tag_handle()
    {
        return Chr('!');
    }

    // [091]
    // c-secondary-tag-handle ::=
    //   '!' '!'
    public Func c_secondary_tag_handle()
    {
        return All(
            Chr('!'),
            Chr('!')
        );
    }

    // [092]
    // c-named-tag-handle ::=
    //   '!' ns-word-char+ '!'
    public Func c_named_tag_handle()
    {
        return All(
            Chr('!'),
            Rep(1, null, GetFunc("ns_word_char")),
            Chr('!')
        );
    }

    // [093]
    // ns-tag-prefix ::=
    //   c-ns-local-tag-prefix | ns-global-tag-prefix
    public Func ns_tag_prefix()
    {
        return Any(
            GetFunc("c_ns_local_tag_prefix"),
            GetFunc("ns_global_tag_prefix")
        );
    }

    // [094]
    // c-ns-local-tag-prefix ::=
    //   '!' ns-uri-char*
    public Func c_ns_local_tag_prefix()
    {
        return All(
            Chr('!'),
            Rep(0, null, GetFunc("ns_uri_char"))
        );
    }

    // [095]
    // ns-global-tag-prefix ::=
    //   ns-tag-char ns-uri-char*
    public Func ns_global_tag_prefix()
    {
        return All(
            GetFunc("ns_tag_char"),
            Rep(0, null, GetFunc("ns_uri_char"))
        );
    }

    // [096]
    // c-ns-properties(n,c) ::=
    //   ( c-ns-tag-property
    //   ( s-separate(n,c) c-ns-anchor-property )? )
    //   | ( c-ns-anchor-property
    //   ( s-separate(n,c) c-ns-tag-property )? )
    public Func c_ns_properties(int n, string c)
    {
        return Any(
            All(
            GetFunc("c_ns_tag_property"),
            Rep(0, 1, All(
            new object?[] { GetFunc("s_separate"), n, c },
            GetFunc("c_ns_anchor_property")
        ))
        ),
            All(
            GetFunc("c_ns_anchor_property"),
            Rep(0, 1, All(
            new object?[] { GetFunc("s_separate"), n, c },
            GetFunc("c_ns_tag_property")
        ))
        )
        );
    }

    // [097]
    // c-ns-tag-property ::=
    //   c-verbatim-tag
    //   | c-ns-shorthand-tag
    //   | c-non-specific-tag
    public Func c_ns_tag_property()
    {
        return Any(
            GetFunc("c_verbatim_tag"),
            GetFunc("c_ns_shorthand_tag"),
            GetFunc("c_non_specific_tag")
        );
    }

    // [098]
    // c-verbatim-tag ::=
    //   '!' '<' ns-uri-char+ '>'
    public Func c_verbatim_tag()
    {
        return All(
            Chr('!'),
            Chr('<'),
            Rep(1, null, GetFunc("ns_uri_char")),
            Chr('>')
        );
    }

    // [099]
    // c-ns-shorthand-tag ::=
    //   c-tag-handle ns-tag-char+
    public Func c_ns_shorthand_tag()
    {
        return All(
            GetFunc("c_tag_handle"),
            Rep(1, null, GetFunc("ns_tag_char"))
        );
    }

    // [100]
    // c-non-specific-tag ::=
    //   '!'
    public Func c_non_specific_tag()
    {
        return Chr('!');
    }

    // [101]
    // c-ns-anchor-property ::=
    //   '&' ns-anchor-name
    public Func c_ns_anchor_property()
    {
        return All(
            Chr('&'),
            GetFunc("ns_anchor_name")
        );
    }

    // [102]
    // ns-anchor-char ::=
    //   ns-char - c-flow-indicator
    public Func ns_anchor_char()
    {
        return But(
            GetFunc("ns_char"),
            GetFunc("c_flow_indicator")
        );
    }

    // [103]
    // ns-anchor-name ::=
    //   ns-anchor-char+
    public Func ns_anchor_name()
    {
        return Rep(1, null, GetFunc("ns_anchor_char"));
    }

    // [104]
    // c-ns-alias-node ::=
    //   '*' ns-anchor-name
    public Func c_ns_alias_node()
    {
        return All(
            Chr('*'),
            GetFunc("ns_anchor_name")
        );
    }

    // [105]
    // e-scalar ::=
    //   <empty>
    public Func e_scalar()
    {
        return GetFunc("empty");
    }

    // [106]
    // e-node ::=
    //   e-scalar
    public Func e_node()
    {
        return GetFunc("e_scalar");
    }

    // [107]
    // nb-double-char ::=
    //   c-ns-esc-char | ( nb-json - '\\' - '"' )
    public Func nb_double_char()
    {
        return Any(
            GetFunc("c_ns_esc_char"),
            But(
            GetFunc("nb_json"),
            Chr('\\'),
            Chr('"')
        )
        );
    }

    // [108]
    // ns-double-char ::=
    //   nb-double-char - s-white
    public Func ns_double_char()
    {
        return But(
            GetFunc("nb_double_char"),
            GetFunc("s_white")
        );
    }

    // [109]
    // c-double-quoted(n,c) ::=
    //   '"' nb-double-text(n,c)
    //   '"'
    public Func c_double_quoted(int n, string c)
    {
        return All(
            Chr('"'),
            new object?[] { GetFunc("nb_double_text"), n, c },
            Chr('"')
        );
    }

    // [110]
    // nb-double-text(n,c) ::=
    //   ( c = flow-out => nb-double-multi-line(n) )
    //   ( c = flow-in => nb-double-multi-line(n) )
    //   ( c = block-key => nb-double-one-line )
    //   ( c = flow-key => nb-double-one-line )
    public Func nb_double_text(int n, string c)
    {
        return Case(c, new Dictionary<string, object>
    {
        { "block-key", GetFunc("nb_double_one_line") },
        { "flow-in", new object?[] { GetFunc("nb_double_multi_line"), n } },
        { "flow-key", GetFunc("nb_double_one_line") },
        { "flow-out", new object?[] { GetFunc("nb_double_multi_line"), n } }
    });
    }

    // [111]
    // nb-double-one-line ::=
    //   nb-double-char*
    public Func nb_double_one_line()
    {
        return Rep(0, null, GetFunc("nb_double_char"));
    }

    // [112]
    // s-double-escaped(n) ::=
    //   s-white* '\\'
    //   b-non-content
    //   l-empty(n,flow-in)* s-flow-line-prefix(n)
    public Func s_double_escaped(int n)
    {
        return All(
            Rep(0, null, GetFunc("s_white")),
            Chr('\\'),
            GetFunc("b_non_content"),
            Rep2(0, null, new object?[] { GetFunc("l_empty"), n, "flow-in" }),
            new object?[] { GetFunc("s_flow_line_prefix"), n }
        );
    }

    // [113]
    // s-double-break(n) ::=
    //   s-double-escaped(n) | s-flow-folded(n)
    public Func s_double_break(int n)
    {
        return Any(
            new object?[] { GetFunc("s_double_escaped"), n },
            new object?[] { GetFunc("s_flow_folded"), n }
        );
    }

    // [114]
    // nb-ns-double-in-line ::=
    //   ( s-white* ns-double-char )*
    public Func nb_ns_double_in_line()
    {
        return Rep(0, null, All(
            Rep(0, null, GetFunc("s_white")),
            GetFunc("ns_double_char")
        ));
    }

    // [115]
    // s-double-next-line(n) ::=
    //   s-double-break(n)
    //   ( ns-double-char nb-ns-double-in-line
    //   ( s-double-next-line(n) | s-white* ) )?
    public Func s_double_next_line(int n)
    {
        return All(
            new object?[] { GetFunc("s_double_break"), n },
            Rep(0, 1, All(
            GetFunc("ns_double_char"),
            GetFunc("nb_ns_double_in_line"),
            Any(
            new object?[] { GetFunc("s_double_next_line"), n },
            Rep(0, null, GetFunc("s_white"))
        )
        ))
        );
    }

    // [116]
    // nb-double-multi-line(n) ::=
    //   nb-ns-double-in-line
    //   ( s-double-next-line(n) | s-white* )
    public Func nb_double_multi_line(int n)
    {
        return All(
            GetFunc("nb_ns_double_in_line"),
            Any(
            new object?[] { GetFunc("s_double_next_line"), n },
            Rep(0, null, GetFunc("s_white"))
        )
        );
    }

    // [117]
    // c-quoted-quote ::=
    //   ''' '''
    public Func c_quoted_quote()
    {
        return All(
            Chr('\''),
            Chr('\'')
        );
    }

    // [118]
    // nb-single-char ::=
    //   c-quoted-quote | ( nb-json - ''' )
    public Func nb_single_char()
    {
        return Any(
            GetFunc("c_quoted_quote"),
            But(
            GetFunc("nb_json"),
            Chr('\'')
        )
        );
    }

    // [119]
    // ns-single-char ::=
    //   nb-single-char - s-white
    public Func ns_single_char()
    {
        return But(
            GetFunc("nb_single_char"),
            GetFunc("s_white")
        );
    }

    // [120]
    // c-single-quoted(n,c) ::=
    //   ''' nb-single-text(n,c)
    //   '''
    public Func c_single_quoted(int n, string c)
    {
        return All(
            Chr('\''),
            new object?[] { GetFunc("nb_single_text"), n, c },
            Chr('\'')
        );
    }

    // [121]
    // nb-single-text(n,c) ::=
    //   ( c = flow-out => nb-single-multi-line(n) )
    //   ( c = flow-in => nb-single-multi-line(n) )
    //   ( c = block-key => nb-single-one-line )
    //   ( c = flow-key => nb-single-one-line )
    public Func nb_single_text(int n, string c)
    {
        return Case(c, new Dictionary<string, object>
    {
        { "block-key", GetFunc("nb_single_one_line") },
        { "flow-in", new object?[] { GetFunc("nb_single_multi_line"), n } },
        { "flow-key", GetFunc("nb_single_one_line") },
        { "flow-out", new object?[] { GetFunc("nb_single_multi_line"), n } }
    });
    }

    // [122]
    // nb-single-one-line ::=
    //   nb-single-char*
    public Func nb_single_one_line()
    {
        return Rep(0, null, GetFunc("nb_single_char"));
    }

    // [123]
    // nb-ns-single-in-line ::=
    //   ( s-white* ns-single-char )*
    public Func nb_ns_single_in_line()
    {
        return Rep(0, null, All(
            Rep(0, null, GetFunc("s_white")),
            GetFunc("ns_single_char")
        ));
    }

    // [124]
    // s-single-next-line(n) ::=
    //   s-flow-folded(n)
    //   ( ns-single-char nb-ns-single-in-line
    //   ( s-single-next-line(n) | s-white* ) )?
    public Func s_single_next_line(int n)
    {
        return All(
            new object?[] { GetFunc("s_flow_folded"), n },
            Rep(0, 1, All(
            GetFunc("ns_single_char"),
            GetFunc("nb_ns_single_in_line"),
            Any(
            new object?[] { GetFunc("s_single_next_line"), n },
            Rep(0, null, GetFunc("s_white"))
        )
        ))
        );
    }

    // [125]
    // nb-single-multi-line(n) ::=
    //   nb-ns-single-in-line
    //   ( s-single-next-line(n) | s-white* )
    public Func nb_single_multi_line(int n)
    {
        return All(
            GetFunc("nb_ns_single_in_line"),
            Any(
            new object?[] { GetFunc("s_single_next_line"), n },
            Rep(0, null, GetFunc("s_white"))
        )
        );
    }

    // [126]
    // ns-plain-first(c) ::=
    //   ( ns-char - c-indicator )
    //   | ( ( '?' | ':' | '-' )
    //   <followed_by_an_ns-plain-safe(c)> )
    public Func ns_plain_first(string c)
    {
        return Any(
            But(
            GetFunc("ns_char"),
            GetFunc("c_indicator")
        ),
            All(
            Any(
            Chr('?'),
            Chr(':'),
            Chr('-')
        ),
            Chk("=", new object?[] { GetFunc("ns_plain_safe"), c })
        )
        );
    }

    // [127]
    // ns-plain-safe(c) ::=
    //   ( c = flow-out => ns-plain-safe-out )
    //   ( c = flow-in => ns-plain-safe-in )
    //   ( c = block-key => ns-plain-safe-out )
    //   ( c = flow-key => ns-plain-safe-in )
    public Func ns_plain_safe(string c)
    {
        return Case(c, new Dictionary<string, object>
    {
        { "block-key", GetFunc("ns_plain_safe_out") },
        { "flow-in", GetFunc("ns_plain_safe_in") },
        { "flow-key", GetFunc("ns_plain_safe_in") },
        { "flow-out", GetFunc("ns_plain_safe_out") }
    });
    }

    // [128]
    // ns-plain-safe-out ::=
    //   ns-char
    public Func ns_plain_safe_out()
    {
        return GetFunc("ns_char");
    }

    // [129]
    // ns-plain-safe-in ::=
    //   ns-char - c-flow-indicator
    public Func ns_plain_safe_in()
    {
        return But(
            GetFunc("ns_char"),
            GetFunc("c_flow_indicator")
        );
    }

    // [130]
    // ns-plain-char(c) ::=
    //   ( ns-plain-safe(c) - ':' - '#' )
    //   | ( <an_ns-char_preceding> '#' )
    //   | ( ':' <followed_by_an_ns-plain-safe(c)> )
    public Func ns_plain_char(string c)
    {
        return Any(
            But(
            new object?[] { GetFunc("ns_plain_safe"), c },
            Chr(':'),
            Chr('\u0023')
        ),
            All(
            Chk("<=", GetFunc("ns_char")),
            Chr('\u0023')
        ),
            All(
            Chr(':'),
            Chk("=", new object?[] { GetFunc("ns_plain_safe"), c })
        )
        );
    }

    // [131]
    // ns-plain(n,c) ::=
    //   ( c = flow-out => ns-plain-multi-line(n,c) )
    //   ( c = flow-in => ns-plain-multi-line(n,c) )
    //   ( c = block-key => ns-plain-one-line(c) )
    //   ( c = flow-key => ns-plain-one-line(c) )
    public Func ns_plain(int n, string c)
    {
        return Case(c, new Dictionary<string, object>
    {
        { "block-key", new object?[] { GetFunc("ns_plain_one_line"), c } },
        { "flow-in", new object?[] { GetFunc("ns_plain_multi_line"), n, c } },
        { "flow-key", new object?[] { GetFunc("ns_plain_one_line"), c } },
        { "flow-out", new object?[] { GetFunc("ns_plain_multi_line"), n, c } }
    });
    }

    // [132]
    // nb-ns-plain-in-line(c) ::=
    //   ( s-white*
    //   ns-plain-char(c) )*
    public Func nb_ns_plain_in_line(string c)
    {
        return Rep(0, null, All(
            Rep(0, null, GetFunc("s_white")),
            new object?[] { GetFunc("ns_plain_char"), c }
        ));
    }

    // [133]
    // ns-plain-one-line(c) ::=
    //   ns-plain-first(c)
    //   nb-ns-plain-in-line(c)
    public Func ns_plain_one_line(string c)
    {
        return All(
            new object?[] { GetFunc("ns_plain_first"), c },
            new object?[] { GetFunc("nb_ns_plain_in_line"), c }
        );
    }

    // [134]
    // s-ns-plain-next-line(n,c) ::=
    //   s-flow-folded(n)
    //   ns-plain-char(c) nb-ns-plain-in-line(c)
    public Func s_ns_plain_next_line(int n, string c)
    {
        return All(
            new object?[] { GetFunc("s_flow_folded"), n },
            new object?[] { GetFunc("ns_plain_char"), c },
            new object?[] { GetFunc("nb_ns_plain_in_line"), c }
        );
    }

    // [135]
    // ns-plain-multi-line(n,c) ::=
    //   ns-plain-one-line(c)
    //   s-ns-plain-next-line(n,c)*
    public Func ns_plain_multi_line(int n, string c)
    {
        return All(
            new object?[] { GetFunc("ns_plain_one_line"), c },
            Rep(0, null, new object?[] { GetFunc("s_ns_plain_next_line"), n, c })
        );
    }

    // [136]
    // in-flow(c) ::=
    //   ( c = flow-out => flow-in )
    //   ( c = flow-in => flow-in )
    //   ( c = block-key => flow-key )
    //   ( c = flow-key => flow-key )
    public object in_flow(string c)
    {
        return Flip(c, new Dictionary<string, object>
    {
        { "block-key", "flow-key" },
        { "flow-in", "flow-in" },
        { "flow-key", "flow-key" },
        { "flow-out", "flow-in" }
    });
    }

    // [137]
    // c-flow-sequence(n,c) ::=
    //   '[' s-separate(n,c)?
    //   ns-s-flow-seq-entries(n,in-flow(c))? ']'
    public Func c_flow_sequence(int n, string c)
    {
        return All(
            Chr('['),
            Rep(0, 1, new object?[] { GetFunc("s_separate"), n, c }),
            Rep2(0, 1, new object?[] { GetFunc("ns_s_flow_seq_entries"), n, new object?[] { GetFunc("in_flow"), c } }),
            Chr(']')
        );
    }

    // [138]
    // ns-s-flow-seq-entries(n,c) ::=
    //   ns-flow-seq-entry(n,c)
    //   s-separate(n,c)?
    //   ( ',' s-separate(n,c)?
    //   ns-s-flow-seq-entries(n,c)? )?
    public Func ns_s_flow_seq_entries(int n, string c)
    {
        return All(
            new object?[] { GetFunc("ns_flow_seq_entry"), n, c },
            Rep(0, 1, new object?[] { GetFunc("s_separate"), n, c }),
            Rep2(0, 1, All(
            Chr(','),
            Rep(0, 1, new object?[] { GetFunc("s_separate"), n, c }),
            Rep2(0, 1, new object?[] { GetFunc("ns_s_flow_seq_entries"), n, c })
        ))
        );
    }

    // [139]
    // ns-flow-seq-entry(n,c) ::=
    //   ns-flow-pair(n,c) | ns-flow-node(n,c)
    public Func ns_flow_seq_entry(int n, string c)
    {
        return Any(
            new object?[] { GetFunc("ns_flow_pair"), n, c },
            new object?[] { GetFunc("ns_flow_node"), n, c }
        );
    }

    // [140]
    // c-flow-mapping(n,c) ::=
    //   '{' s-separate(n,c)?
    //   ns-s-flow-map-entries(n,in-flow(c))? '}'
    public Func c_flow_mapping(int n, string c)
    {
        return All(
            Chr('\u007B'),
            Rep(0, 1, new object?[] { GetFunc("s_separate"), n, c }),
            Rep2(0, 1, new object?[] { GetFunc("ns_s_flow_map_entries"), n, new object?[] { GetFunc("in_flow"), c } }),
            Chr('\u007D')
        );
    }

    // [141]
    // ns-s-flow-map-entries(n,c) ::=
    //   ns-flow-map-entry(n,c)
    //   s-separate(n,c)?
    //   ( ',' s-separate(n,c)?
    //   ns-s-flow-map-entries(n,c)? )?
    public Func ns_s_flow_map_entries(int n, string c)
    {
        return All(
            new object?[] { GetFunc("ns_flow_map_entry"), n, c },
            Rep(0, 1, new object?[] { GetFunc("s_separate"), n, c }),
            Rep2(0, 1, All(
            Chr(','),
            Rep(0, 1, new object?[] { GetFunc("s_separate"), n, c }),
            Rep2(0, 1, new object?[] { GetFunc("ns_s_flow_map_entries"), n, c })
        ))
        );
    }

    // [142]
    // ns-flow-map-entry(n,c) ::=
    //   ( '?' s-separate(n,c)
    //   ns-flow-map-explicit-entry(n,c) )
    //   | ns-flow-map-implicit-entry(n,c)
    public Func ns_flow_map_entry(int n, string c)
    {
        return Any(
            All(
            Chr('?'),
            Chk("=", Any(
            GetFunc("end_of_stream"),
            GetFunc("s_white"),
            GetFunc("b_break")
        )),
            new object?[] { GetFunc("s_separate"), n, c },
            new object?[] { GetFunc("ns_flow_map_explicit_entry"), n, c }
        ),
            new object?[] { GetFunc("ns_flow_map_implicit_entry"), n, c }
        );
    }

    // [143]
    // ns-flow-map-explicit-entry(n,c) ::=
    //   ns-flow-map-implicit-entry(n,c)
    //   | ( e-node
    //   e-node )
    public Func ns_flow_map_explicit_entry(int n, string c)
    {
        return Any(
            new object?[] { GetFunc("ns_flow_map_implicit_entry"), n, c },
            All(
            GetFunc("e_node"),
            GetFunc("e_node")
        )
        );
    }

    // [144]
    // ns-flow-map-implicit-entry(n,c) ::=
    //   ns-flow-map-yaml-key-entry(n,c)
    //   | c-ns-flow-map-empty-key-entry(n,c)
    //   | c-ns-flow-map-json-key-entry(n,c)
    public Func ns_flow_map_implicit_entry(int n, string c)
    {
        return Any(
            new object?[] { GetFunc("ns_flow_map_yaml_key_entry"), n, c },
            new object?[] { GetFunc("c_ns_flow_map_empty_key_entry"), n, c },
            new object?[] { GetFunc("c_ns_flow_map_json_key_entry"), n, c }
        );
    }

    // [145]
    // ns-flow-map-yaml-key-entry(n,c) ::=
    //   ns-flow-yaml-node(n,c)
    //   ( ( s-separate(n,c)?
    //   c-ns-flow-map-separate-value(n,c) )
    //   | e-node )
    public Func ns_flow_map_yaml_key_entry(int n, string c)
    {
        return All(
            new object?[] { GetFunc("ns_flow_yaml_node"), n, c },
            Any(
            All(
            Rep(0, 1, new object?[] { GetFunc("s_separate"), n, c }),
            new object?[] { GetFunc("c_ns_flow_map_separate_value"), n, c }
        ),
            GetFunc("e_node")
        )
        );
    }

    // [146]
    // c-ns-flow-map-empty-key-entry(n,c) ::=
    //   e-node
    //   c-ns-flow-map-separate-value(n,c)
    public Func c_ns_flow_map_empty_key_entry(int n, string c)
    {
        return All(
            GetFunc("e_node"),
            new object?[] { GetFunc("c_ns_flow_map_separate_value"), n, c }
        );
    }

    // [147]
    // c-ns-flow-map-separate-value(n,c) ::=
    //   ':' <not_followed_by_an_ns-plain-safe(c)>
    //   ( ( s-separate(n,c) ns-flow-node(n,c) )
    //   | e-node )
    public Func c_ns_flow_map_separate_value(int n, string c)
    {
        return All(
            Chr(':'),
            Chk("!", new object?[] { GetFunc("ns_plain_safe"), c }),
            Any(
            All(
            new object?[] { GetFunc("s_separate"), n, c },
            new object?[] { GetFunc("ns_flow_node"), n, c }
        ),
            GetFunc("e_node")
        )
        );
    }

    // [148]
    // c-ns-flow-map-json-key-entry(n,c) ::=
    //   c-flow-json-node(n,c)
    //   ( ( s-separate(n,c)?
    //   c-ns-flow-map-adjacent-value(n,c) )
    //   | e-node )
    public Func c_ns_flow_map_json_key_entry(int n, string c)
    {
        return All(
            new object?[] { GetFunc("c_flow_json_node"), n, c },
            Any(
            All(
            Rep(0, 1, new object?[] { GetFunc("s_separate"), n, c }),
            new object?[] { GetFunc("c_ns_flow_map_adjacent_value"), n, c }
        ),
            GetFunc("e_node")
        )
        );
    }

    // [149]
    // c-ns-flow-map-adjacent-value(n,c) ::=
    //   ':' ( (
    //   s-separate(n,c)?
    //   ns-flow-node(n,c) )
    //   | e-node )
    public Func c_ns_flow_map_adjacent_value(int n, string c)
    {
        return All(
            Chr(':'),
            Any(
            All(
            Rep(0, 1, new object?[] { GetFunc("s_separate"), n, c }),
            new object?[] { GetFunc("ns_flow_node"), n, c }
        ),
            GetFunc("e_node")
        )
        );
    }

    // [150]
    // ns-flow-pair(n,c) ::=
    //   ( '?' s-separate(n,c)
    //   ns-flow-map-explicit-entry(n,c) )
    //   | ns-flow-pair-entry(n,c)
    public Func ns_flow_pair(int n, string c)
    {
        return Any(
            All(
            Chr('?'),
            Chk("=", Any(
            GetFunc("end_of_stream"),
            GetFunc("s_white"),
            GetFunc("b_break")
        )),
            new object?[] { GetFunc("s_separate"), n, c },
            new object?[] { GetFunc("ns_flow_map_explicit_entry"), n, c }
        ),
            new object?[] { GetFunc("ns_flow_pair_entry"), n, c }
        );
    }

    // [151]
    // ns-flow-pair-entry(n,c) ::=
    //   ns-flow-pair-yaml-key-entry(n,c)
    //   | c-ns-flow-map-empty-key-entry(n,c)
    //   | c-ns-flow-pair-json-key-entry(n,c)
    public Func ns_flow_pair_entry(int n, string c)
    {
        return Any(
            new object?[] { GetFunc("ns_flow_pair_yaml_key_entry"), n, c },
            new object?[] { GetFunc("c_ns_flow_map_empty_key_entry"), n, c },
            new object?[] { GetFunc("c_ns_flow_pair_json_key_entry"), n, c }
        );
    }

    // [152]
    // ns-flow-pair-yaml-key-entry(n,c) ::=
    //   ns-s-implicit-yaml-key(flow-key)
    //   c-ns-flow-map-separate-value(n,c)
    public Func ns_flow_pair_yaml_key_entry(int n, string c)
    {
        return All(
            new object?[] { GetFunc("ns_s_implicit_yaml_key"), "flow-key" },
            new object?[] { GetFunc("c_ns_flow_map_separate_value"), n, c }
        );
    }

    // [153]
    // c-ns-flow-pair-json-key-entry(n,c) ::=
    //   c-s-implicit-json-key(flow-key)
    //   c-ns-flow-map-adjacent-value(n,c)
    public Func c_ns_flow_pair_json_key_entry(int n, string c)
    {
        return All(
            new object?[] { GetFunc("c_s_implicit_json_key"), "flow-key" },
            new object?[] { GetFunc("c_ns_flow_map_adjacent_value"), n, c }
        );
    }

    // [154]
    // ns-s-implicit-yaml-key(c) ::=
    //   ns-flow-yaml-node(n/a,c)
    //   s-separate-in-line?
    //   <at_most_1024_characters_altogether>
    public Func ns_s_implicit_yaml_key(string c)
    {
        return All(
            Max(1024),
            new object?[] { GetFunc("ns_flow_yaml_node"), null, c },
            Rep(0, 1, GetFunc("s_separate_in_line"))
        );
    }

    // [155]
    // c-s-implicit-json-key(c) ::=
    //   c-flow-json-node(n/a,c)
    //   s-separate-in-line?
    //   <at_most_1024_characters_altogether>
    public Func c_s_implicit_json_key(string c)
    {
        return All(
            Max(1024),
            new object?[] { GetFunc("c_flow_json_node"), null, c },
            Rep(0, 1, GetFunc("s_separate_in_line"))
        );
    }

    // [156]
    // ns-flow-yaml-content(n,c) ::=
    //   ns-plain(n,c)
    public object ns_flow_yaml_content(int n, string c)
    {
        return new object?[] { GetFunc("ns_plain"), n, c };
    }

    // [157]
    // c-flow-json-content(n,c) ::=
    //   c-flow-sequence(n,c) | c-flow-mapping(n,c)
    //   | c-single-quoted(n,c) | c-double-quoted(n,c)
    public Func c_flow_json_content(int n, string c)
    {
        return Any(
            new object?[] { GetFunc("c_flow_sequence"), n, c },
            new object?[] { GetFunc("c_flow_mapping"), n, c },
            new object?[] { GetFunc("c_single_quoted"), n, c },
            new object?[] { GetFunc("c_double_quoted"), n, c }
        );
    }

    // [158]
    // ns-flow-content(n,c) ::=
    //   ns-flow-yaml-content(n,c) | c-flow-json-content(n,c)
    public Func ns_flow_content(int n, string c)
    {
        return Any(
            new object?[] { GetFunc("ns_flow_yaml_content"), n, c },
            new object?[] { GetFunc("c_flow_json_content"), n, c }
        );
    }

    // [159]
    // ns-flow-yaml-node(n,c) ::=
    //   c-ns-alias-node
    //   | ns-flow-yaml-content(n,c)
    //   | ( c-ns-properties(n,c)
    //   ( ( s-separate(n,c)
    //   ns-flow-yaml-content(n,c) )
    //   | e-scalar ) )
    public Func ns_flow_yaml_node(int n, string c)
    {
        return Any(
            GetFunc("c_ns_alias_node"),
            new object?[] { GetFunc("ns_flow_yaml_content"), n, c },
            All(
            new object?[] { GetFunc("c_ns_properties"), n, c },
            Any(
            All(
            new object?[] { GetFunc("s_separate"), n, c },
            new object?[] { GetFunc("ns_flow_content"), n, c }
        ),
            GetFunc("e_scalar")
        )
        )
        );
    }

    // [160]
    // c-flow-json-node(n,c) ::=
    //   ( c-ns-properties(n,c)
    //   s-separate(n,c) )?
    //   c-flow-json-content(n,c)
    public Func c_flow_json_node(int n, string c)
    {
        return All(
            Rep(0, 1, All(
            new object?[] { GetFunc("c_ns_properties"), n, c },
            new object?[] { GetFunc("s_separate"), n, c }
        )),
            new object?[] { GetFunc("c_flow_json_content"), n, c }
        );
    }

    // [161]
    // ns-flow-node(n,c) ::=
    //   c-ns-alias-node
    //   | ns-flow-content(n,c)
    //   | ( c-ns-properties(n,c)
    //   ( ( s-separate(n,c)
    //   ns-flow-content(n,c) )
    //   | e-scalar ) )
    public Func ns_flow_node(int n, string c)
    {
        return Any(
            GetFunc("c_ns_alias_node"),
            new object?[] { GetFunc("ns_flow_content"), n, c },
            All(
            new object?[] { GetFunc("c_ns_properties"), n, c },
            Any(
            All(
            new object?[] { GetFunc("s_separate"), n, c },
            new object?[] { GetFunc("ns_flow_content"), n, c }
        ),
            GetFunc("e_scalar")
        )
        )
        );
    }

    // [162]
    // c-b-block-header(m,t) ::=
    //   ( ( c-indentation-indicator(m)
    //   c-chomping-indicator(t) )
    //   | ( c-chomping-indicator(t)
    //   c-indentation-indicator(m) ) )
    //   s-b-comment
    public Func c_b_block_header(int n)
    {
        return All(
            Any(
            All(
            new object?[] { GetFunc("c_indentation_indicator"), n },
            GetFunc("c_chomping_indicator"),
            Chk("=", Any(
            GetFunc("end_of_stream"),
            GetFunc("s_white"),
            GetFunc("b_break")
        ))
        ),
            All(
            GetFunc("c_chomping_indicator"),
            new object?[] { GetFunc("c_indentation_indicator"), n },
            Chk("=", Any(
            GetFunc("end_of_stream"),
            GetFunc("s_white"),
            GetFunc("b_break")
        ))
        )
        ),
            GetFunc("s_b_comment")
        );
    }

    // [163]
    // c-indentation-indicator(m) ::=
    //   ( ns-dec-digit => m = ns-dec-digit - x:30 )
    //   ( <empty> => m = auto-detect() )
    public Func c_indentation_indicator(int n)
    {
        return Any(
            If(Rng('\u0031', '\u0039'), Set("m", Ord(Match()))),
            If(GetFunc("empty"), Set("m", new object?[] { GetFunc("auto_detect"), n }))
        );
    }

    // [164]
    // c-chomping-indicator(t) ::=
    //   ( '-' => t = strip )
    //   ( '+' => t = keep )
    //   ( <empty> => t = clip )
    public Func c_chomping_indicator()
    {
        return Any(
            If(Chr('-'), Set("t", "strip")),
            If(Chr('+'), Set("t", "keep")),
            If(GetFunc("empty"), Set("t", "clip"))
        );
    }

    // [165]
    // b-chomped-last(t) ::=
    //   ( t = strip => b-non-content | <end_of_file> )
    //   ( t = clip => b-as-line-feed | <end_of_file> )
    //   ( t = keep => b-as-line-feed | <end_of_file> )
    public Func b_chomped_last(string t)
    {
        return Case(t, new Dictionary<string, object>
    {
        { "clip", Any( GetFunc("b_as_line_feed"), GetFunc("end_of_stream") ) },
        { "keep", Any( GetFunc("b_as_line_feed"), GetFunc("end_of_stream") ) },
        { "strip", Any( GetFunc("b_non_content"), GetFunc("end_of_stream") ) }
    });
    }

    // [166]
    // l-chomped-empty(n,t) ::=
    //   ( t = strip => l-strip-empty(n) )
    //   ( t = clip => l-strip-empty(n) )
    //   ( t = keep => l-keep-empty(n) )
    public Func l_chomped_empty(int n, string t)
    {
        return Case(t, new Dictionary<string, object>
    {
        { "clip", new object?[] { GetFunc("l_strip_empty"), n } },
        { "keep", new object?[] { GetFunc("l_keep_empty"), n } },
        { "strip", new object?[] { GetFunc("l_strip_empty"), n } }
    });
    }

    // [167]
    // l-strip-empty(n) ::=
    //   ( s-indent(<=n) b-non-content )*
    //   l-trail-comments(n)?
    public Func l_strip_empty(int n)
    {
        return All(
            Rep(0, null, All(
            new object?[] { GetFunc("s_indent_le"), n },
            GetFunc("b_non_content")
        )),
            Rep2(0, 1, new object?[] { GetFunc("l_trail_comments"), n })
        );
    }

    // [168]
    // l-keep-empty(n) ::=
    //   l-empty(n,block-in)*
    //   l-trail-comments(n)?
    public Func l_keep_empty(int n)
    {
        return All(
            Rep(0, null, new object?[] { GetFunc("l_empty"), n, "block-in" }),
            Rep2(0, 1, new object?[] { GetFunc("l_trail_comments"), n })
        );
    }

    // [169]
    // l-trail-comments(n) ::=
    //   s-indent(<n)
    //   c-nb-comment-text b-comment
    //   l-comment*
    public Func l_trail_comments(int n)
    {
        return All(
            new object?[] { GetFunc("s_indent_lt"), n },
            GetFunc("c_nb_comment_text"),
            GetFunc("b_comment"),
            Rep(0, null, GetFunc("l_comment"))
        );
    }

    // [170]
    // c-l+literal(n) ::=
    //   '|' c-b-block-header(m,t)
    //   l-literal-content(n+m,t)
    public Func c_l_literal(int n)
    {
        return All(
            Chr('|'),
            new object?[] { GetFunc("c_b_block_header"), n },
            new object?[] { GetFunc("l_literal_content"), AddNum(n, GetM()), GetT() }
        );
    }

    // [171]
    // l-nb-literal-text(n) ::=
    //   l-empty(n,block-in)*
    //   s-indent(n) nb-char+
    public Func l_nb_literal_text(int n)
    {
        return All(
            Rep(0, null, new object?[] { GetFunc("l_empty"), n, "block-in" }),
            new object?[] { GetFunc("s_indent"), n },
            Rep2(1, null, GetFunc("nb_char"))
        );
    }

    // [172]
    // b-nb-literal-next(n) ::=
    //   b-as-line-feed
    //   l-nb-literal-text(n)
    public Func b_nb_literal_next(int n)
    {
        return All(
            GetFunc("b_as_line_feed"),
            new object?[] { GetFunc("l_nb_literal_text"), n }
        );
    }

    // [173]
    // l-literal-content(n,t) ::=
    //   ( l-nb-literal-text(n)
    //   b-nb-literal-next(n)*
    //   b-chomped-last(t) )?
    //   l-chomped-empty(n,t)
    public Func l_literal_content(int n, string t)
    {
        return All(
            Rep(0, 1, All(
            new object?[] { GetFunc("l_nb_literal_text"), n },
            Rep(0, null, new object?[] { GetFunc("b_nb_literal_next"), n }),
            new object?[] { GetFunc("b_chomped_last"), t }
        )),
            new object?[] { GetFunc("l_chomped_empty"), n, t }
        );
    }

    // [174]
    // c-l+folded(n) ::=
    //   '>' c-b-block-header(m,t)
    //   l-folded-content(n+m,t)
    public Func c_l_folded(int n)
    {
        return All(
            Chr('>'),
            new object?[] { GetFunc("c_b_block_header"), n },
            new object?[] { GetFunc("l_folded_content"), AddNum(n, GetM()), GetT() }
        );
    }

    // [175]
    // s-nb-folded-text(n) ::=
    //   s-indent(n) ns-char
    //   nb-char*
    public Func s_nb_folded_text(int n)
    {
        return All(
            new object?[] { GetFunc("s_indent"), n },
            GetFunc("ns_char"),
            Rep(0, null, GetFunc("nb_char"))
        );
    }

    // [176]
    // l-nb-folded-lines(n) ::=
    //   s-nb-folded-text(n)
    //   ( b-l-folded(n,block-in) s-nb-folded-text(n) )*
    public Func l_nb_folded_lines(int n)
    {
        return All(
            new object?[] { GetFunc("s_nb_folded_text"), n },
            Rep(0, null, All(
            new object?[] { GetFunc("b_l_folded"), n, "block-in" },
            new object?[] { GetFunc("s_nb_folded_text"), n }
        ))
        );
    }

    // [177]
    // s-nb-spaced-text(n) ::=
    //   s-indent(n) s-white
    //   nb-char*
    public Func s_nb_spaced_text(int n)
    {
        return All(
            new object?[] { GetFunc("s_indent"), n },
            GetFunc("s_white"),
            Rep(0, null, GetFunc("nb_char"))
        );
    }

    // [178]
    // b-l-spaced(n) ::=
    //   b-as-line-feed
    //   l-empty(n,block-in)*
    public Func b_l_spaced(int n)
    {
        return All(
            GetFunc("b_as_line_feed"),
            Rep(0, null, new object?[] { GetFunc("l_empty"), n, "block-in" })
        );
    }

    // [179]
    // l-nb-spaced-lines(n) ::=
    //   s-nb-spaced-text(n)
    //   ( b-l-spaced(n) s-nb-spaced-text(n) )*
    public Func l_nb_spaced_lines(int n)
    {
        return All(
            new object?[] { GetFunc("s_nb_spaced_text"), n },
            Rep(0, null, All(
            new object?[] { GetFunc("b_l_spaced"), n },
            new object?[] { GetFunc("s_nb_spaced_text"), n }
        ))
        );
    }

    // [180]
    // l-nb-same-lines(n) ::=
    //   l-empty(n,block-in)*
    //   ( l-nb-folded-lines(n) | l-nb-spaced-lines(n) )
    public Func l_nb_same_lines(int n)
    {
        return All(
            Rep(0, null, new object?[] { GetFunc("l_empty"), n, "block-in" }),
            Any(
            new object?[] { GetFunc("l_nb_folded_lines"), n },
            new object?[] { GetFunc("l_nb_spaced_lines"), n }
        )
        );
    }

    // [181]
    // l-nb-diff-lines(n) ::=
    //   l-nb-same-lines(n)
    //   ( b-as-line-feed l-nb-same-lines(n) )*
    public Func l_nb_diff_lines(int n)
    {
        return All(
            new object?[] { GetFunc("l_nb_same_lines"), n },
            Rep(0, null, All(
            GetFunc("b_as_line_feed"),
            new object?[] { GetFunc("l_nb_same_lines"), n }
        ))
        );
    }

    // [182]
    // l-folded-content(n,t) ::=
    //   ( l-nb-diff-lines(n)
    //   b-chomped-last(t) )?
    //   l-chomped-empty(n,t)
    public Func l_folded_content(int n, string t)
    {
        return All(
            Rep(0, 1, All(
            new object?[] { GetFunc("l_nb_diff_lines"), n },
            new object?[] { GetFunc("b_chomped_last"), t }
        )),
            new object?[] { GetFunc("l_chomped_empty"), n, t }
        );
    }

    // [183]
    // l+block-sequence(n) ::=
    //   ( s-indent(n+m)
    //   c-l-block-seq-entry(n+m) )+
    //   <for_some_fixed_auto-detected_m_>_0>
    public Func l_block_sequence(int n)
    {
        var m = AutoDetectIndent(n);
        if (m == 0) return new Func(new System.Func<bool>(() => false), "l_block_sequence");
        var nm = AddNum(n, m);
        return All(
            Rep(1, null, All(
            new object?[] { GetFunc("s_indent"), nm },
            new object?[] { GetFunc("c_l_block_seq_entry"), nm }
        ))
        );
    }

    // [184]
    // c-l-block-seq-entry(n) ::=
    //   '-' <not_followed_by_an_ns-char>
    //   s-l+block-indented(n,block-in)
    public Func c_l_block_seq_entry(int n)
    {
        return All(
            Chr('-'),
            Chk("!", GetFunc("ns_char")),
            new object?[] { GetFunc("s_l_block_indented"), n, "block-in" }
        );
    }

    // [185]
    // s-l+block-indented(n,c) ::=
    //   ( s-indent(m)
    //   ( ns-l-compact-sequence(n+1+m)
    //   | ns-l-compact-mapping(n+1+m) ) )
    //   | s-l+block-node(n,c)
    //   | ( e-node s-l-comments )
    public Func s_l_block_indented(int n, string c)
    {
        var m = AutoDetectIndent(n);
        return Any(
            All(
            new object?[] { GetFunc("s_indent"), m },
            Any(
            new object?[] { GetFunc("ns_l_compact_sequence"), AddNum(n, AddNum(1, m)) },
            new object?[] { GetFunc("ns_l_compact_mapping"), AddNum(n, AddNum(1, m)) }
        )
        ),
            new object?[] { GetFunc("s_l_block_node"), n, c },
            All(
            GetFunc("e_node"),
            GetFunc("s_l_comments")
        )
        );
    }

    // [186]
    // ns-l-compact-sequence(n) ::=
    //   c-l-block-seq-entry(n)
    //   ( s-indent(n) c-l-block-seq-entry(n) )*
    public Func ns_l_compact_sequence(int n)
    {
        return All(
            new object?[] { GetFunc("c_l_block_seq_entry"), n },
            Rep(0, null, All(
            new object?[] { GetFunc("s_indent"), n },
            new object?[] { GetFunc("c_l_block_seq_entry"), n }
        ))
        );
    }

    // [187]
    // l+block-mapping(n) ::=
    //   ( s-indent(n+m)
    //   ns-l-block-map-entry(n+m) )+
    //   <for_some_fixed_auto-detected_m_>_0>
    public Func l_block_mapping(int n)
    {
        var m = AutoDetectIndent(n);
        if (m == 0) return new Func(new System.Func<bool>(() => false), "l_block_mapping");
        var nm = AddNum(n, m);
        return All(
            Rep(1, null, All(
            new object?[] { GetFunc("s_indent"), nm },
            new object?[] { GetFunc("ns_l_block_map_entry"), nm }
        ))
        );
    }

    // [188]
    // ns-l-block-map-entry(n) ::=
    //   c-l-block-map-explicit-entry(n)
    //   | ns-l-block-map-implicit-entry(n)
    public Func ns_l_block_map_entry(int n)
    {
        return Any(
            new object?[] { GetFunc("c_l_block_map_explicit_entry"), n },
            new object?[] { GetFunc("ns_l_block_map_implicit_entry"), n }
        );
    }

    // [189]
    // c-l-block-map-explicit-entry(n) ::=
    //   c-l-block-map-explicit-key(n)
    //   ( l-block-map-explicit-value(n)
    //   | e-node )
    public Func c_l_block_map_explicit_entry(int n)
    {
        return All(
            new object?[] { GetFunc("c_l_block_map_explicit_key"), n },
            Any(
            new object?[] { GetFunc("l_block_map_explicit_value"), n },
            GetFunc("e_node")
        )
        );
    }

    // [190]
    // c-l-block-map-explicit-key(n) ::=
    //   '?'
    //   s-l+block-indented(n,block-out)
    public Func c_l_block_map_explicit_key(int n)
    {
        return All(
            Chr('?'),
            Chk("=", Any(
            GetFunc("end_of_stream"),
            GetFunc("s_white"),
            GetFunc("b_break")
        )),
            new object?[] { GetFunc("s_l_block_indented"), n, "block-out" }
        );
    }

    // [191]
    // l-block-map-explicit-value(n) ::=
    //   s-indent(n)
    //   ':' s-l+block-indented(n,block-out)
    public Func l_block_map_explicit_value(int n)
    {
        return All(
            new object?[] { GetFunc("s_indent"), n },
            Chr(':'),
            new object?[] { GetFunc("s_l_block_indented"), n, "block-out" }
        );
    }

    // [192]
    // ns-l-block-map-implicit-entry(n) ::=
    //   (
    //   ns-s-block-map-implicit-key
    //   | e-node )
    //   c-l-block-map-implicit-value(n)
    public Func ns_l_block_map_implicit_entry(int n)
    {
        return All(
            Any(
            GetFunc("ns_s_block_map_implicit_key"),
            GetFunc("e_node")
        ),
            new object?[] { GetFunc("c_l_block_map_implicit_value"), n }
        );
    }

    // [193]
    // ns-s-block-map-implicit-key ::=
    //   c-s-implicit-json-key(block-key)
    //   | ns-s-implicit-yaml-key(block-key)
    public Func ns_s_block_map_implicit_key()
    {
        return Any(
            new object?[] { GetFunc("c_s_implicit_json_key"), "block-key" },
            new object?[] { GetFunc("ns_s_implicit_yaml_key"), "block-key" }
        );
    }

    // [194]
    // c-l-block-map-implicit-value(n) ::=
    //   ':' (
    //   s-l+block-node(n,block-out)
    //   | ( e-node s-l-comments ) )
    public Func c_l_block_map_implicit_value(int n)
    {
        return All(
            Chr(':'),
            Any(
            new object?[] { GetFunc("s_l_block_node"), n, "block-out" },
            All(
            GetFunc("e_node"),
            GetFunc("s_l_comments")
        )
        )
        );
    }

    // [195]
    // ns-l-compact-mapping(n) ::=
    //   ns-l-block-map-entry(n)
    //   ( s-indent(n) ns-l-block-map-entry(n) )*
    public Func ns_l_compact_mapping(int n)
    {
        return All(
            new object?[] { GetFunc("ns_l_block_map_entry"), n },
            Rep(0, null, All(
            new object?[] { GetFunc("s_indent"), n },
            new object?[] { GetFunc("ns_l_block_map_entry"), n }
        ))
        );
    }

    // [196]
    // s-l+block-node(n,c) ::=
    //   s-l+block-in-block(n,c) | s-l+flow-in-block(n)
    public Func s_l_block_node(int n, string c)
    {
        return Any(
            new object?[] { GetFunc("s_l_block_in_block"), n, c },
            new object?[] { GetFunc("s_l_flow_in_block"), n }
        );
    }

    // [197]
    // s-l+flow-in-block(n) ::=
    //   s-separate(n+1,flow-out)
    //   ns-flow-node(n+1,flow-out) s-l-comments
    public Func s_l_flow_in_block(int n)
    {
        return All(
            new object?[] { GetFunc("s_separate"), AddNum(n, 1), "flow-out" },
            new object?[] { GetFunc("ns_flow_node"), AddNum(n, 1), "flow-out" },
            GetFunc("s_l_comments")
        );
    }

    // [198]
    // s-l+block-in-block(n,c) ::=
    //   s-l+block-scalar(n,c) | s-l+block-collection(n,c)
    public Func s_l_block_in_block(int n, string c)
    {
        return Any(
            new object?[] { GetFunc("s_l_block_scalar"), n, c },
            new object?[] { GetFunc("s_l_block_collection"), n, c }
        );
    }

    // [199]
    // s-l+block-scalar(n,c) ::=
    //   s-separate(n+1,c)
    //   ( c-ns-properties(n+1,c) s-separate(n+1,c) )?
    //   ( c-l+literal(n) | c-l+folded(n) )
    public Func s_l_block_scalar(int n, string c)
    {
        return All(
            new object?[] { GetFunc("s_separate"), AddNum(n, 1), c },
            Rep(0, 1, All(
            new object?[] { GetFunc("c_ns_properties"), AddNum(n, 1), c },
            new object?[] { GetFunc("s_separate"), AddNum(n, 1), c }
        )),
            Any(
            new object?[] { GetFunc("c_l_literal"), n },
            new object?[] { GetFunc("c_l_folded"), n }
        )
        );
    }

    // [200]
    // s-l+block-collection(n,c) ::=
    //   ( s-separate(n+1,c)
    //   c-ns-properties(n+1,c) )?
    //   s-l-comments
    //   ( l+block-sequence(seq-spaces(n,c))
    //   | l+block-mapping(n) )
    public Func s_l_block_collection(int n, string c)
    {
        return All(
            Rep(0, 1, All(
            new object?[] { GetFunc("s_separate"), AddNum(n, 1), c },
            Any(
            All(
            new object?[] { GetFunc("c_ns_properties"), AddNum(n, 1), c },
            GetFunc("s_l_comments")
        ),
            All(
            GetFunc("c_ns_tag_property"),
            GetFunc("s_l_comments")
        ),
            All(
            GetFunc("c_ns_anchor_property"),
            GetFunc("s_l_comments")
        )
        )
        )),
            GetFunc("s_l_comments"),
            Any(
            new object?[] { GetFunc("l_block_sequence"), new object?[] { GetFunc("seq_spaces"), n, c } },
            new object?[] { GetFunc("l_block_mapping"), n }
        )
        );
    }

    // [201]
    // seq-spaces(n,c) ::=
    //   ( c = block-out => n-1 )
    //   ( c = block-in => n )
    public object seq_spaces(int n, string c)
    {
        return Flip(c, new Dictionary<string, object>
    {
        { "block-in", n },
        { "block-out", SubNum(n, 1) }
    });
    }

    // [202]
    // l-document-prefix ::=
    //   c-byte-order-mark? l-comment*
    public Func l_document_prefix()
    {
        return All(
            Rep(0, 1, GetFunc("c_byte_order_mark")),
            Rep2(0, null, GetFunc("l_comment"))
        );
    }

    // [203]
    // c-directives-end ::=
    //   '-' '-' '-'
    public Func c_directives_end()
    {
        return All(
            Chr('-'),
            Chr('-'),
            Chr('-'),
            Chk("=", Any(
            GetFunc("end_of_stream"),
            GetFunc("s_white"),
            GetFunc("b_break")
        ))
        );
    }

    // [204]
    // c-document-end ::=
    //   '.' '.' '.'
    public Func c_document_end()
    {
        return All(
            Chr('.'),
            Chr('.'),
            Chr('.')
        );
    }

    // [205]
    // l-document-suffix ::=
    //   c-document-end s-l-comments
    public Func l_document_suffix()
    {
        return All(
            GetFunc("c_document_end"),
            GetFunc("s_l_comments")
        );
    }

    // [206]
    // c-forbidden ::=
    //   <start_of_line>
    //   ( c-directives-end | c-document-end )
    //   ( b-char | s-white | <end_of_file> )
    public Func c_forbidden()
    {
        return All(
            GetFunc("start_of_line"),
            Any(
            GetFunc("c_directives_end"),
            GetFunc("c_document_end")
        ),
            Any(
            GetFunc("b_char"),
            GetFunc("s_white"),
            GetFunc("end_of_stream")
        )
        );
    }

    // [207]
    // l-bare-document ::=
    //   s-l+block-node(-1,block-in)
    //   <excluding_c-forbidden_content>
    public Func l_bare_document()
    {
        return All(
            Exclude(GetFunc("c_forbidden")),
            new object?[] { GetFunc("s_l_block_node"), -1, "block-in" }
        );
    }

    // [208]
    // l-explicit-document ::=
    //   c-directives-end
    //   ( l-bare-document
    //   | ( e-node s-l-comments ) )
    public Func l_explicit_document()
    {
        return All(
            GetFunc("c_directives_end"),
            Any(
            GetFunc("l_bare_document"),
            All(
            GetFunc("e_node"),
            GetFunc("s_l_comments")
        )
        )
        );
    }

    // [209]
    // l-directive-document ::=
    //   l-directive+
    //   l-explicit-document
    public Func l_directive_document()
    {
        return All(
            Rep(1, null, GetFunc("l_directive")),
            GetFunc("l_explicit_document")
        );
    }

    // [210]
    // l-any-document ::=
    //   l-directive-document
    //   | l-explicit-document
    //   | l-bare-document
    public Func l_any_document()
    {
        return Any(
            GetFunc("l_directive_document"),
            GetFunc("l_explicit_document"),
            GetFunc("l_bare_document")
        );
    }

    // [211]
    // l-yaml-stream ::=
    //   l-document-prefix* l-any-document?
    //   ( ( l-document-suffix+ l-document-prefix*
    //   l-any-document? )
    //   | ( l-document-prefix* l-explicit-document? ) )*
    public Func l_yaml_stream()
    {
        return All(
            GetFunc("l_document_prefix"),
            Rep(0, 1, GetFunc("l_any_document")),
            Rep2(0, null, Any(
            All(
            GetFunc("l_document_suffix"),
            Rep(0, null, GetFunc("l_document_prefix")),
            Rep2(0, 1, GetFunc("l_any_document"))
        ),
            All(
            GetFunc("l_document_prefix"),
            Rep(0, 1, GetFunc("l_explicit_document"))
        )
        ))
        );
    }

}
