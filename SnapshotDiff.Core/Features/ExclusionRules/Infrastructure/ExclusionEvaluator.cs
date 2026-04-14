using SnapshotDiff.Features.ExclusionRules.Domain;

namespace SnapshotDiff.Features.ExclusionRules.Infrastructure;

/// <summary>
/// Snapshot of rules used during a single scan. Immutable; created by <see cref="ExclusionService"/>.
/// </summary>
internal sealed class ExclusionEvaluator(IReadOnlyList<ExclusionRule> allRules) : IExclusionEvaluator
{
    private readonly IReadOnlyList<ExclusionRule> _systemRules =
        allRules.Where(r => r.Type == ExclusionRuleType.System).ToList();

    public bool IsExcluded(string fullPath, bool isDirectory)
    {
        var name = Path.GetFileName(fullPath.TrimEnd(Path.DirectorySeparatorChar,
                                                      Path.AltDirectorySeparatorChar));
        foreach (var rule in allRules)
        {
            if (!rule.IsEnabled) continue;
            if (rule.IsDirectoryOnly && !isDirectory) continue;
            if (PatternMatcher.Matches(rule.Pattern, name, fullPath))
                return true;
        }
        return false;
    }

    public bool IsSystemProtected(string fullPath)
    {
        var name = Path.GetFileName(fullPath.TrimEnd(Path.DirectorySeparatorChar,
                                                      Path.AltDirectorySeparatorChar));
        foreach (var rule in _systemRules)
        {
            if (!rule.IsEnabled) continue;
            if (PatternMatcher.Matches(rule.Pattern, name, fullPath))
                return true;
        }
        return false;
    }
}
