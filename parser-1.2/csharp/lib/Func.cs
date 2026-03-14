namespace YamlParser;

using System.Diagnostics;

/// <summary>
/// Wrapper for a grammar rule function, holding its delegate, name, trace info,
/// and cached receiver lookups.
/// </summary>
public class Func
{
    public Delegate Del { get; }
    public string Name { get; set; }
    public string Trace { get; set; }
    public string? Num { get; set; }
    public Dictionary<string, Action<Receiver, MatchInfo>?>? Receivers { get; set; }

    public Func(Delegate del, string name, string? trace = null)
    {
        Del = del;
        Name = name;
        Trace = trace ?? name;
    }

    public override string ToString() => Name;
}

public class MatchInfo
{
    public string Text { get; set; } = "";
    public ParserState State { get; set; } = null!;
    public int Start { get; set; }
}

public class ParserState
{
    public string? Name { get; set; }
    public bool Doc { get; set; }
    public int Lvl { get; set; }
    public int Beg { get; set; }
    public int? End { get; set; }
    public int? M { get; set; }
    public string? T { get; set; }
}
