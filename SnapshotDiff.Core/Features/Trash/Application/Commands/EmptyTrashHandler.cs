using SnapshotDiff.Features.Trash.Infrastructure;

namespace SnapshotDiff.Features.Trash.Application.Commands;

/// <summary>
/// Handles an <see cref="EmptyTrashCommand"/> by permanently deleting all items from the trash.
/// </summary>
public sealed class EmptyTrashHandler(ITrashService trashService)
{
    /// <summary>
    /// Permanently deletes all items from the trash.
    /// </summary>
    public async Task HandleAsync(EmptyTrashCommand command, CancellationToken ct = default)
    {
        await trashService.EmptyTrashAsync(ct);
    }
}
