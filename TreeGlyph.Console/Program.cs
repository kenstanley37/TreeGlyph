using TextCopy;
using TreeGlyph.Core.Models;
using TreeGlyph.Core.Services;

namespace TreeGlyph.Console;

class Program
{
    static void Main(string[] args)
    {
        if (args.Contains("--help"))
        {
            ShowHelp();
            return;
        }

        // 👉 Declare folder FIRST
        string folder = args.FirstOrDefault(arg => !arg.StartsWith("--")) ?? Directory.GetCurrentDirectory();

        var ignoreFilePath = Path.Combine(folder, ".treeglyphignore");
        string[] ignoreRules = File.Exists(ignoreFilePath)
            ? File.ReadAllLines(ignoreFilePath)
            : Array.Empty<string>();

        int depth = TryGetFlagValue(args, "--depth", out var val) ? int.Parse(val) : int.MaxValue;
        string? savePath = TryGetFlagValue(args, "--save", out var filePath) ? filePath : null;
        bool toClipboard = args.Contains("--clipboard");

        var exclusions = args
            .Where(arg => !arg.StartsWith("--") && arg != folder)
            .SelectMany(arg => arg.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .ToArray();

        var builder = new TreeBuilderService();
        builder.SetExclusions(ignoreRules.Concat(exclusions));

        var root = builder.BuildTree(folder);
        if (root is null)
        {
            System.Console.WriteLine("Nothing to render. Folder may be empty or excluded.");
            return;
        }

        string tree = RenderAsciiTree(root, 0, depth);
        System.Console.WriteLine(tree);

        if (toClipboard)
            ClipboardService.SetText(tree);

        if (!string.IsNullOrWhiteSpace(savePath))
            File.WriteAllText(savePath, tree);
    }

    static void ShowHelp()
    {
        System.Console.WriteLine("""
        Usage: treeglyph [folder] [excludes] [options]
        Example: treeglyph src bin,obj,node_modules --depth 2 --save tree.txt --clipboard

        Options:
          --depth [n]       Limit nesting depth
          --save [path]     Save output to file
          --clipboard       Copy output to clipboard
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