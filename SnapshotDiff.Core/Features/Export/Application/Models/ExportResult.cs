namespace SnapshotDiff.Features.Export.Application.Models;

public sealed record ExportResult
{
    public required string FilePath { get; init; }
    public required long FileSize { get; init; }
    public required DateTime ExportedAt { get; init; }
    public required ExportFormat Format { get; init; }
    public required int EntryCount { get; init; }

    public string FileName => Path.GetFileName(FilePath);
}
