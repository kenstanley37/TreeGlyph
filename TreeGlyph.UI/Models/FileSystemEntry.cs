namespace TreeGlyph.UI.Models;

public class FileSystemEntry
{
    public required string Name { get; init; }
    public required bool IsDirectory { get; init; }
    public List<FileSystemEntry> Children { get; init; } = [];

    public string ToAscii(string indent = "", bool isLast = true)
    {
        var prefix = indent + (isLast ? "└── " : "├── ");
        var result = $"{prefix}{Name}\n";

        for (int i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            var nextIndent = indent + (isLast ? "    " : "│   ");
            result += child.ToAscii(nextIndent, i == Children.Count - 1);
        }

        return result;
    }
}