namespace SnapshotDiff.Features.ExclusionRules.Infrastructure;

/// <summary>
/// Fast glob-style pattern matching for exclusion rules.
/// Supports * (any sequence) and ? (single char) wildcards, case-insensitive.
/// Absolute-path patterns (starting with / or a drive letter) are matched as prefixes.
/// </summary>
internal static class PatternMatcher
{
    /// <summary>
    /// Returns true when <paramref name="name"/> (file/dir name) or
    /// <paramref name="fullPath"/> matches <paramref name="pattern"/>.
    /// </summary>
    public static bool Matches(string pattern, string name, string fullPath)
    {
        if (string.IsNullOrWhiteSpace(pattern)) return false;

        // Absolute path prefix: pattern starts with path separator or drive letter
        if (IsAbsolutePath(pattern))
            return fullPath.StartsWith(pattern.TrimEnd('/', '\\'),
                                       StringComparison.OrdinalIgnoreCase);

        // Path-segment pattern with directory separator → match against full path segments
        if (pattern.Contains('/') || pattern.Contains('\\'))
            return MatchGlob(pattern.Replace('/', Path.DirectorySeparatorChar)
                                    .Replace('\\', Path.DirectorySeparatorChar),
                             fullPath);

        // Simple name-only pattern
        return MatchGlob(pattern, name);
    }

    private static bool IsAbsolutePath(string pattern) =>
        pattern.StartsWith('/') ||
        pattern.StartsWith('\\') ||
        (pattern.Length >= 3 && pattern[1] == ':' && (pattern[2] == '\\' || pattern[2] == '/'));

    private static bool MatchGlob(string pattern, string input)
    {
        if (!pattern.Contains('*') && !pattern.Contains('?'))
            return string.Equals(pattern, input, StringComparison.OrdinalIgnoreCase);

        return MatchWildcard(pattern, input, 0, 0);
    }

    /// <summary>
    /// Iterative two-pointer wildcard matching (O(n*m) worst case).
    /// Avoids exponential backtracking of recursive approach.
    /// </summary>
    private static bool MatchWildcard(string pattern, string input, int pi, int ni)
    {
        int starPi = -1;
        int starNi = -1;

        while (ni < input.Length)
        {
            if (pi < pattern.Length && pattern[pi] == '*')
            {
                // Collapse consecutive stars
                while (pi < pattern.Length && pattern[pi] == '*') pi++;
                starPi = pi;
                starNi = ni;
            }
            else if (pi < pattern.Length &&
                     (pattern[pi] == '?' || char.ToLowerInvariant(pattern[pi]) == char.ToLowerInvariant(input[ni])))
            {
                pi++;
                ni++;
            }
            else if (starPi >= 0)
            {
                // Backtrack: let the last '*' consume one more character
                pi = starPi;
                ni = ++starNi;
            }
            else
            {
                return false;
            }
        }

        // Consume trailing stars
        while (pi < pattern.Length && pattern[pi] == '*') pi++;
        return pi == pattern.Length;
    }
}
