using SnapshotDiff.Features.Scanner.Domain;

namespace SnapshotDiff.Features.Scanner.Infrastructure;

/// <summary>
/// Executes a directory scan and returns a <see cref="ScanResult"/>.
/// Progress is reported through an optional <see cref="IProgress{T}"/> sink.
/// </summary>
public interface IScannerService
{
    /// <summary>
    /// Scans the directory specified in <paramref name="options"/> and returns an immutable result.
    /// </summary>
    /// <param name="options">Scan configuration including the root path and optional exclusion evaluator.</param>
    /// <param name="progress">Optional sink for real-time progress updates.</param>
    /// <param name="ct">Token to cancel a long-running scan.</param>
    Task<ScanResult> ScanAsync(
        ScanOptions options,
        IProgress<ScanProgress>? progress = null,
        CancellationToken ct = default);
}
