using System.Text;
using TreeGlyph.UI.Models;

namespace TreeGlyph.UI.Helpers
{
    public static class FileSystemEntryExtensions
    {
        public static string ToAsciiTree(this FileSystemEntry entry, string indent = "", bool isLast = true)
        {
            var result = new StringBuilder();
            var connector = isLast ? "└── " : "├── ";
            result.AppendLine(indent + connector + entry.Name);

            indent += isLast ? "    " : "│   ";

            for (int i = 0; i < entry.Children.Count; i++)
            {
                var child = entry.Children[i];
                bool childIsLast = i == entry.Children.Count - 1;
                result.Append(child.ToAsciiTree(indent, childIsLast));
            }

            return result.ToString();
        }
    }
}
