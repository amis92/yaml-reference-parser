#!/usr/bin/env dotnet-script

#r "nuget: VYaml, 0.25.0"
#r "nuget: System.CommandLine, 2.0.0-beta4.22272.1"
#r "nuget: System.Memory.Data, 8.0.0"
#r "nuget: Microsoft.Extensions.Logging.Console, 8.0.0"

#nullable enable

using System.Buffers;
using System.Collections.Immutable;
using System.CommandLine;
using System.CommandLine.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VYaml;

const string Version = "1.0.0";

var fileOption = new Option<FileInfo>(
        ["--file", "-f"],
        "File path of 'yaml-spec-#.#.yaml")
{
    IsRequired = true,
};
var debugOutputOption = new Option<FileInfo>(
        ["----debug-output"],
        description: "Debug output file path",
        getDefaultValue: () => new("/tmp/yaml-grammar.cs"))
{
    IsHidden = true,
};
var outputOption = new Option<FileInfo>(
        ["--output", "-o"],
        description: "Output file path, if not specified, output to stdout.");
var ruleOption = new Option<string>(
        ["--rule", "-r"],
        description: "Starting rule of grammar")
{
    IsRequired = true,
};
var verbosityOption = new Option<LogLevel?>(
        ["--verbosity", "-v"],
        description: "Set the logging verbosity level.",
        isDefault: true,
        parseArgument: result =>
        {
            if (result.Tokens.Count == 0)
            {
                return LogLevel.Debug;
            }
            var token = result.Tokens[0];
            return Enum.Parse<LogLevel>(token.Value);
        })
{
    Arity = ArgumentArity.ZeroOrOne,
};
var command = new RootCommand("YAML Grammar Generator for C#")
{
    ruleOption,
    fileOption,
    outputOption,
    debugOutputOption,
    verbosityOption,
};
command.SetHandler(async context =>
{
    var cancellationToken = context.GetCancellationToken();
    cancellationToken.ThrowIfCancellationRequested();
    var rule = context.ParseResult.GetValueForOption(ruleOption)
        ?? throw new InvalidOperationException("Rule is required.");
    var file = context.ParseResult.GetValueForOption(fileOption)
        ?? throw new InvalidOperationException("File is required.");
    var output = context.ParseResult.GetValueForOption(outputOption);
    var debugOutput = context.ParseResult.GetValueForOption(debugOutputOption)
        ?? throw new InvalidOperationException("Debug output is required.");
    var verbosity = context.ParseResult.GetValueForOption(verbosityOption)
        ?? LogLevel.Information;
    var grammar = await TryCreateGrammar();
    if (grammar is null)
    {
        return;
    }
    if (output is not null)
    {
        await WriteAsync(output, grammar, cancellationToken);
    }
    else
    {
        context.Console.Out.WriteLine(grammar.ToString());
    }
    context.ExitCode = 0;
    return;

    async Task<BinaryData?> TryCreateGrammar()
    {
        var spec = await ReadAsync(file, cancellationToken);
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(verbosity);
        });
        var generator = new GrammarGenerator(spec, loggerFactory);
        try
        {
            var grammar = await generator.GenerateGrammarCSharpAsync(rule, cancellationToken);
            await WriteAsync(debugOutput, grammar, cancellationToken);
            return grammar;
        }
        catch (Exception ex)
        {
            var partialOutput = BinaryData.FromString(generator.Builder.ToString());
            await WriteAsync(debugOutput, partialOutput, cancellationToken);
            context.Console.Error.WriteLine(ex.ToString());
            context.ExitCode = 1;
            return null;
        }
    }

    static async Task<BinaryData> ReadAsync(FileInfo file, CancellationToken cancellationToken)
    {
        using var fileStream = file.OpenRead();
        var content = await BinaryData.FromStreamAsync(fileStream, cancellationToken);
        return content;
    }

    static async Task WriteAsync(FileInfo file, BinaryData content, CancellationToken cancellationToken)
    {
        using var fileStream = file.OpenWrite();
        await fileStream.WriteAsync(content, cancellationToken);
    }
});

return await command.InvokeAsync(Args.ToArray());

