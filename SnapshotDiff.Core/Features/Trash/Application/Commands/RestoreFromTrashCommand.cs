namespace SnapshotDiff.Features.Trash.Application.Commands;

/// <summary>
/// Restores a previously trashed item back to its original location.
/// </summary>
/// <param name="Id">Trash item ID returned by <see cref="MoveToTrashHandler"/>.</param>
public record RestoreFromTrashCommand(string Id);
