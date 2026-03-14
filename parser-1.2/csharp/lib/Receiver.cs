namespace YamlParser;

using System.Text;
using System.Text.Json;

public class YamlEvent
{
    public string Event { get; set; } = "";
    public bool? Explicit { get; set; }
    public string? Version { get; set; }
    public bool? Flow { get; set; }
    public string? Style { get; set; }
    public string? Value { get; set; }
    public string? Anchor { get; set; }
    public string? Tag { get; set; }
    public string? AliasName { get; set; }
    public string? Text { get; set; } // for cache entries
    public bool HasValue => Value != null;
}

public class Receiver
{
    public Parser? Parser { get; set; }
    public List<YamlEvent> Events { get; set; } = new();
    public List<List<YamlEvent>> Cache { get; set; } = new();
    public Action<YamlEvent>? Callback { get; set; }

    public string? CurrentAnchor { get; set; }
    public string? CurrentTag { get; set; }
    public Dictionary<string, string> TagMap { get; set; } = new();
    public YamlEvent? DocumentStart { get; set; }
    public YamlEvent? DocumentEnd { get; set; }
    public string? TagHandle { get; set; }
    public bool InScalar { get; set; }
    public string First { get; set; } = "";

    public static YamlEvent StreamStartEvent() => new() { Event = "stream_start" };
    public static YamlEvent StreamEndEvent() => new() { Event = "stream_end" };
    public static YamlEvent DocumentStartEvent(bool expl = false) =>
        new() { Event = "document_start", Explicit = expl };
    public static YamlEvent DocumentEndEvent(bool expl = false) =>
        new() { Event = "document_end", Explicit = expl };
    public static YamlEvent MappingStartEvent(bool flow = false) =>
        new() { Event = "mapping_start", Flow = flow };
    public static YamlEvent MappingEndEvent() => new() { Event = "mapping_end" };
    public static YamlEvent SequenceStartEvent(bool flow = false) =>
        new() { Event = "sequence_start", Flow = flow };
    public static YamlEvent SequenceEndEvent() => new() { Event = "sequence_end" };
    public static YamlEvent ScalarEvent(string style, string value) =>
        new() { Event = "scalar", Style = style, Value = value };
    public static YamlEvent AliasEvent(string name) =>
        new() { Event = "alias", AliasName = name };
    public static YamlEvent CacheEvent(string text) => new() { Text = text };

    public void Send(YamlEvent evt)
    {
        if (Callback != null)
            Callback(evt);
        else
            Events.Add(evt);
    }

    public YamlEvent Add(YamlEvent evt)
    {
        if (evt.Event != null && evt.Event.Length > 0)
        {
            if (CurrentAnchor != null)
            {
                evt.Anchor = CurrentAnchor;
                CurrentAnchor = null;
            }
            if (CurrentTag != null)
            {
                evt.Tag = CurrentTag;
                CurrentTag = null;
            }
        }
        Push(evt);
        return evt;
    }

    public void Push(YamlEvent evt)
    {
        if (Cache.Count > 0)
        {
            Cache[^1].Add(evt);
        }
        else
        {
            if (evt.Event == "mapping_start" || evt.Event == "sequence_start" || evt.Event == "scalar")
            {
                CheckDocumentStart();
            }
            Send(evt);
        }
    }

    public void CacheUp(YamlEvent? evt = null)
    {
        Cache.Add(new List<YamlEvent>());
        if (evt != null) Add(evt);
    }

    public void CacheDown(YamlEvent? evt = null)
    {
        if (Cache.Count == 0) throw new Exception("cache_down");
        var events = Cache[^1];
        Cache.RemoveAt(Cache.Count - 1);
        foreach (var e in events) Push(e);
        if (evt != null) Add(evt);
    }

    public List<YamlEvent> CacheDrop()
    {
        if (Cache.Count == 0) throw new Exception("cache_drop");
        var events = Cache[^1];
        Cache.RemoveAt(Cache.Count - 1);
        return events;
    }

