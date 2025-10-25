using System.Text.RegularExpressions;

namespace Core.Helpers;

public class GlobMatcher
{
    private readonly List<Func<string, bool>> matchers = [];

    public GlobMatcher(IEnumerable<string> rules)
    {
        foreach (var line in rules)
        {
            var trimmed = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
                continue;

            bool isNegation = trimmed.StartsWith("!");
            string pattern = isNegation ? trimmed[1..] : trimmed;

            string regex = "^" + Regex.Escape(pattern)
                .Replace(@"\*", ".*")
                .Replace(@"\?", ".") + "$";

            var expr = new Regex(regex, RegexOptions.IgnoreCase);

            matchers.Add(path => isNegation ? !expr.IsMatch(path) : expr.IsMatch(path));
        }
    }

    /// <summary>
    /// Checks whether the path should be excluded based on rules.
    /// </summary>
    public bool IsExcluded(string relativePath) => ShouldExclude(relativePath);

    public bool ShouldExclude(string relativePath)
    {
        foreach (var match in matchers)
        {
            if (match(relativePath))
                return true;
        }
        return false;
    }
}