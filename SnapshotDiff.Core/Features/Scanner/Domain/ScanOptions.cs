using SnapshotDiff.Features.ExclusionRules.Infrastructure;

namespace SnapshotDiff.Features.Scanner.Domain;

/// <summary>
/// Input parameters for a single scan run passed to <see cref="Infrastructure.IScannerService.ScanAsync"/>.
/// </summary>
public sealed record ScanOptions
{
    /// <summary>
    /// Absolute path to the root directory to scan.
    /// </summary>
    public required string RootPath { get; init; }

    /// <summary>
    /// Optional evaluator snapshot built from the current exclusion rules.
    /// When <see langword="null"/>, no files or directories are excluded.
    /// </summary>
    public IExclusionEvaluator? ExclusionEvaluator { get; init; }
}