    public YamlEvent? CacheGet(string type)
    {
        if (Cache.Count > 0 && Cache[^1].Count > 0 && Cache[^1][0].Event == type)
            return Cache[^1][0];
        return null;
    }

    public void CheckDocumentStart()
    {
        if (DocumentStart == null) return;
        Send(DocumentStart);
        DocumentStart = null;
        DocumentEnd = DocumentEndEvent();
    }

    public void CheckDocumentEnd()
    {
        if (DocumentEnd == null) return;
        Send(DocumentEnd);
        DocumentEnd = null;
        TagMap = new Dictionary<string, string>();
        DocumentStart = DocumentStartEvent();
    }

    // --- Receiver callbacks (try__, got__, not__) ---

    public void try__l_yaml_stream(MatchInfo o)
    {
        Add(StreamStartEvent());
        TagMap = new Dictionary<string, string>();
        DocumentStart = DocumentStartEvent();
        DocumentEnd = null;
    }
    public void got__l_yaml_stream(MatchInfo o)
    {
        CheckDocumentEnd();
        Add(StreamEndEvent());
    }

    public void got__ns_yaml_version(MatchInfo o)
    {
        if (DocumentStart?.Version != null)
            throw new Exception("Multiple %YAML directives not allowed");
        if (DocumentStart != null)
            DocumentStart.Version = o.Text;
    }

    public void got__c_tag_handle(MatchInfo o)
    {
        TagHandle = o.Text;
    }
    public void got__ns_tag_prefix(MatchInfo o)
    {
        if (TagHandle != null)
            TagMap[TagHandle] = o.Text;
    }

    public void got__c_directives_end(MatchInfo o)
    {
        CheckDocumentEnd();
        if (DocumentStart != null)
            DocumentStart.Explicit = true;
    }
    public void got__c_document_end(MatchInfo o)
    {
        if (DocumentEnd != null)
            DocumentEnd.Explicit = true;
        CheckDocumentEnd();
    }

    public void got__c_flow_mapping__all__x7b(MatchInfo o) { Add(MappingStartEvent(true)); }
    public void got__c_flow_mapping__all__x7d(MatchInfo o) { Add(MappingEndEvent()); }

    public void got__c_flow_sequence__all__x5b(MatchInfo o) { Add(SequenceStartEvent(true)); }
    public void got__c_flow_sequence__all__x5d(MatchInfo o) { Add(SequenceEndEvent()); }

    public void try__l_block_mapping(MatchInfo o) { CacheUp(MappingStartEvent()); }
    public void got__l_block_mapping(MatchInfo o) { CacheDown(MappingEndEvent()); }
    public void not__l_block_mapping(MatchInfo o) { CacheDrop(); }

    public void try__l_block_sequence(MatchInfo o) { CacheUp(SequenceStartEvent()); }
    public void got__l_block_sequence(MatchInfo o) { CacheDown(SequenceEndEvent()); }
    public void not__l_block_sequence(MatchInfo o)
    {
        var evts = CacheDrop();
        if (evts.Count > 0)
        {
            CurrentAnchor = evts[0].Anchor;
            CurrentTag = evts[0].Tag;
        }
    }

    public void try__ns_l_compact_mapping(MatchInfo o) { CacheUp(MappingStartEvent()); }
    public void got__ns_l_compact_mapping(MatchInfo o) { CacheDown(MappingEndEvent()); }
    public void not__ns_l_compact_mapping(MatchInfo o) { CacheDrop(); }

    public void try__ns_l_compact_sequence(MatchInfo o) { CacheUp(SequenceStartEvent()); }
    public void got__ns_l_compact_sequence(MatchInfo o) { CacheDown(SequenceEndEvent()); }
    public void not__ns_l_compact_sequence(MatchInfo o) { CacheDrop(); }

    public void try__ns_flow_pair(MatchInfo o) { CacheUp(MappingStartEvent(true)); }
    public void got__ns_flow_pair(MatchInfo o) { CacheDown(MappingEndEvent()); }
    public void not__ns_flow_pair(MatchInfo o) { CacheDrop(); }

