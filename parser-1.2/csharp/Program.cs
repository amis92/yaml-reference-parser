namespace YamlParser;

using System.Text;

class Program
{
    static int Main(string[] args)
    {
        bool eventsOnly = false;
        string? filePath = null;
        string? testSuiteDir = null;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--events")
                eventsOnly = true;
            else if (args[i] == "--test-suite" && i + 1 < args.Length)
                testSuiteDir = args[++i];
            else
                filePath = args[i];
        }

        if (testSuiteDir != null)
            return TestSuiteRunner.Run(testSuiteDir);

        string yaml;
        if (filePath != null)
        {
            yaml = File.ReadAllText(filePath, Encoding.UTF8);
        }
        else
        {
            using var reader = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8);
            yaml = reader.ReadToEnd();
        }

        var receiver = new TestReceiver();
        var parser = new Parser(receiver);

        bool pass = true;
        var start = DateTime.UtcNow;

        try
        {
            parser.Parse(yaml);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
            pass = false;
        }

        var time = (DateTime.UtcNow - start).TotalSeconds;

        if (eventsOnly)
        {
            Console.Write(receiver.Output());
            return pass ? 0 : 1;
        }

        var displayYaml = yaml;
        string prefix;
        if (yaml.Contains('\n') && yaml.IndexOf('\n') < yaml.Length - 1)
        {
            prefix = "\n";
        }
        else
        {
            prefix = "";
            displayYaml = displayYaml.TrimEnd('\n') + "\\n";
        }

        if (pass)
        {
            Console.WriteLine($"PASS - '{prefix}{displayYaml}'");
            Console.Write(receiver.Output());
            Console.WriteLine($"Parse time {time:F5}s");
            return 0;
        }
        else
        {
            Console.WriteLine($"FAIL - '{prefix}{displayYaml}'");
            Console.Write(receiver.Output());
            Console.WriteLine($"Parse time {time:F5}s");
            return 1;
        }
    }
}
