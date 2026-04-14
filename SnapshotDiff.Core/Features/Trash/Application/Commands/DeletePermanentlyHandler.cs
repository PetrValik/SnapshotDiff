using SnapshotDiff.Features.Trash.Infrastructure;

namespace SnapshotDiff.Features.Trash.Application.Commands;

/// <summary>
/// Handles a <see cref="DeletePermanentlyCommand"/> by permanently removing a single trash item.
/// </summary>
public sealed class DeletePermanentlyHandler(ITrashService trashService)
{
    /// <summary>
    /// Permanently deletes the trash item identified by <see cref="DeletePermanentlyCommand.Id"/>.
    /// </summary>
    public async Task HandleAsync(DeletePermanentlyCommand command, CancellationToken ct = default)
    {
        await trashService.DeletePermanentlyAsync(command.Id, ct);
    }
}
