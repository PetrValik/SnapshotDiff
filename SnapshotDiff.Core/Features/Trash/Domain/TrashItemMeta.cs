namespace SnapshotDiff.Features.Trash.Domain;

/// <summary>
/// Metadata for a single item in the trash. Stored as a row in the SQLite <c>TrashItems</c> table.
/// The physical content is stored at <c>{AppData}/trash/files/{Id}</c>.
/// </summary>
public record TrashItemMeta
{
    /// <summary>
    /// Unique identifier used as the storage file name inside the trash files directory.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Absolute path where the file or directory was located before it was trashed.
    /// </summary>
    public string OriginalPath { get; init; } = "";

    /// <summary>
    /// File or directory name (last path segment).
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// UTC timestamp when the item was moved to the trash.
    /// </summary>
    public DateTime DeletedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp after which the item is eligible for automatic purge (default: 30 days after deletion).
    /// </summary>
    public DateTime ExpiresAt { get; init; }

    /// <summary>
    /// <see langword="true"/> if the trashed item is a directory; <see langword="false"/> for a file.
    /// </summary>
    public bool IsDirectory { get; init; }

    /// <summary>
    /// Size in bytes at the time the item was trashed. Directories report the recursive total.
    /// </summary>
    public long SizeBytes { get; init; }
}
