namespace SnapshotDiff.Features.Config.Domain;

/// <summary>
/// A directory that the file cleaner can scan.
/// </summary>
public sealed class WatchedDirectory
{
    /// <summary>
    /// Full absolute path to the directory.
    /// </summary>
    public required string Path { get; set; }

    /// <summary>
    /// User-defined label. Defaults to the folder name if empty.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Whether this directory is enabled for scanning.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// UTC timestamp when this directory was added.
    /// </summary>
    public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UTC timestamp of the last completed scan, or null if never scanned.
    /// </summary>
    public DateTime? LastScannedAt { get; set; }

    /// <summary>
    /// Per-directory filter overrides. Null = use global defaults.
    /// </summary>
    public DirectoryCustomFilter? CustomFilter { get; set; }  // reserved for future per-directory filters

    /// <summary>
    /// Extra glob patterns excluded only for this directory (on top of system + global rules).
    /// Examples: "*.log", "cache", "build".
    /// </summary>
    public List<string> ExclusionPatterns { get; set; } = [];

    /// <summary>
    /// The effective display name: Label if set, otherwise the last folder segment.
    /// </summary>
    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Label)) return Label;
            var trimmed = Path.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
            var name = System.IO.Path.GetFileName(trimmed);
            return string.IsNullOrEmpty(name) ? Path : name;
        }
    }
}
