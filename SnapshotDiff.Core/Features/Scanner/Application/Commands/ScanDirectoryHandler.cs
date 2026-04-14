using SnapshotDiff.Features.ExclusionRules.Infrastructure;
using SnapshotDiff.Features.Scanner.Domain;
using SnapshotDiff.Features.Scanner.Infrastructure;

namespace SnapshotDiff.Features.Scanner.Application.Commands;

/// <summary>
/// Handles a <see cref="ScanDirectoryCommand"/> by running a full directory scan,
/// applying exclusion rules, and storing the result in <see cref="IScanStateService"/>.
/// </summary>
public sealed class ScanDirectoryHandler(
    IScannerService scanner,
    IScanStateService state,
    IExclusionService exclusionService)
{
    /// <summary>
    /// Executes the scan: builds an exclusion evaluator snapshot, invokes the scanner,
    /// and persists the result in the in-memory scan state.
    /// </summary>
    /// <param name="command">Contains the target directory path and optional per-directory filter.</param>
    /// <param name="progress">Optional progress sink for real-time UI updates.</param>
    /// <param name="ct">Cancellation token to abort a long-running scan.</param>
    /// <returns>The completed <see cref="ScanResult"/> including all discovered entries.</returns>
    public async Task<ScanResult> HandleAsync(
        ScanDirectoryCommand command,
        IProgress<ScanProgress>? progress = null,
        CancellationToken ct = default)
    {
        var evaluator = exclusionService.GetEvaluatorForScan(command.DirectoryPath);

        var options = new ScanOptions
        {
            RootPath = command.DirectoryPath,
            ExclusionEvaluator = evaluator
        };

        var result = await scanner.ScanAsync(options, progress, ct);
        state.Store(result);
        return result;
    }
}