public class GrammarGenerator(BinaryData YamlSpec, ILoggerFactory? loggerFactory = null)
{
    private ILogger Logger { get; } =
        loggerFactory?.CreateLogger<GrammarGenerator>() ?? new NullLogger<GrammarGenerator>();
    /// <summary>Key: rule name; Value: rule number.</summary>
    Dictionary<string, string> NamesIndex { get; set; } = [];
    public BinaryData YamlSpec { get; } = YamlSpec;
    public StringBuilder Builder { get; } = new();

    public async Task<BinaryData> GenerateGrammarCSharpAsync(string rule, CancellationToken cancellationToken)
    {
        Builder.Clear();
        cancellationToken.ThrowIfCancellationRequested();
        var yaml = ParseYamlStream(new(YamlSpec.ToMemory()));
        var yamlSpecDocument = yaml.Documents.Single();
        var root = (YamlMappingNode)yamlSpecDocument.RootNode!;
        NamesIndex = root.Children.Where(x => x.Key!.ToString()[0] == ':')
            .ToDictionary(x => x.Value!.ToString(), x => x.Key!.ToString()[1..]);

        Logger.LogDebug($"Generating grammar for rule YAML spec rule {rule}.");
        // prefix
        Builder.AppendLine($$"""
            // This grammar was generated from https://yaml.org/spec/1.2/spec.html

            using System;
            using System.Collections.Generic;
            using System.Linq;

            namespace YamlSharp;

            partial struct YamlGrammar
            {
                public YamlParseResult Top() => {{SafeRuleName(rule)}}();
            """);

        foreach (var (ruleName, ruleIndex) in NamesIndex)
        {
            var ruleNode = root.Children.First(x => x.Key!.ToString() == ruleName).Value!;
            CreateRuleMethod(ruleName, ruleIndex, ruleNode);
        }
        // suffix
        Builder.AppendLine(@"""
            }
            """);
        Logger.LogDebug("Grammar generated.");
        await Task.CompletedTask;
        return BinaryData.FromString(Builder.ToString());
    }

    static string SafeRuleName(string ruleName) => Regex.Replace(ruleName, "[^a-zA-Z0-9]", "_");

    // private void CreateRuleMethod(string ruleName, string ruleIndex, YamlNode ruleNode)
    // {
    //     Logger.LogDebug($"Creating method for rule {ruleName}.");
    //     Builder.AppendLine($$"""
    //             public YamlParseResult {{SafeRuleName(ruleName)}}()
    //             {
    //                 var start = _index;
    //                 var result = {{SafeRuleName(ruleIndex)}}();
    //                 if (result.Success)
    //                 {
    //                     return result;
    //                 }
    //                 _index = start;
    //                 return YamlParseResult.Fail();
    //             }
    //         """);
    // }
    JsonSerializerOptions PrettyJsonOptions { get; } = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    bool IsRule(string name) => NamesIndex.ContainsKey(name);

    void CreateRuleMethod(string ruleName, string ruleIndex, YamlNode ruleNode)
    {
        Logger.LogDebug($"Creating method for rule {ruleIndex}: {ruleName}.");
        var children = ParseGrammarExpression(ruleNode);
        var ruleDefinition = new RuleDefinitionExpression(ruleName)
        {
            Expressions = [.. children],
        };
        Builder.AppendLine($$"""
            /*
            {{JsonSerializer.Serialize(ruleDefinition, PrettyJsonOptions)}}
            */
            """);
    }

    IEnumerable<GrammarExpressionBase> ParseGrammarExpression(YamlNode? node) => node switch
    {
        YamlScalarNode scalarNode => [ParseScalarExpression(scalarNode)],
        YamlSequenceNode sequenceNode => ParseSequenceExpression(sequenceNode),
        YamlMappingNode mappingNode => ParseMappingExpression(mappingNode),
        _ => throw new NotSupportedException($"Rule node {node?.GetType().Name ?? "null"} is not supported."),
    };

