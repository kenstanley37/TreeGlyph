using DotNet.Globbing;
using Core.Models;

namespace Core.Services;

public class TreeBuilderService
{
    private List<Glob> _exclusionGlobs = new();
    private string _rootPath = string.Empty;

    public void SetExclusions(IEnumerable<string> patterns)
    {
        _exclusionGlobs = patterns
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => Glob.Parse(p.Trim()))
            .ToList();
    }

    public FileSystemEntry BuildTree(string rootPath)
    {
        _rootPath = rootPath;
        var root = new DirectoryInfo(rootPath);
        return BuildNode(root);
    }

    private FileSystemEntry BuildNode(DirectoryInfo dir)
    {
        var relDir = GetRelativePathWithSlash(dir.FullName);
        if (IsExcluded(relDir)) return null!;

        var node = new FileSystemEntry
        {
            Name = dir.Name,
            IsDirectory = true,
            Children = []
        };

        foreach (var subDir in dir.GetDirectories())
        {
            var child = BuildNode(subDir);
            if (child is not null)
                node.Children.Add(child);
        }

        foreach (var file in dir.GetFiles())
        {
            var relFile = GetRelativePathWithSlash(file.FullName);
            if (IsExcluded(relFile)) continue;

            node.Children.Add(new FileSystemEntry
            {
                Name = file.Name,
                IsDirectory = false
            });
        }

        return node;
    }

    private string GetRelativePathWithSlash(string fullPath)
    {
        var rel = Path.GetRelativePath(_rootPath, fullPath)
                      .Replace('\\', '/');
        if (Directory.Exists(fullPath))
            rel += "/";
        return rel;
    }

    public bool IsExcluded(string relativePath)
    {
        return _exclusionGlobs.Any(glob => glob.IsMatch(relativePath));
    }
}