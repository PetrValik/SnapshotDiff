using SnapshotDiff.Features.Trash.Domain;

namespace SnapshotDiff.Features.Trash.Infrastructure;

public interface ITrashService
{
    /// <summary>
    /// Moves a file or directory to the trash. Returns the item id.
    /// </summary>
    Task<string> MoveToTrashAsync(string fullPath, CancellationToken ct = default);

    /// <summary>
    /// Restores an item from the trash to its original location.
    /// </summary>
    Task RestoreAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Permanently deletes an item from the trash (files + metadata).
    /// </summary>
    Task DeletePermanentlyAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Permanently deletes all items in the trash.
    /// </summary>
    Task EmptyTrashAsync(CancellationToken ct = default);

    /// <summary>
    /// Deletes all items whose <see cref="TrashItemMeta.ExpiresAt"/> has passed.
    /// </summary>
    Task PurgeExpiredAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns all trash items sorted by DeletedAt descending.
    /// </summary>
    Task<IReadOnlyList<TrashItemMeta>> GetItemsAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns a single trash item by id, or null if not found.
    /// </summary>
    Task<TrashItemMeta?> GetItemAsync(string id, CancellationToken ct = default);
}
