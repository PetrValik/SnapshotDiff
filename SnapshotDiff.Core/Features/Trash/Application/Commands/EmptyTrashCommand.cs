namespace SnapshotDiff.Features.Trash.Application.Commands;

/// <summary>
/// Permanently deletes all items currently in the trash, bypassing the 30-day retention window.
/// </summary>
public record EmptyTrashCommand();
