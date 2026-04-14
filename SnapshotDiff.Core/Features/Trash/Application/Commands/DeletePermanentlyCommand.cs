namespace SnapshotDiff.Features.Trash.Application.Commands;

/// <summary>
/// Permanently deletes a single trash item identified by its ID,
/// removing both the physical file and its metadata record.
/// </summary>
/// <param name="Id">Trash item ID to delete permanently.</param>
public record DeletePermanentlyCommand(string Id);