    public void try__ns_l_block_map_implicit_entry(MatchInfo o) { CacheUp(); }
    public void got__ns_l_block_map_implicit_entry(MatchInfo o) { CacheDown(); }
    public void not__ns_l_block_map_implicit_entry(MatchInfo o) { CacheDrop(); }

    public void try__c_l_block_map_explicit_entry(MatchInfo o) { CacheUp(); }
    public void got__c_l_block_map_explicit_entry(MatchInfo o) { CacheDown(); }
    public void not__c_l_block_map_explicit_entry(MatchInfo o) { CacheDrop(); }

    public void try__c_ns_flow_map_empty_key_entry(MatchInfo o) { CacheUp(); }
    public void got__c_ns_flow_map_empty_key_entry(MatchInfo o) { CacheDown(); }
    public void not__c_ns_flow_map_empty_key_entry(MatchInfo o) { CacheDrop(); }

    public void got__ns_plain(MatchInfo o)
    {
        var text = o.Text;
        text = System.Text.RegularExpressions.Regex.Replace(text, @"[ \t]*\r?\n[ \t]*", "\n");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"(\n)(\n*)", m =>
            m.Groups[2].Value.Length > 0 ? m.Groups[2].Value : " ");
        Add(ScalarEvent("plain", text));
    }

    public void got__c_single_quoted(MatchInfo o)
    {
        var text = o.Text[1..^1]; // strip quotes
        text = System.Text.RegularExpressions.Regex.Replace(text, @"[ \t]*\r?\n[ \t]*", "\n");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"(\n)(\n*)", m =>
            m.Groups[2].Value.Length > 0 ? m.Groups[2].Value : " ");
        text = text.Replace("''", "'");
        Add(ScalarEvent("single", text));
    }

    public void got__c_double_quoted(MatchInfo o)
    {
        var text = o.Text[1..^1]; // strip quotes
        // Process line folding (but not escaped newlines)
        text = System.Text.RegularExpressions.Regex.Replace(text, @"(?<!\\)[ \t]*\r?\n[ \t]*", "\n");
        // Remove escaped newlines
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\\\n[ \t]*", "");
        // Fold newlines
        text = System.Text.RegularExpressions.Regex.Replace(text, @"(\n)(\n*)", m =>
            m.Groups[2].Value.Length > 0 ? m.Groups[2].Value : " ");
        // Process escape sequences
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\\([""\/])", "$1");
        text = text.Replace("\\ ", " ");
        text = text.Replace("\\b", "\b");
        text = text.Replace("\\\t", "\t");
        text = text.Replace("\\t", "\t");
        text = text.Replace("\\n", "\n");
        text = text.Replace("\\r", "\r");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\\x([0-9a-fA-F]{2})", m =>
            ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\\u([0-9a-fA-F]{4})", m =>
            char.ConvertFromUtf32(Convert.ToInt32(m.Groups[1].Value, 16)));
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\\U([0-9a-fA-F]{8})", m =>
            char.ConvertFromUtf32(Convert.ToInt32(m.Groups[1].Value, 16)));
        text = text.Replace("\\\\", "\\");
        Add(ScalarEvent("double", text));
    }

    public void got__l_empty(MatchInfo o)
    {
        if (InScalar) Add(CacheEvent(""));
    }
    public void got__l_nb_literal_text__all__rep2(MatchInfo o)
    {
        Add(CacheEvent(o.Text));
    }
    public void try__c_l_literal(MatchInfo o)
    {
        InScalar = true;
        CacheUp();
    }
    public void got__c_l_literal(MatchInfo o)
    {
        InScalar = false;
        var lines = CacheDrop();
        if (lines.Count > 0 && lines[^1].Text == "")
            lines.RemoveAt(lines.Count - 1);
        var text = string.Join("", lines.Select(l => l.Text + "\n"));
        var t = Parser!.StateCurr().T;
        if (t == "clip")
        {
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\n+\z", "\n");
        }
        else if (t == "strip")
        {
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\n+\z", "");
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(text, @"\S"))
        {
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\n(\n+)\z", "$1");
        }
        Add(ScalarEvent("literal", text));
    }
    public void not__c_l_literal(MatchInfo o)
    {
        InScalar = false;
        CacheDrop();
    }

    public void got__ns_char(MatchInfo o)
    {
        if (InScalar) First = o.Text;
    }
    public void got__s_white(MatchInfo o)
    {
        if (InScalar) First = o.Text;
    }
    public void got__s_nb_folded_text__all__rep(MatchInfo o)
    {
        Add(CacheEvent(First + o.Text));
    }
    public void got__s_nb_spaced_text__all__rep(MatchInfo o)
    {
        Add(CacheEvent(First + o.Text));
    }
    public void try__c_l_folded(MatchInfo o)
    {
        InScalar = true;
        First = "";
        CacheUp();
    }
    public void got__c_l_folded(MatchInfo o)
    {
        InScalar = false;
        var lines = CacheDrop().Select(l => l.Text ?? "").ToList();
        var text = string.Join("\n", lines);
        // Fold: non-indented lines that precede a non-indented line get space-joined
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^(\S.*)\n(?=\S)", "$1 ", System.Text.RegularExpressions.RegexOptions.Multiline);
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^(\S.*)\n(\n+)", "$1$2", System.Text.RegularExpressions.RegexOptions.Multiline);
        text = System.Text.RegularExpressions.Regex.Replace(text, @"^([ \t]+\S.*)\n(\n+)(?=\S)", "$1$2", System.Text.RegularExpressions.RegexOptions.Multiline);
        text += "\n";

        var t = Parser!.StateCurr().T;
        if (t == "clip")
        {
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\n+\z", "\n");
            if (text == "\n") text = "";
        }
        else if (t == "strip")
        {
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\n+\z", "");
        }
        Add(ScalarEvent("folded", text));
    }
    public void not__c_l_folded(MatchInfo o)
    {
        InScalar = false;
        CacheDrop();
    }

    public void got__e_scalar(MatchInfo o) { Add(ScalarEvent("plain", "")); }

    public void not__s_l_block_collection__all__rep__all__any__all(MatchInfo o)
    {
        CurrentAnchor = null;
        CurrentTag = null;
    }

    public void got__c_ns_anchor_property(MatchInfo o)
    {
        CurrentAnchor = o.Text[1..]; // strip leading &
    }

    public void got__c_ns_tag_property(MatchInfo o)
    {
        var tag = o.Text;
        if (tag.StartsWith("!<") && tag.EndsWith(">"))
        {
            CurrentTag = tag[2..^1];
        }
        else if (tag.StartsWith("!!"))
        {
            if (TagMap.TryGetValue("!!", out var prefix))
                CurrentTag = prefix + tag[2..];
            else
                CurrentTag = "tag:yaml.org,2002:" + tag[2..];
        }
        else if (tag.Length > 1 && tag[0] == '!')
        {
            // Check for named tag handle like !foo!
            var idx = tag.IndexOf('!', 1);
            if (idx > 0)
            {
                var handle = tag[..(idx + 1)];
                if (TagMap.TryGetValue(handle, out var prefix))
                    CurrentTag = prefix + tag[(idx + 1)..];
                else
                    throw new Exception($"No %TAG entry for '{handle}'");
            }
            else if (TagMap.TryGetValue("!", out var prefix2))
            {
                CurrentTag = prefix2 + tag[1..];
            }
            else
            {
                CurrentTag = tag;
            }
        }
        else
        {
            CurrentTag = tag;
        }
        // URL decode %XX sequences
        if (CurrentTag != null)
        {
            CurrentTag = System.Text.RegularExpressions.Regex.Replace(CurrentTag, @"%([0-9a-fA-F]{2})", m =>
                ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString());
        }
    }

    public void got__c_ns_alias_node(MatchInfo o)
    {
        var name = o.Text;
        if (name.StartsWith("*"))
            name = name[1..];
        Add(AliasEvent(name));
    }
}
