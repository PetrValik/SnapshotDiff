using SnapshotDiff.Features.ExclusionRules.Domain;

namespace SnapshotDiff.Features.ExclusionRules.Infrastructure;

public interface IExclusionService
{
    /// <summary>
    /// Read-only system rules for the current OS.
    /// </summary>
    IReadOnlyList<ExclusionRule> GetSystemRules();

    /// <summary>
    /// User-defined global rules (apply to every scan).
    /// </summary>
    IReadOnlyList<ExclusionRule> GetGlobalUserRules();

    /// <summary>
    /// User-defined patterns for a specific watched directory.
    /// </summary>
    IReadOnlyList<string> GetPerDirectoryPatterns(string directoryPath);

    /// <summary>
    /// Returns an evaluator that combines system + global + per-directory rules
    /// for the given watched directory.
    /// </summary>
    IExclusionEvaluator GetEvaluatorForScan(string watchedDirectoryPath);

    Task AddGlobalRuleAsync(string pattern, string description = "", CancellationToken ct = default);
    Task RemoveGlobalRuleAsync(string ruleId, CancellationToken ct = default);
    Task ToggleGlobalRuleAsync(string ruleId, bool isEnabled, CancellationToken ct = default);

    Task AddPerDirectoryPatternAsync(string directoryPath, string pattern, CancellationToken ct = default);
    Task RemovePerDirectoryPatternAsync(string directoryPath, string pattern, CancellationToken ct = default);
}
