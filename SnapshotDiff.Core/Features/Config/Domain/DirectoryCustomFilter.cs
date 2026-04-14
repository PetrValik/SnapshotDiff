namespace SnapshotDiff.Features.Config.Domain;

/// <summary>
/// Per-directory filter overrides. A null property means "use the global default".
/// </summary>
public sealed class DirectoryCustomFilter
{
    /// <summary>
    /// Files older than this many days are considered stale. Null = use global default.
    /// </summary>
    public int? StaleAfterDays { get; set; }

    /// <summary>
    /// Files newer than this many days are considered new. Null = use global default.
    /// </summary>
    public int? NewWithinDays { get; set; }

    /// <summary>
    /// Minimum file size in bytes to include in results.
    /// </summary>
    public long? MinSizeBytes { get; set; }

    /// <summary>
    /// If non-empty, only files with these extensions are included (e.g. ".zip", ".log").
    /// </summary>
    public List<string> OnlyExtensions { get; set; } = [];

    /// <summary>
    /// Extensions to exclude (e.g. ".tmp").
    /// </summary>
    public List<string> ExcludeExtensions { get; set; } = [];
}
