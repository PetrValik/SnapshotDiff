using SnapshotDiff.Features.Trash.Domain;
using SnapshotDiff.Features.Trash.Infrastructure;

namespace SnapshotDiff.Features.Trash.Application.Queries;

/// <summary>
/// Handles a <see cref="GetTrashItemsQuery"/> by returning all current trash item metadata.
/// </summary>
public sealed class GetTrashItemsHandler(ITrashService trashService)
{
    /// <summary>
    /// Returns all items currently in the trash, ordered by deletion time descending.
    /// </summary>
    public async Task<IReadOnlyList<TrashItemMeta>> HandleAsync(
        GetTrashItemsQuery query,
        CancellationToken ct = default)
    {
        return await trashService.GetItemsAsync(ct);
    }
}
