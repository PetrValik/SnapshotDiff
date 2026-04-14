using SnapshotDiff.Features.Trash.Infrastructure;

namespace SnapshotDiff.Features.Trash.Application.Commands;

/// <summary>
/// Handles a <see cref="MoveToTrashCommand"/> by delegating to <see cref="ITrashService"/>.
/// </summary>
public sealed class MoveToTrashHandler(ITrashService trashService)
{
    /// <summary>
    /// Moves the file at <see cref="MoveToTrashCommand.FullPath"/> to the trash.
    /// </summary>
    /// <returns>The new trash item ID (GUID string) assigned to the trashed file.</returns>
    public Task<string> HandleAsync(MoveToTrashCommand command, CancellationToken ct = default) =>
        trashService.MoveToTrashAsync(command.FullPath, ct);
}
