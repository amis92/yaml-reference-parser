#!/usr/bin/env dotnet-script

#r "nuget: YamlDotNet, 15.1.2"
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
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using YamlDotNet;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

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
    public BinaryData YamlSpec { get; } = YamlSpec;
    public StringBuilder Builder { get; } = new();

    public async Task<BinaryData> GenerateGrammarCSharpAsync(string rule, CancellationToken cancellationToken)
    {
        Builder.Clear();
        cancellationToken.ThrowIfCancellationRequested();
        var yaml = new YamlStream();
        yaml.Load(new StreamReader(YamlSpec.ToStream()));
        var yamlSpecDocument = yaml.Documents.Single();
        var ruleNodes = (YamlMappingNode)yamlSpecDocument.RootNode;
        var namesIndex = ruleNodes.Where(x => x.Key.ToString()[0] == ':')
            .ToDictionary(x => x.Value.ToString(), x => x.Key.ToString()[1..]);

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

        foreach (var (ruleName, ruleIndex) in namesIndex)
        {
            var ruleNode = ruleNodes[ruleName];
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
    };

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

    IEnumerable<GrammarExpressionBase> ParseGrammarExpression(YamlNode node)
    {
        return node switch
        {
            YamlScalarNode scalarNode => [ParseScalarExpression(scalarNode)],
            YamlSequenceNode sequenceNode => ParseSequenceExpression(sequenceNode),
            YamlMappingNode mappingNode => ParseMappingExpression(mappingNode),
            _ => throw new NotSupportedException($"Rule node type {node.GetType()} is not supported."),
        };
    }

    GrammarExpressionBase ParseScalarExpression(YamlScalarNode node)
    {
        return node switch
        {
            { Style: ScalarStyle.DoubleQuoted, Value: { } value } => new LiteralStringExpression(value),
            { Style: ScalarStyle.SingleQuoted, Value: { } value } => new LiteralCharacterExpression(value),
            { Style: ScalarStyle.Plain, Value: [_] value } => new LiteralCharacterExpression(value),
            { Style: ScalarStyle.Plain, Value: ['x', _, ..] value } => new LiteralCharacterExpression(value),
            { Style: ScalarStyle.Plain, Value: ['(', _, ..] value } => new ParsingFunctionExpression(value),
            { Style: ScalarStyle.Plain, Value: ['<', _, ..] value } => new LiteralCharacterExpression(value),
            { Style: ScalarStyle.Plain, Value: { } value } => new RuleNameExpression(value),
            _ => throw new NotSupportedException($"Rule scalar (style={node?.Style}) is not supported. Value: '{node?.Value}'."),
        };
    }

    IEnumerable<GrammarExpressionBase> ParseSequenceExpression(YamlSequenceNode node)
    {
        foreach (var item in node.Children)
        {
            var result = ParseGrammarExpression(item).ToList();
            if (result.Count == 1)
            {
                yield return result[0];
            }
            else if (result is [LiteralCharacterExpression first, LiteralCharacterExpression last])
            {
                yield return new LiteralRangeExpression(first.Value, last.Value);
            }
            else
            {
                throw new NotSupportedException($"Rule sequence with {result.Count} elements is not supported.");
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

    abstract record GrammarExpressionBase(GrammarExpressionType Type)
    {
        public ImmutableArray<GrammarExpressionBase> Expressions { get; init; } = [];
    }

    record RuleDefinitionExpression(string Name) : GrammarExpressionBase(GrammarExpressionType.Rule);

    record RuleNameExpression(string Name) : GrammarExpressionBase(GrammarExpressionType.RuleName);

    record VariableExpression(string Name) : GrammarExpressionBase(GrammarExpressionType.Variable);

    record LiteralCharacterExpression(string Value) : GrammarExpressionBase(GrammarExpressionType.LiteralCharacter);

    record LiteralRangeExpression(string Start, string End) : GrammarExpressionBase(GrammarExpressionType.LiteralRange);

    record LiteralStringExpression(string Value) : GrammarExpressionBase(GrammarExpressionType.LiteralString);

    record ParsingFunctionExpression(string Name) : GrammarExpressionBase(GrammarExpressionType.ParsingFunction);

    record SpecialRuleExpression(string Name) : GrammarExpressionBase(GrammarExpressionType.SpecialRule);

    enum GrammarExpressionType
    {
        /// <summary>Definition of a grammar rule.</summary>
        Rule,
        /// <summary>Name of a grammar rule.</summary>
        RuleName,
        /// <summary>Single-character name of grammar variable.</summary>
        Variable,
        /// <summary>Single-character literal, including hex-code character.</summary>
        LiteralCharacter,
        /// <summary>Range of literals, as a pair of character literals.</summary>
        LiteralRange,
        /// <summary>Multi-character literal.</summary>
        LiteralString,
        /// <summary>Parsing function name (with parentheses).</summary>
        ParsingFunction,
        /// <summary>Special rule name (with angle brackets).</summary>
        SpecialRule,
    }
}