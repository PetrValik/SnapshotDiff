using SnapshotDiff.Features.Export.Application.Models;
using SnapshotDiff.Features.Scanner.Domain;
using SnapshotDiff.Infrastructure.Common;

namespace SnapshotDiff.Features.Export.Application;

/// <summary>
/// Exports a list of scan entries to a file in the requested format.
/// </summary>
public interface IExportService
{
    Task<Result<ExportResult>> ExportAsync(
        IReadOnlyList<ScanEntry> entries,
        ExportFormat format,
        string? suggestedFileName = null,
        CancellationToken ct = default);
}
