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
            RngHigh(0x10000, 0x10FFFF)
        );
    }

    // [002]
    public Func nb_json()
    {
        return Any(
            Chr('\u0009'),
            RngHigh((int)'\u0020', 0x10FFFF)
        );
    }

    // [003]
    public Func c_byte_order_mark()
    {
        return Chr('\uFEFF');
    }

    // [004]
    public Func c_sequence_entry()
    {
        return Chr('-');
    }

    // [005]
    public Func c_mapping_key()
    {
        return Chr('?');
    }

    // [006]
    public Func c_mapping_value()
    {
        return Chr(':');
    }

    // [007]
    public Func c_collect_entry()
    {
        return Chr(',');
    }

    // [008]
    public Func c_sequence_start()
    {
        return Chr('[');
    }

    // [009]
    public Func c_sequence_end()
    {
        return Chr(']');
    }

    // [010]
    public Func c_mapping_start()
    {
        return Chr('{');
    }

    // [011]
    public Func c_mapping_end()
    {
        return Chr('}');
    }

    // [012]
    public Func c_comment()
    {
        return Chr('#');
    }

    // [013]
    public Func c_anchor()
    {
        return Chr('&');
    }

    // [014]
    public Func c_alias()
    {
        return Chr('*');
    }

    // [015]
    public Func c_tag()
    {
        return Chr('!');
    }

    // [016]
    public Func c_literal()
    {
        return Chr('|');
    }

    // [017]
    public Func c_folded()
    {
        return Chr('>');
    }

    // [018]
    public Func c_single_quote()
    {
        return Chr('\'');
    }

    // [019]
    public Func c_double_quote()
    {
        return Chr('"');
    }

    // [020]
    public Func c_directive()
    {
        return Chr('%');
    }

    // [021]
    public Func c_reserved()
    {
        return Any(
            Chr('@'),
            Chr('`')
        );
    }

    // [022]
    public Func c_indicator()
    {
        return Any(
            Chr('-'),
            Chr('?'),
            Chr(':'),
            Chr(','),
            Chr('['),
            Chr(']'),
            Chr('{'),
            Chr('}'),
            Chr('#'),
            Chr('&'),
            Chr('*'),
            Chr('!'),
            Chr('|'),
            Chr('>'),
            Chr('\''),
            Chr('"'),
            Chr('%'),
            Chr('@'),
            Chr('`')
        );
    }

    // [023]
    public Func c_flow_indicator()
    {
        return Any(
            Chr(','),
            Chr('['),
            Chr(']'),
            Chr('{'),
            Chr('}')
        );
    }

    // [024]
    public Func b_line_feed()
    {
        return Chr('\u000A');
    }

    // [025]
    public Func b_carriage_return()
    {
        return Chr('\u000D');
    }

    // [026]
    public Func b_char()
    {
        return Any(
            GetFunc("b_line_feed"),
            GetFunc("b_carriage_return")
        );
    }

    // [027]
    public Func nb_char()
    {
        return But(
            GetFunc("c_printable"),
            GetFunc("b_char"),
            GetFunc("c_byte_order_mark")
        );
    }

    // [028]
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
    public Func b_as_line_feed()
    {
        return GetFunc("b_break");
    }

    // [030]
    public Func b_non_content()
    {
        return GetFunc("b_break");
    }

    // [031]
    public Func s_space()
    {
        return Chr('\u0020');
    }

    // [032]
    public Func s_tab()
    {
        return Chr('\u0009');
    }

    // [033]
    public Func s_white()
    {
        return Any(
            GetFunc("s_space"),
            GetFunc("s_tab")
        );
    }

    // [034]
    public Func ns_char()
    {
        return But(
            GetFunc("nb_char"),
            GetFunc("s_white")
        );
    }

    // [035]
    public Func ns_dec_digit()
    {
        return Rng('\u0030', '\u0039');
    }

    // [036]
    public Func ns_hex_digit()
    {
        return Any(
            GetFunc("ns_dec_digit"),
            Rng('\u0041', '\u0046'),
            Rng('\u0061', '\u0066')
        );
    }

    // [037]
    public Func ns_ascii_letter()
    {
        return Any(
            Rng('\u0041', '\u005A'),
            Rng('\u0061', '\u007A')
        );
    }

    // [038]
    public Func ns_word_char()
    {
        return Any(
            GetFunc("ns_dec_digit"),
            GetFunc("ns_ascii_letter"),
            Chr('-')
        );
    }

    // [039]
    public Func ns_uri_char()
    {
        return Any(
            All(
            Chr('%'),
            GetFunc("ns_hex_digit"),
            GetFunc("ns_hex_digit")
        ),
            GetFunc("ns_word_char"),
            Chr('#'),
            Chr(';'),
            Chr('/'),
            Chr('?'),
            Chr(':'),
            Chr('@'),
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
    public Func ns_tag_char()
    {
        return But(
            GetFunc("ns_uri_char"),
            Chr('!'),
            GetFunc("c_flow_indicator")
        );
    }

    // [041]
    public Func c_escape()
    {
        return Chr('\\');
    }

    // [042]
    public Func ns_esc_null()
    {
        return Chr('0');
    }

    // [043]
    public Func ns_esc_bell()
    {
        return Chr('a');
    }

    // [044]
    public Func ns_esc_backspace()
    {
        return Chr('b');
    }

    // [045]
    public Func ns_esc_horizontal_tab()
    {
        return Any(
            Chr('t'),
            Chr('\u0009')
        );
    }

    // [046]
    public Func ns_esc_line_feed()
    {
        return Chr('n');
    }

    // [047]
    public Func ns_esc_vertical_tab()
    {
        return Chr('v');
    }

    // [048]
    public Func ns_esc_form_feed()
    {
        return Chr('f');
    }

    // [049]
    public Func ns_esc_carriage_return()
    {
        return Chr('r');
    }

    // [050]
    public Func ns_esc_escape()
    {
        return Chr('e');
    }

    // [051]
    public Func ns_esc_space()
    {
        return Chr('\u0020');
    }

    // [052]
    public Func ns_esc_double_quote()
    {
        return Chr('"');
    }

    // [053]
    public Func ns_esc_slash()
    {
        return Chr('/');
    }

    // [054]
    public Func ns_esc_backslash()
    {
        return Chr('\\');
    }

    // [055]
    public Func ns_esc_next_line()
    {
        return Chr('N');
    }

    // [056]
    public Func ns_esc_non_breaking_space()
    {
        return Chr('_');
    }

    // [057]
    public Func ns_esc_line_separator()
    {
        return Chr('L');
    }

    // [058]
    public Func ns_esc_paragraph_separator()
    {
        return Chr('P');
    }

    // [059]
    public Func ns_esc_8_bit()
    {
        return All(
            Chr('x'),
            Rep(2, 2, GetFunc("ns_hex_digit"))
        );
    }

    // [060]
    public Func ns_esc_16_bit()
    {
        return All(
            Chr('u'),
            Rep(4, 4, GetFunc("ns_hex_digit"))
        );
    }

    // [061]
    public Func ns_esc_32_bit()
    {
        return All(
            Chr('U'),
            Rep(8, 8, GetFunc("ns_hex_digit"))
        );
    }

    // [062]
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
    public Func s_indent(int n)
    {
        return Rep(n, n, GetFunc("s_space"));
    }

    // [064]
    public Func s_indent_lt(int n)
    {
        return May(All(
            Rep(0, null, GetFunc("s_space")),
            Lt(Len(Match()), n)
        ));
    }

    // [065]
    public Func s_indent_le(int n)
    {
        return May(All(
            Rep(0, null, GetFunc("s_space")),
            Le(Len(Match()), n)
        ));
    }

    // [066]
    public Func s_separate_in_line()
    {
        return Any(
            Rep(1, null, GetFunc("s_white")),
            GetFunc("start_of_line")
        );
    }

    // [067]
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
    public object s_block_line_prefix(int n)
    {
        return new object?[] { GetFunc("s_indent"), n };
    }

    // [069]
    public Func s_flow_line_prefix(int n)
    {
        return All(
            new object?[] { GetFunc("s_indent"), n },
            Rep(0, 1, GetFunc("s_separate_in_line"))
        );
    }

    // [070]
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
    public Func b_l_trimmed(int n, string c)
    {
        return All(
            GetFunc("b_non_content"),
            Rep(1, null, new object?[] { GetFunc("l_empty"), n, c })
        );
    }

    // [072]
    public Func b_as_space()
    {
        return GetFunc("b_break");
    }

    // [073]
    public Func b_l_folded(int n, string c)
    {
        return Any(
            new object?[] { GetFunc("b_l_trimmed"), n, c },
            GetFunc("b_as_space")
        );
    }

    // [074]
    public Func s_flow_folded(int n)
    {
        return All(
            Rep(0, 1, GetFunc("s_separate_in_line")),
            new object?[] { GetFunc("b_l_folded"), n, "flow-in" },
            new object?[] { GetFunc("s_flow_line_prefix"), n }
        );
    }

    // [075]
    public Func c_nb_comment_text()
    {
        return All(
            Chr('#'),
            Rep(0, null, GetFunc("nb_char"))
        );
    }

    // [076]
    public Func b_comment()
    {
        return Any(
            GetFunc("b_non_content"),
            GetFunc("end_of_stream")
        );
    }

    // [077]
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
    public Func l_comment()
    {
        return All(
            GetFunc("s_separate_in_line"),
            Rep(0, 1, GetFunc("c_nb_comment_text")),
            GetFunc("b_comment")
        );
    }

    // [079]
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
    public Func ns_directive_name()
    {
        return Rep(1, null, GetFunc("ns_char"));
    }

    // [085]
    public Func ns_directive_parameter()
    {
        return Rep(1, null, GetFunc("ns_char"));
    }

    // [086]
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
    public Func ns_yaml_version()
    {
        return All(
            Rep(1, null, GetFunc("ns_dec_digit")),
            Chr('.'),
            Rep2(1, null, GetFunc("ns_dec_digit"))
        );
    }

    // [088]
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
    public Func c_tag_handle()
    {
        return Any(
            GetFunc("c_named_tag_handle"),
            GetFunc("c_secondary_tag_handle"),
            GetFunc("c_primary_tag_handle")
        );
    }

    // [090]
    public Func c_primary_tag_handle()
    {
        return Chr('!');
    }

    // [091]
    public Func c_secondary_tag_handle()
    {
        return All(
            Chr('!'),
            Chr('!')
        );
    }

    // [092]
    public Func c_named_tag_handle()
    {
        return All(
            Chr('!'),
            Rep(1, null, GetFunc("ns_word_char")),
            Chr('!')
        );
    }

    // [093]
    public Func ns_tag_prefix()
    {
        return Any(
            GetFunc("c_ns_local_tag_prefix"),
            GetFunc("ns_global_tag_prefix")
        );
    }

    // [094]
    public Func c_ns_local_tag_prefix()
    {
        return All(
            Chr('!'),
            Rep(0, null, GetFunc("ns_uri_char"))
        );
    }

    // [095]
    public Func ns_global_tag_prefix()
    {
        return All(
            GetFunc("ns_tag_char"),
            Rep(0, null, GetFunc("ns_uri_char"))
        );
    }

    // [096]
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
    public Func c_ns_tag_property()
    {
        return Any(
            GetFunc("c_verbatim_tag"),
            GetFunc("c_ns_shorthand_tag"),
            GetFunc("c_non_specific_tag")
        );
    }

    // [098]
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
    public Func c_ns_shorthand_tag()
    {
        return All(
            GetFunc("c_tag_handle"),
            Rep(1, null, GetFunc("ns_tag_char"))
        );
    }

    // [100]
    public Func c_non_specific_tag()
    {
        return Chr('!');
    }

    // [101]
    public Func c_ns_anchor_property()
    {
        return All(
            Chr('&'),
            GetFunc("ns_anchor_name")
        );
    }

    // [102]
    public Func ns_anchor_char()
    {
        return But(
            GetFunc("ns_char"),
            GetFunc("c_flow_indicator")
        );
    }

    // [103]
    public Func ns_anchor_name()
    {
        return Rep(1, null, GetFunc("ns_anchor_char"));
    }

    // [104]
    public Func c_ns_alias_node()
    {
        return All(
            Chr('*'),
            GetFunc("ns_anchor_name")
        );
    }

    // [105]
    public Func e_scalar()
    {
        return GetFunc("empty");
    }

    // [106]
    public Func e_node()
    {
        return GetFunc("e_scalar");
    }

    // [107]
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
    public Func ns_double_char()
    {
        return But(
            GetFunc("nb_double_char"),
            GetFunc("s_white")
        );
    }

    // [109]
    public Func c_double_quoted(int n, string c)
    {
        return All(
            Chr('"'),
            new object?[] { GetFunc("nb_double_text"), n, c },
            Chr('"')
        );
    }

    // [110]
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
    public Func nb_double_one_line()
    {
        return Rep(0, null, GetFunc("nb_double_char"));
    }

    // [112]
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
    public Func s_double_break(int n)
    {
        return Any(
            new object?[] { GetFunc("s_double_escaped"), n },
            new object?[] { GetFunc("s_flow_folded"), n }
        );
    }

    // [114]
    public Func nb_ns_double_in_line()
    {
        return Rep(0, null, All(
            Rep(0, null, GetFunc("s_white")),
            GetFunc("ns_double_char")
        ));
    }

    // [115]
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
    public Func c_quoted_quote()
    {
        return All(
            Chr('\''),
            Chr('\'')
        );
    }

    // [118]
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
    public Func ns_single_char()
    {
        return But(
            GetFunc("nb_single_char"),
            GetFunc("s_white")
        );
    }

    // [120]
    public Func c_single_quoted(int n, string c)
    {
        return All(
            Chr('\''),
            new object?[] { GetFunc("nb_single_text"), n, c },
            Chr('\'')
        );
    }

    // [121]
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
    public Func nb_single_one_line()
    {
        return Rep(0, null, GetFunc("nb_single_char"));
    }

    // [123]
    public Func nb_ns_single_in_line()
    {
        return Rep(0, null, All(
            Rep(0, null, GetFunc("s_white")),
            GetFunc("ns_single_char")
        ));
    }

    // [124]
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
    public Func ns_plain_safe_out()
    {
        return GetFunc("ns_char");
    }

    // [129]
    public Func ns_plain_safe_in()
    {
        return But(
            GetFunc("ns_char"),
            GetFunc("c_flow_indicator")
        );
    }

    // [130]
    public Func ns_plain_char(string c)
    {
        return Any(
            But(
            new object?[] { GetFunc("ns_plain_safe"), c },
            Chr(':'),
            Chr('#')
        ),
            All(
            Chk("<=", GetFunc("ns_char")),
            Chr('#')
        ),
            All(
            Chr(':'),
            Chk("=", new object?[] { GetFunc("ns_plain_safe"), c })
        )
        );
    }

    // [131]
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
    public Func nb_ns_plain_in_line(string c)
    {
        return Rep(0, null, All(
            Rep(0, null, GetFunc("s_white")),
            new object?[] { GetFunc("ns_plain_char"), c }
        ));
    }

    // [133]
    public Func ns_plain_one_line(string c)
    {
        return All(
            new object?[] { GetFunc("ns_plain_first"), c },
            new object?[] { GetFunc("nb_ns_plain_in_line"), c }
        );
    }

    // [134]
    public Func s_ns_plain_next_line(int n, string c)
    {
        return All(
            new object?[] { GetFunc("s_flow_folded"), n },
            new object?[] { GetFunc("ns_plain_char"), c },
            new object?[] { GetFunc("nb_ns_plain_in_line"), c }
        );
    }

    // [135]
    public Func ns_plain_multi_line(int n, string c)
    {
        return All(
            new object?[] { GetFunc("ns_plain_one_line"), c },
            Rep(0, null, new object?[] { GetFunc("s_ns_plain_next_line"), n, c })
        );
    }

    // [136]
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
    public Func ns_flow_seq_entry(int n, string c)
    {
        return Any(
            new object?[] { GetFunc("ns_flow_pair"), n, c },
            new object?[] { GetFunc("ns_flow_node"), n, c }
        );
    }

    // [140]
    public Func c_flow_mapping(int n, string c)
    {
        return All(
            Chr('{'),
            Rep(0, 1, new object?[] { GetFunc("s_separate"), n, c }),
            Rep2(0, 1, new object?[] { GetFunc("ns_s_flow_map_entries"), n, new object?[] { GetFunc("in_flow"), c } }),
            Chr('}')
        );
    }

    // [141]
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
    public Func ns_flow_map_implicit_entry(int n, string c)
    {
        return Any(
            new object?[] { GetFunc("ns_flow_map_yaml_key_entry"), n, c },
            new object?[] { GetFunc("c_ns_flow_map_empty_key_entry"), n, c },
            new object?[] { GetFunc("c_ns_flow_map_json_key_entry"), n, c }
        );
    }

    // [145]
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
    public Func c_ns_flow_map_empty_key_entry(int n, string c)
    {
        return All(
            GetFunc("e_node"),
            new object?[] { GetFunc("c_ns_flow_map_separate_value"), n, c }
        );
    }

    // [147]
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
    public Func ns_flow_pair_entry(int n, string c)
    {
        return Any(
            new object?[] { GetFunc("ns_flow_pair_yaml_key_entry"), n, c },
            new object?[] { GetFunc("c_ns_flow_map_empty_key_entry"), n, c },
            new object?[] { GetFunc("c_ns_flow_pair_json_key_entry"), n, c }
        );
    }

    // [152]
    public Func ns_flow_pair_yaml_key_entry(int n, string c)
    {
        return All(
            new object?[] { GetFunc("ns_s_implicit_yaml_key"), "flow-key" },
            new object?[] { GetFunc("c_ns_flow_map_separate_value"), n, c }
        );
    }

    // [153]
    public Func c_ns_flow_pair_json_key_entry(int n, string c)
    {
        return All(
            new object?[] { GetFunc("c_s_implicit_json_key"), "flow-key" },
            new object?[] { GetFunc("c_ns_flow_map_adjacent_value"), n, c }
        );
    }

    // [154]
    public Func ns_s_implicit_yaml_key(string c)
    {
        return All(
            Max(1024),
            new object?[] { GetFunc("ns_flow_yaml_node"), null, c },
            Rep(0, 1, GetFunc("s_separate_in_line"))
        );
    }

    // [155]
    public Func c_s_implicit_json_key(string c)
    {
        return All(
            Max(1024),
            new object?[] { GetFunc("c_flow_json_node"), null, c },
            Rep(0, 1, GetFunc("s_separate_in_line"))
        );
    }

    // [156]
    public object ns_flow_yaml_content(int n, string c)
    {
        return new object?[] { GetFunc("ns_plain"), n, c };
    }

    // [157]
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
    public Func ns_flow_content(int n, string c)
    {
        return Any(
            new object?[] { GetFunc("ns_flow_yaml_content"), n, c },
            new object?[] { GetFunc("c_flow_json_content"), n, c }
        );
    }

    // [159]
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
    public Func c_indentation_indicator(int n)
    {
        return Any(
            If(Rng('\u0031', '\u0039'), Set("m", Ord(Match()))),
            If(GetFunc("empty"), Set("m", new object?[] { GetFunc("auto_detect"), n }))
        );
    }

    // [164]
    public Func c_chomping_indicator()
    {
        return Any(
            If(Chr('-'), Set("t", "strip")),
            If(Chr('+'), Set("t", "keep")),
            If(GetFunc("empty"), Set("t", "clip"))
        );
    }

    // [165]
    public Func b_chomped_last(string t)
    {
        return Case(t, new Dictionary<string, object>
    {
        { "clip", Any(
            GetFunc("b_as_line_feed"),
            GetFunc("end_of_stream")
        ) },
        { "keep", Any(
            GetFunc("b_as_line_feed"),
            GetFunc("end_of_stream")
        ) },
        { "strip", Any(
            GetFunc("b_non_content"),
            GetFunc("end_of_stream")
        ) }
    });
    }

    // [166]
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
    public Func l_keep_empty(int n)
    {
        return All(
            Rep(0, null, new object?[] { GetFunc("l_empty"), n, "block-in" }),
            Rep2(0, 1, new object?[] { GetFunc("l_trail_comments"), n })
        );
    }

    // [169]
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
    public Func c_l_literal(int n)
    {
        return All(
            Chr('|'),
            new object?[] { GetFunc("c_b_block_header"), n },
            new object?[] { GetFunc("l_literal_content"), AddNum(n, GetM()), GetT() }
        );
    }

    // [171]
    public Func l_nb_literal_text(int n)
    {
        return All(
            Rep(0, null, new object?[] { GetFunc("l_empty"), n, "block-in" }),
            new object?[] { GetFunc("s_indent"), n },
            Rep2(1, null, GetFunc("nb_char"))
        );
    }

    // [172]
    public Func b_nb_literal_next(int n)
    {
        return All(
            GetFunc("b_as_line_feed"),
            new object?[] { GetFunc("l_nb_literal_text"), n }
        );
    }

    // [173]
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
    public Func c_l_folded(int n)
    {
        return All(
            Chr('>'),
            new object?[] { GetFunc("c_b_block_header"), n },
            new object?[] { GetFunc("l_folded_content"), AddNum(n, GetM()), GetT() }
        );
    }

    // [175]
    public Func s_nb_folded_text(int n)
    {
        return All(
            new object?[] { GetFunc("s_indent"), n },
            GetFunc("ns_char"),
            Rep(0, null, GetFunc("nb_char"))
        );
    }

    // [176]
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
    public Func s_nb_spaced_text(int n)
    {
        return All(
            new object?[] { GetFunc("s_indent"), n },
            GetFunc("s_white"),
            Rep(0, null, GetFunc("nb_char"))
        );
    }

    // [178]
    public Func b_l_spaced(int n)
    {
        return All(
            GetFunc("b_as_line_feed"),
            Rep(0, null, new object?[] { GetFunc("l_empty"), n, "block-in" })
        );
    }

    // [179]
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
    public Func l_block_sequence(int n)
    {
        var m = AutoDetectIndent(n);
        if (m == 0) return new Func(new System.Func<bool>(() => false), "l_block_sequence");
        var nm = AddNum(n, m);
        return All(
            Rep(1, null,
                All(
                    new object?[] { GetFunc("s_indent"), nm },
                    new object?[] { GetFunc("c_l_block_seq_entry"), nm }
                ))
        );
    }

    // [184]
    public Func c_l_block_seq_entry(int n)
    {
        return All(
            Chr('-'),
            Chk("!", GetFunc("ns_char")),
            new object?[] { GetFunc("s_l_block_indented"), n, "block-in" }
        );
    }

    // [185]
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
    public Func l_block_mapping(int n)
    {
        var m = AutoDetectIndent(n);
        if (m == 0) return new Func(new System.Func<bool>(() => false), "l_block_mapping");
        var nm = AddNum(n, m);
        return All(
            Rep(1, null,
                All(
                    new object?[] { GetFunc("s_indent"), nm },
                    new object?[] { GetFunc("ns_l_block_map_entry"), nm }
                ))
        );
    }

    // [188]
    public Func ns_l_block_map_entry(int n)
    {
        return Any(
            new object?[] { GetFunc("c_l_block_map_explicit_entry"), n },
            new object?[] { GetFunc("ns_l_block_map_implicit_entry"), n }
        );
    }

    // [189]
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
    public Func l_block_map_explicit_value(int n)
    {
        return All(
            new object?[] { GetFunc("s_indent"), n },
            Chr(':'),
            new object?[] { GetFunc("s_l_block_indented"), n, "block-out" }
        );
    }

    // [192]
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
    public Func ns_s_block_map_implicit_key()
    {
        return Any(
            new object?[] { GetFunc("c_s_implicit_json_key"), "block-key" },
            new object?[] { GetFunc("ns_s_implicit_yaml_key"), "block-key" }
        );
    }

    // [194]
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
    public Func s_l_block_node(int n, string c)
    {
        return Any(
            new object?[] { GetFunc("s_l_block_in_block"), n, c },
            new object?[] { GetFunc("s_l_flow_in_block"), n }
        );
    }

    // [197]
    public Func s_l_flow_in_block(int n)
    {
        return All(
            new object?[] { GetFunc("s_separate"), AddNum(n, 1), "flow-out" },
            new object?[] { GetFunc("ns_flow_node"), AddNum(n, 1), "flow-out" },
            GetFunc("s_l_comments")
        );
    }

    // [198]
    public Func s_l_block_in_block(int n, string c)
    {
        return Any(
            new object?[] { GetFunc("s_l_block_scalar"), n, c },
            new object?[] { GetFunc("s_l_block_collection"), n, c }
        );
    }

    // [199]
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
    public object seq_spaces(int n, string c)
    {
        return Flip(c, new Dictionary<string, object>
    {
        { "block-in", n },
        { "block-out", SubNum(n, 1) }
    });
    }

    // [202]
    public Func l_document_prefix()
    {
        return All(
            Rep(0, 1, GetFunc("c_byte_order_mark")),
            Rep2(0, null, GetFunc("l_comment"))
        );
    }

    // [203]
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
    public Func c_document_end()
    {
        return All(
            Chr('.'),
            Chr('.'),
            Chr('.')
        );
    }

    // [205]
    public Func l_document_suffix()
    {
        return All(
            GetFunc("c_document_end"),
            GetFunc("s_l_comments")
        );
    }

    // [206]
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
    public Func l_bare_document()
    {
        return All(
            Exclude(GetFunc("c_forbidden")),
            new object?[] { GetFunc("s_l_block_node"), -1, "block-in" }
        );
    }

    // [208]
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
    public Func l_directive_document()
    {
        return All(
            Rep(1, null, GetFunc("l_directive")),
            GetFunc("l_explicit_document")
        );
    }

    // [210]
    public Func l_any_document()
    {
        return Any(
            GetFunc("l_directive_document"),
            GetFunc("l_explicit_document"),
            GetFunc("l_bare_document")
        );
    }

    // [211]
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