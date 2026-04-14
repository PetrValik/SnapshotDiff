using SnapshotDiff.Features.ExclusionRules.Domain;
using SnapshotDiff.Features.ExclusionRules.Infrastructure;

namespace SnapshotDiff.Features.ExclusionRules.Application.Queries;

/// <summary>
/// Queries the current exclusion rules, optionally scoped to a specific watched directory.
/// </summary>
/// <param name="DirectoryPath">
/// When provided, per-directory patterns for this directory are included in the result.
/// Pass <see langword="null"/> to skip per-directory patterns.
/// </param>
public sealed record GetExclusionRulesQuery(string? DirectoryPath = null);

/// <summary>
/// Combined result returned by <see cref="GetExclusionRulesHandler"/>,
/// grouping rules by their origin.
/// </summary>
/// <param name="SystemRules">Built-in, read-only OS-specific rules.</param>
/// <param name="GlobalUserRules">User-defined rules that apply to every scan.</param>
/// <param name="PerDirectoryPatterns">Raw pattern strings scoped to a specific directory. Empty when no directory was requested.</param>
public sealed record ExclusionRulesResult(
    IReadOnlyList<ExclusionRule> SystemRules,
    IReadOnlyList<ExclusionRule> GlobalUserRules,
    IReadOnlyList<string> PerDirectoryPatterns
);

/// <summary>
/// Handles <see cref="GetExclusionRulesQuery"/> by assembling all rule categories from
/// <see cref="IExclusionService"/> into an <see cref="ExclusionRulesResult"/>.
/// </summary>
public sealed class GetExclusionRulesHandler(IExclusionService service)
{
    /// <summary>
    /// Returns all exclusion rules grouped by type for the optionally specified directory.
    /// </summary>
    public ExclusionRulesResult Handle(GetExclusionRulesQuery query) =>
        new(
            service.GetSystemRules(),
            service.GetGlobalUserRules(),
            query.DirectoryPath is not null
                ? service.GetPerDirectoryPatterns(query.DirectoryPath)
                : []
        );
}
