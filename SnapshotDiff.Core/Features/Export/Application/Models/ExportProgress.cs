namespace SnapshotDiff.Features.Export.Application.Models;

/// <summary>
/// Progress information for export operations
/// </summary>
public sealed record ExportProgress
{
    public required long ProcessedItems { get; init; }
    public required long? TotalItems { get; init; }
    public required long ProcessedBytes { get; init; }
    public required string? CurrentItem { get; init; }

    /// <summary>
    /// Percentage of completion (0-100)
    /// </summary>
    public int PercentComplete =>
        TotalItems.HasValue && TotalItems.Value > 0
            ? (int)((ProcessedItems * 100) / TotalItems.Value)
            : 0;

    public static ExportProgress Create(
        long processedItems,
        long? totalItems,
        long processedBytes,
        string? currentItem = null) =>
        new()
        {
            ProcessedItems = processedItems,
            TotalItems = totalItems,
            ProcessedBytes = processedBytes,
            CurrentItem = currentItem
        };
}
