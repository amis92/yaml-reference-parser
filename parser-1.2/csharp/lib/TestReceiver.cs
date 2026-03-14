namespace YamlParser;

using System.Text;

public class TestReceiver : Receiver
{
    private static readonly Dictionary<string, string> EventMap = new()
    {
        ["stream_start"] = "+STR",
        ["stream_end"] = "-STR",
        ["document_start"] = "+DOC",
        ["document_end"] = "-DOC",
        ["mapping_start"] = "+MAP",
        ["mapping_end"] = "-MAP",
        ["sequence_start"] = "+SEQ",
        ["sequence_end"] = "-SEQ",
        ["scalar"] = "=VAL",
        ["alias"] = "=ALI",
    };

    private static readonly Dictionary<string, string> StyleMap = new()
    {
        ["plain"] = ":",
        ["single"] = "'",
        ["double"] = "\"",
        ["literal"] = "|",
        ["folded"] = ">",
    };

    public string Output()
    {
        var sb = new StringBuilder();
        foreach (var evt in Events)
        {
            if (!EventMap.TryGetValue(evt.Event, out var type))
                continue;

            var parts = new List<string> { type };

            if (type == "+DOC" && evt.Explicit == true)
                parts.Add("---");
            if (type == "-DOC" && evt.Explicit == true)
                parts.Add("...");
            if (type == "+MAP" && evt.Flow == true)
                parts.Add("{}");
            if (type == "+SEQ" && evt.Flow == true)
                parts.Add("[]");
            if (evt.Anchor != null)
                parts.Add($"&{evt.Anchor}");
            if (evt.Tag != null)
                parts.Add($"<{evt.Tag}>");
            if (evt.AliasName != null)
                parts.Add($"*{evt.AliasName}");
            if (evt.HasValue)
            {
                var style = StyleMap.GetValueOrDefault(evt.Style ?? "plain", ":");
                var value = evt.Value ?? "";
                value = value.Replace("\\", "\\\\");
                value = value.Replace("\b", "\\b");
                value = value.Replace("\t", "\\t");
                value = value.Replace("\n", "\\n");
                value = value.Replace("\r", "\\r");
                parts.Add($"{style}{value}");
            }
            sb.AppendLine(string.Join(" ", parts));
        }
        return sb.ToString();
    }
}
