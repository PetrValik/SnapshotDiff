using SnapshotDiff.Features.Trash.Domain;

namespace SnapshotDiff.Features.Trash.Infrastructure;

public interface ITrashRepository
{
    Task InsertAsync(TrashItemMeta meta, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
    Task<TrashItemMeta?> GetAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<TrashItemMeta>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TrashItemMeta>> GetExpiredAsync(DateTime now, CancellationToken ct = default);
    Task DeleteAllAsync(CancellationToken ct = default);
}
