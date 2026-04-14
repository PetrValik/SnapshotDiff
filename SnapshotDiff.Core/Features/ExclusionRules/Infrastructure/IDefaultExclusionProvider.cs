using SnapshotDiff.Features.ExclusionRules.Domain;

namespace SnapshotDiff.Features.ExclusionRules.Infrastructure;

/// <summary>
/// Supplies the OS-specific built-in exclusion rules.
/// </summary>
public interface IDefaultExclusionProvider
{
    IReadOnlyList<ExclusionRule> GetSystemRules();
}
