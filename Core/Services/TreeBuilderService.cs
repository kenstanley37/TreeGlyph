using Core.Models;
using DotNet.Globbing;
using Infrastructure.Services;

namespace Core.Services;

public class TreeBuilderService
{
    private List<Glob> _exclusionGlobs = new();
    private string _rootPath = string.Empty;
    public List<string> SkippedPaths { get; } = new();

    private Action<string>? _logSkippedPath;

    public void SetSkippedPathLogger(Action<string> logger)
    {
        _logSkippedPath = logger;
    }

    public void SetExclusions(IEnumerable<string> patterns)
    {
        var patternList = patterns.ToList();

        LogService.Write("EXCLUSION", "SetExclusions was called");
        LogService.Write("EXCLUSION", "Raw patterns received:");

        foreach (var pattern in patternList)
        {
            LogService.Write("EXCLUSION", $"• {pattern}");
        }

        _exclusionGlobs = patternList
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => Glob.Parse(p.Trim().ToLowerInvariant()))
            .ToList();

        LogService.Write("EXCLUSION", $"Parsed {_exclusionGlobs.Count} globs");
    }

    public FileSystemEntry BuildTree(string rootPath, CancellationToken token)
    {
        _rootPath = rootPath;
        SkippedPaths.Clear();

        LogService.Write("TREEGEN", $"BuildTree started for: {rootPath}");
        LogService.Write("TREEGEN", "SkippedPaths cleared.");

        var root = new DirectoryInfo(rootPath);
        return BuildNode(root, token);
    }

    private FileSystemEntry BuildNode(DirectoryInfo dir, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        try
        {
            var relDir = GetRelativePathWithSlash(dir.FullName, isDirectory: true);
            if (IsExcluded(relDir))
            {
                var msg = $"Excluded folder: {relDir}";
                _logSkippedPath?.Invoke(msg);
                LogService.Write("TREEGEN", msg);
                return null!;
            }

            var node = new FileSystemEntry
            {
                Name = dir.Name,
                IsDirectory = true,
                Children = []
            };

            DirectoryInfo[] subDirs;
            try
            {
                subDirs = dir.GetDirectories();
            }
            catch (Exception ex)
            {
                var message = $"{dir.FullName} — {ex.GetType().Name}: {ex.Message}";
                SkippedPaths.Add(message);
                _logSkippedPath?.Invoke(message);
                LogService.Write("TREEGEN", $"Failed to access directory: {message}");
                return null!;
            }

            foreach (var subDir in subDirs)
            {
                token.ThrowIfCancellationRequested();

                var child = BuildNode(subDir, token);
                if (child is not null)
                    node.Children.Add(child);
            }

            FileInfo[] files;
            try
            {
                files = dir.GetFiles();
            }
            catch (Exception ex)
            {
                var message = $"{dir.FullName} — {ex.GetType().Name}: {ex.Message}";
                SkippedPaths.Add(message);
                _logSkippedPath?.Invoke(message);
                LogService.Write("TREEGEN", $"Failed to access files in: {message}");
                return node;
            }

            foreach (var file in files)
            {
                token.ThrowIfCancellationRequested();

                var relFile = GetRelativePathWithSlash(file.FullName, isDirectory: false);
                if (IsExcluded(relFile))
                {
                    var msg = $"Excluded file: {relFile}";
                    _logSkippedPath?.Invoke(msg);
                    LogService.Write("TREEGEN", msg);
                    continue;
                }

                node.Children.Add(new FileSystemEntry
                {
                    Name = file.Name,
                    IsDirectory = false
                });
            }

            return node;
        }
        catch (Exception ex)
        {
            var message = $"{dir.FullName} — {ex.GetType().Name}: {ex.Message}";
            SkippedPaths.Add(message);
            _logSkippedPath?.Invoke(message);
            LogService.Write("TREEGEN", $"Unexpected error: {message}");
            return null!;
        }
    }

    private string GetRelativePathWithSlash(string fullPath, bool isDirectory)
    {
        var rel = Path.GetRelativePath(_rootPath, fullPath).Replace('\\', '/');
        var normalized = isDirectory ? rel.TrimEnd('/') + "/" : rel;

        //LogService.Write("PATH", $"Normalized: {fullPath} → {normalized} (IsDirectory: {isDirectory})");

        return normalized;
    }

    public bool IsExcluded(string relativePath)
    {
        var normalized = relativePath.ToLowerInvariant();

        foreach (var glob in _exclusionGlobs)
        {
            if (glob.IsMatch(normalized))
            {
                var message = $"Excluded by: {glob} → {normalized}";
                _logSkippedPath?.Invoke(message);
                //LogService.Write("EXCLUSION", message);
                return true;
            }
        }

        //LogService.Write("EXCLUSION", $"Not excluded: {normalized}");
        return false;
    }
}