using Core.Models;
using Core.Services;
using TextCopy;

namespace Console;

class Program
{
    static void Main(string[] args)
    {
        var globalIgnorePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TreeGlyph",
            "ignore-global.txt");

        if (TryGetFlagValue(args, "--setglobal", out var ruleList))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(globalIgnorePath)!);
            var rules = ruleList.Split(',', StringSplitOptions.RemoveEmptyEntries);
            File.WriteAllLines(globalIgnorePath, rules);
            System.Console.WriteLine($"✅ Global ignore list saved to: {globalIgnorePath}");
            return;
        }

        if (args.Contains("--editglobal"))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(globalIgnorePath)!);
            if (!File.Exists(globalIgnorePath))
                File.WriteAllText(globalIgnorePath, "");
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = globalIgnorePath,
                UseShellExecute = true
            });
            System.Console.WriteLine($"📝 Opened global ignore file: {globalIgnorePath}");
            return;
        }

        if (args.Contains("--help"))
        {
            ShowHelp();
            return;
        }

        string folder = args.FirstOrDefault(arg => !arg.StartsWith("--")) ?? Directory.GetCurrentDirectory();

        var localIgnorePath = Path.Combine(folder, ".treeglyphignore");
        string[] localRules = File.Exists(localIgnorePath)
            ? File.ReadAllLines(localIgnorePath)
            : Array.Empty<string>();

        bool skipGlobalIgnore = args.Contains("--noglobal");
        string[] globalRules = skipGlobalIgnore || !File.Exists(globalIgnorePath)
            ? Array.Empty<string>()
            : File.ReadAllLines(globalIgnorePath);

        int depth = TryGetFlagValue(args, "--depth", out var val) ? int.Parse(val) : int.MaxValue;
        string? savePath = TryGetFlagValue(args, "--save", out var filePath) ? filePath : null;
        bool toClipboard = args.Contains("--clipboard");

        var cliExclusions = args
            .Where(arg => !arg.StartsWith("--") && arg != folder)
            .SelectMany(arg => arg.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .ToArray();

        var allRules = globalRules.Concat(localRules).Concat(cliExclusions);

        var builder = new TreeBuilderService();
        builder.SetExclusions(allRules);

        using var cts = new CancellationTokenSource();
        System.Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
            System.Console.WriteLine("\n⛔ Tree building canceled.");
        };

        var progressThread = new Thread(() =>
        {
            while (!cts.IsCancellationRequested)
            {
                System.Console.Write(".");
                Thread.Sleep(500);
            }
        });
        progressThread.Start();

        FileSystemEntry? root = null;
        try
        {
            root = builder.BuildTree(folder, cts.Token);
        }
        catch (OperationCanceledException)
        {
            System.Console.WriteLine("\n⛔ Operation canceled by user.");
        }

        progressThread.Join();

        if (root is null)
        {
            System.Console.WriteLine("\nNothing to render. Folder may be empty or excluded.");
            return;
        }

        System.Console.WriteLine("\n✅ Tree built successfully.\n");

        string tree = RenderAsciiTree(root, 0, depth);
        System.Console.WriteLine(tree);

        if (builder.SkippedPaths.Any())
        {
            System.Console.WriteLine("\n⚠️ Skipped folders:");
            foreach (var path in builder.SkippedPaths)
                System.Console.WriteLine($" - {path}");
        }

        if (toClipboard)
            ClipboardService.SetText(tree);

        if (!string.IsNullOrWhiteSpace(savePath))
            File.WriteAllText(savePath, tree);
    }

    static void ShowHelp()
    {
        System.Console.WriteLine("""
            Usage: treeglyph FOLDER [excludes] OPTIONS
            Example: treeglyph src bin,obj,node_modules --depth 2 --save tree.txt --clipboard

            Options:
              --depth [n]       Limit nesting depth
              --save [path]     Save output to file
              --clipboard       Copy output to clipboard
              --noglobal        Skip applying global ignore rules
              --setglobal [r]   Overwrite global ignore with comma-separated rules
              --editglobal      Open global ignore file in default text editor
              --help            Show this help
            """);
    }

    static bool TryGetFlagValue(string[] args, string flag, out string value)
    {
        int index = Array.IndexOf(args, flag);
        if (index >= 0 && index + 1 < args.Length)
        {
            value = args[index + 1];
            return true;
        }
        value = string.Empty;
        return false;
    }

    static string RenderAsciiTree(FileSystemEntry node, int level, int maxDepth, string indent = "", bool isLast = true)
    {
        if (level >= maxDepth) return "";

        var prefix = indent + (isLast ? "└── " : "├── ");
        var output = prefix + node.Name + (node.IsDirectory ? "/" : "") + "\n";

        if (node.Children?.Count > 0)
        {
            for (int i = 0; i < node.Children.Count; i++)
            {
                bool lastChild = i == node.Children.Count - 1;
                var newIndent = indent + (isLast ? "    " : "│   ");
                output += RenderAsciiTree(node.Children[i], level + 1, maxDepth, newIndent, lastChild);
            }
        }

        return output;
    }
}