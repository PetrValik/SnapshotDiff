using SnapshotDiff.Features.Trash.Infrastructure;

namespace SnapshotDiff.Features.Trash.Application.Commands;

/// <summary>
/// Handles a <see cref="RestoreFromTrashCommand"/> by delegating the restore operation
/// to <see cref="ITrashService"/>.
/// </summary>
public sealed class RestoreFromTrashHandler(ITrashService trashService)
{
    /// <summary>
    /// Restores the trashed item identified by <see cref="RestoreFromTrashCommand.Id"/>
    /// to its original file-system location.
    /// </summary>
    public async Task HandleAsync(RestoreFromTrashCommand command, CancellationToken ct = default)
    {
        await trashService.RestoreAsync(command.Id, ct);
    }
}
