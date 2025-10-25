using Core.Models;
using System.Text;

namespace Core.Helpers
{
    public static class FileSystemEntryExtensions
    {
        public static string ToAsciiTree(this FileSystemEntry entry, int level = 0, int maxDepth = int.MaxValue, string indent = "", bool isLast = true)
        {
            if (level >= maxDepth) return "";

            var result = new StringBuilder();
            var connector = isLast ? "└── " : "├── ";
            result.AppendLine(indent + connector + entry.Name);

            indent += isLast ? "    " : "│   ";

            for (int i = 0; i < entry.Children.Count; i++)
            {
                var child = entry.Children[i];
                bool childIsLast = i == entry.Children.Count - 1;
                result.Append(child.ToAsciiTree(level + 1, maxDepth, indent, childIsLast));
            }

            return result.ToString();
        }
    }
}