    GrammarExpressionBase ParseScalarExpression(YamlScalarNode node) => node switch
    {
        { Value: [_] value } => new LiteralCharacterExpression(value),
        { Value: ['x', _, ..] value } => new LiteralCharacterExpression(value),
        { Value: ['(', _, ..] value } => new ParsingFunctionExpression(value),
        { Value: ['<', _, ..] value } => new LiteralCharacterExpression(value),
        { Value: { } value } when IsRule(value) => new RuleNameExpression(value),
        { Value: { } value } => new LiteralStringExpression(value),
        _ => throw new NotSupportedException($"Rule scalar is not supported. Value: '{node?.Value}'."),
    };

    IEnumerable<GrammarExpressionBase> ParseSequenceExpression(YamlSequenceNode node)
    {
        foreach (var item in node.Children)
        {
            if (item is YamlSequenceNode seq)
            {
                var result = ParseSequenceExpression(seq).ToList();
                if (result is [LiteralCharacterExpression first, LiteralCharacterExpression last])
                {
                    yield return new LiteralRangeExpression(first.Value, last.Value);
                }
                else
                {
                    throw new NotSupportedException($"Rule sequence with {result.Count} elements is not supported.");
                }
            }
            else
            {
                var childExpressions = ParseGrammarExpression(item);
                foreach (var child in childExpressions)
                {
                    yield return child;
                }
            }
        }
    }

    IEnumerable<GrammarExpressionBase> ParseMappingExpression(YamlMappingNode node)
    {
        foreach (var (key, value) in node.Children)
        {
            if (key is YamlScalarNode scalarNode)
            {
                var keyExpression = ParseScalarExpression(scalarNode);
                var valueExpression = ParseGrammarExpression(value).ToList();
                var result = keyExpression with { Expressions = [.. valueExpression] };
                yield return result;
            }
            else
            {
                throw new NotSupportedException($"Rule mapping with non-scalar key is not supported.");
            }
        }
    }

    [JsonPolymorphic]
    [JsonDerivedType(typeof(RuleDefinitionExpression), "ruleDef")]
    [JsonDerivedType(typeof(RuleNameExpression), "rule")]
    [JsonDerivedType(typeof(VariableExpression), "var")]
    [JsonDerivedType(typeof(LiteralCharacterExpression), "char")]
    [JsonDerivedType(typeof(LiteralRangeExpression), "range")]
    [JsonDerivedType(typeof(LiteralStringExpression), "literal")]
    [JsonDerivedType(typeof(ParsingFunctionExpression), "fun")]
    [JsonDerivedType(typeof(SpecialRuleExpression), "assert")]
    abstract record GrammarExpressionBase
    {
        public ImmutableArray<GrammarExpressionBase> Expressions { get; init; } = [];
    }

    /// <summary>Definition of a grammar rule.</summary>
    record RuleDefinitionExpression(string Name) : GrammarExpressionBase;

    /// <summary>Name of a grammar rule.</summary>
    record RuleNameExpression(string Name) : GrammarExpressionBase;

    /// <summary>Single-character name of grammar variable.</summary>
    record VariableExpression(string Name) : GrammarExpressionBase;

    /// <summary>Single-character literal, including hex-code character.</summary>
    record LiteralCharacterExpression(string Value) : GrammarExpressionBase;

    /// <summary>Range of literals, as a pair of character literals.</summary>
    record LiteralRangeExpression(string Start, string End) : GrammarExpressionBase;

    /// <summary>Multi-character literal.</summary>
    record LiteralStringExpression(string Value) : GrammarExpressionBase;

    /// <summary>Parsing function name (with parentheses).</summary>
    record ParsingFunctionExpression(string Name) : GrammarExpressionBase;

    /// <summary>Special rule name (with angle brackets).</summary>
    record SpecialRuleExpression(string Name) : GrammarExpressionBase;

    // -------------------------------------
    // VYaml Node parser
    // -------------------------------------

