using System.Text.RegularExpressions;

namespace TreeGlyph.UI.Helpers;

public class GlobMatcher
{
    private readonly List<Func<string, bool>> matchers = [];

    public GlobMatcher(IEnumerable<string> rules)
    {
        foreach (var line in rules)
        {
            var trimmed = line.Trim();

            if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                continue;

            bool isNegation = trimmed.StartsWith("!");
            string pattern = isNegation ? trimmed[1..] : trimmed;

            string regex = "^" + Regex.Escape(pattern)
                .Replace(@"\*", ".*")
                .Replace(@"\?", ".") + "$";

            var expr = new Regex(regex, RegexOptions.IgnoreCase);

            if (isNegation)
                matchers.Add(path => !expr.IsMatch(path));
            else
                matchers.Add(path => expr.IsMatch(path));
        }
    }

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