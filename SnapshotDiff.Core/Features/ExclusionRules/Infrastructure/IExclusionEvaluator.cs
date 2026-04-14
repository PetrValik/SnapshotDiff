using SnapshotDiff.Features.ExclusionRules.Domain;

namespace SnapshotDiff.Features.ExclusionRules.Infrastructure;

/// <summary>
/// Evaluates whether a given path should be excluded from a scan.
/// Obtained via <see cref="IExclusionService.GetEvaluatorForScan"/>.
/// </summary>
public interface IExclusionEvaluator
{
    bool IsExcluded(string fullPath, bool isDirectory);

    /// <summary>
    /// Returns true if the path matches a <see cref="ExclusionRuleType.System"/> rule.
    /// Used to decide whether a stricter delete confirmation is required.
    /// </summary>
    bool IsSystemProtected(string fullPath);
}
