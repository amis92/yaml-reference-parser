namespace YamlParser;

using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;

/// <summary>
/// Test runner that loads the official YAML test suite and validates parser output.
/// Outputs TAP (Test Anything Protocol) format.
/// </summary>
public class TestSuiteRunner
{
    private int _testNum;
    private int _passed;
    private int _failed;

    public static int Run(string suiteDir)
    {
        var runner = new TestSuiteRunner();
        var tests = runner.LoadAllTests(suiteDir);

        Console.WriteLine($"1..{tests.Count}");

        foreach (var test in tests)
        {
            runner.RunTest(test);
        }

        Console.WriteLine($"# Passed: {runner._passed}/{runner._testNum}");
        Console.WriteLine($"# Failed: {runner._failed}/{runner._testNum}");

        return runner._failed == 0 ? 0 : 1;
    }

    private List<TestCase> LoadAllTests(string suiteDir)
    {
        var files = Directory.GetFiles(suiteDir, "*.yaml")
            .OrderBy(f => f)
            .ToList();

        var allTests = new List<TestCase>();
        foreach (var file in files)
        {
            try
            {
                var tests = LoadTests(file);
                allTests.AddRange(tests);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"# Error loading {Path.GetFileName(file)}: {ex.Message}");
            }
        }
        return allTests;
    }

    private List<TestCase> LoadTests(string file)
    {
        var content = File.ReadAllText(file, Encoding.UTF8);
        var yaml = new YamlStream();
        using var reader = new StringReader(content);
        yaml.Load(reader);

        var result = new List<TestCase>();
        string? lastTree = null;
        bool? lastSkip = null;

        if (yaml.Documents.Count == 0) return result;

        var root = yaml.Documents[0].RootNode;
        if (root is not YamlSequenceNode seq) return result;

        foreach (var item in seq)
        {
            if (item is not YamlMappingNode map) continue;

            var test = new TestCase
            {
                File = Path.GetFileName(file),
                Name = GetString(map, "name") ?? "",
                Yaml = GetString(map, "yaml"),
                Tree = GetString(map, "tree"),
                Json = GetString(map, "json"),
                Fail = GetBool(map, "fail"),
                Skip = GetBool(map, "skip"),
            };

            // Inherit tree from previous test if not present
            if (test.Tree != null)
            {
                lastTree = test.Tree;
                lastSkip = test.Skip;
            }
            else if (test.Fail != true)
            {
                test.Tree = lastTree;
                // Inherit skip status
                if (test.Skip != true && lastSkip == true)
                    test.Skip = true;
            }

            result.Add(test);
        }

        return result;
    }

    private string? GetString(YamlMappingNode map, string key)
    {
        var keyNode = new YamlScalarNode(key);
        if (map.Children.TryGetValue(keyNode, out var value))
        {
            if (value is YamlScalarNode scalar)
                return scalar.Value;
        }
        return null;
    }

    private bool? GetBool(YamlMappingNode map, string key)
    {
        var s = GetString(map, key);
        if (s == null) return null;
        return s.ToLower() switch
        {
            "true" => true,
            "yes" => true,
            "1" => true,
            _ => false,
        };
    }

    private void RunTest(TestCase test)
    {
        _testNum++;
        var description = $"{test.File} - {test.Name}";

        if (test.Skip == true)
        {
            _passed++;
            Console.WriteLine($"ok {_testNum} - {description} # SKIP");
            return;
        }

        var yamlText = UnescapeYaml(test.Yaml ?? "");
        string? result = ParseYaml(yamlText);
        var expectedTree = NormalizeTree(test.Tree);
        var actualTree = NormalizeTree(result);

        if (test.Fail == true)
        {
            if (result == null)
            {
                _passed++;
                Console.WriteLine($"ok {_testNum} - {description}");
            }
            else
            {
                _failed++;
                Console.WriteLine($"not ok {_testNum} - {description}");
                Console.WriteLine("  # Expected parse to fail but it succeeded");
            }
        }
        else if (expectedTree == null && result == null)
        {
            _passed++;
            Console.WriteLine($"ok {_testNum} - {description}");
        }
        else if (expectedTree == null && result != null)
        {
            _failed++;
            Console.WriteLine($"not ok {_testNum} - {description}");
            Console.WriteLine($"  # No expected tree but parse succeeded");
        }
        else if (result == null)
        {
            _failed++;
            Console.WriteLine($"not ok {_testNum} - {description}");
            Console.WriteLine("  # Parse failed unexpectedly");
        }
        else if (actualTree == expectedTree)
        {
            _passed++;
            Console.WriteLine($"ok {_testNum} - {description}");
        }
        else
        {
            _failed++;
            Console.WriteLine($"not ok {_testNum} - {description}");
            Console.WriteLine("  # Expected:");
            if (expectedTree != null)
                foreach (var line in expectedTree.Split('\n'))
                    Console.WriteLine($"  #   {line}");
            Console.WriteLine("  # Actual:");
            if (actualTree != null)
                foreach (var line in actualTree.Split('\n'))
                    Console.WriteLine($"  #   {line}");
        }
    }

    private string? ParseYaml(string yamlText)
    {
        try
        {
            var receiver = new TestReceiver();
            var parser = new Parser(receiver);
            parser.Parse(yamlText);
            return receiver.Output();
        }
        catch
        {
            return null;
        }
    }

    private string? NormalizeTree(string? tree)
    {
        if (tree == null) return null;
        // Remove ∎ (U+220E) end marker and replace ␣ (U+2423) with space
        tree = tree.Replace("\u220E", "");
        tree = tree.Replace("\u2423", " ");
        var lines = tree.Split('\n')
            .Select(l => l.Trim(' ', '\t', '\r'))
            .Where(l => l.Length > 0)
            .ToArray();
        return string.Join("\n", lines);
    }

    private string UnescapeYaml(string text)
    {
        // ␣ (U+2423) -> space
        text = text.Replace("\u2423", " ");
        // —»  (U+2014 + U+00BB) -> tab (with optional leading dashes)
        text = Regex.Replace(text, @"\u2014*\u00BB", "\t");
        // ⇔ (U+21D4) -> BOM
        text = text.Replace("\u21D4", "\uFEFF");
        // ↵ (U+21B5) -> nothing
        text = text.Replace("\u21B5", "");
        // ∎ (U+220E) -> nothing (with optional trailing newline)
        text = Regex.Replace(text, @"\u220E\n?$", "");
        return text;
    }
}

public class TestCase
{
    public string File { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Yaml { get; set; }
    public string? Tree { get; set; }
    public string? Json { get; set; }
    public bool? Fail { get; set; }
    public bool? Skip { get; set; }
}