    static YamlStream ParseYamlStream(in ReadOnlySequence<byte> utf8yaml)
    {
        var parser = VYaml.Parser.YamlParser.FromSequence(utf8yaml);
        var stream = new YamlStream([]);
        var document = default(YamlDocument);
        var nodeStack = ImmutableStack<YamlNode>.Empty;
        while (!parser.End && parser.Read())
        {
            VYaml.Parser.ParseEventType eventType = parser.CurrentEventType;
            switch (eventType)
            {
                case VYaml.Parser.ParseEventType.StreamStart:
                    Debug.Assert(document is null);
                    Debug.Assert(nodeStack.IsEmpty);
                    break;
                case VYaml.Parser.ParseEventType.StreamEnd:
                    Debug.Assert(document is null);
                    Debug.Assert(nodeStack.IsEmpty);
                    break;
                case VYaml.Parser.ParseEventType.DocumentStart:
                    Debug.Assert(document is null);
                    document = new YamlDocument(null);
                    Debug.Assert(nodeStack.IsEmpty);
                    break;
                case VYaml.Parser.ParseEventType.DocumentEnd:
                    {
                        var rootNode = default(YamlNode);
                        nodeStack = nodeStack.IsEmpty ? nodeStack : nodeStack.Pop(out rootNode);
                        if (!nodeStack.IsEmpty)
                        {
                            throw new InvalidOperationException($"Node stack is not empty at the end of document, parser at {parser.CurrentMark}.");
                        }
                        if (document is null)
                        {
                            throw new InvalidOperationException($"Document end event without start, parser at {parser.CurrentMark}.");
                        }
                        stream = stream with { Documents = [.. stream.Documents, document with { RootNode = rootNode }] };
                        document = default;
                        break;
                    }
                case VYaml.Parser.ParseEventType.Alias:
                    // ignoring, spec doesn't use tags/anchors/aliases
                    break;
                case VYaml.Parser.ParseEventType.Scalar:
                    {
                        var node = new YamlScalarNode(parser.GetScalarAsString());
                        nodeStack = WithNode(nodeStack, node, ref parser);
                        break;
                    }
                case VYaml.Parser.ParseEventType.SequenceStart:
                    nodeStack = nodeStack.Push(new YamlSequenceNode([]));
                    break;
                case VYaml.Parser.ParseEventType.SequenceEnd:
                    {
                        nodeStack = nodeStack.Pop(out var node);
                        Debug.Assert(node is YamlSequenceNode);
                        nodeStack = WithNode(nodeStack, node, ref parser);
                        break;
                    }
                case VYaml.Parser.ParseEventType.MappingStart:
                    nodeStack = nodeStack.Push(new YamlMappingNode([]));
                    break;
                case VYaml.Parser.ParseEventType.MappingEnd:
                    {
                        nodeStack = nodeStack.Pop(out var node);
                        Debug.Assert(node is YamlMappingNode);
                        nodeStack = WithNode(nodeStack, node, ref parser);
                        break;
                    }
                default:
                    throw new InvalidOperationException($"Unknown event type {eventType}.");
            }
        }
        return stream;

        static ImmutableStack<YamlNode> WithNode(
            ImmutableStack<YamlNode> stack,
            YamlNode node,
            ref VYaml.Parser.YamlParser parser) => stack switch
            {
                null => throw new ArgumentNullException(nameof(stack), $"Stack is null, parser at mark: {parser.CurrentMark}"),
                { IsEmpty: true } =>
                    stack.Push(node),
                { } when stack.Peek() is YamlSequenceNode seq =>
                    stack.Pop().Push(seq with { Children = seq.Children.Add(node) }),
                { } when stack.Peek() is YamlMappingNode =>
                    stack.Push(node),
                { } when stack.Pop(out var prevNode) is { IsEmpty: false } prevStack && prevStack.Peek() is YamlMappingNode map =>
                    prevStack.Pop().Push(map with { Children = map.Children.Add(new YamlMappingPair(prevNode, node)) }),
                _ => throw new InvalidOperationException($"Unexpected stack state at {parser.CurrentMark}: '{string.Join(", ", stack.Select(x => x.ToString()))}'."),
            };
    }

    record YamlStream(ImmutableArray<YamlDocument> Documents);

    record YamlDocument(YamlNode? RootNode);

    abstract record YamlNode();

    record YamlScalarNode(string? Value) : YamlNode
    {
        public override string ToString()
        {
            return Value ?? "";
        }
    }

    record YamlSequenceNode(ImmutableArray<YamlNode> Children) : YamlNode;

    record YamlMappingNode(ImmutableArray<YamlMappingPair> Children) : YamlNode;

    record YamlMappingPair(YamlNode? Key, YamlNode? Value) : YamlNode;
}