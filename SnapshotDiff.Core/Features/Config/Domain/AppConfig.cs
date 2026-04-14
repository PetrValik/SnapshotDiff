using SnapshotDiff.Infrastructure.Storage;

namespace SnapshotDiff.Features.Config.Domain;

/// <summary>
/// Root configuration model for SnapshotDiff.
/// Stored as JSON in the platform-specific app data directory.
/// The concrete path is provided at startup by IStoragePathProvider.
/// </summary>
public sealed class AppConfig
{
    public List<WatchedDirectory> WatchedDirectories { get; set; } = [];

    /// <summary>
    /// User-defined exclusion patterns applied to every scan (global scope).
    /// </summary>
    public List<UserExclusionPattern> GlobalExclusionPatterns { get; set; } = [];

    /// <summary>
    /// Default number of days after which a file is considered stale.
    /// </summary>
    public int DefaultStaleAfterDays { get; set; } = 365;

    /// <summary>
    /// Default number of days within which a file is considered new.
    /// </summary>
    public int DefaultNewWithinDays { get; set; } = 30;

    public AppearanceConfig Appearance { get; set; } = new();

    /// <summary>
    /// Data storage path. Set at startup via IStoragePathProvider.
    /// Falls back to AppPaths.GetDefaultDataPath() when not explicitly set.
    /// </summary>
    public string DataPath { get; set; } = string.Empty;

    /// <summary>
    /// UI language override. Empty = use OS default.
    /// Supported: "cs", "en"
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Resolves DataPath: returns the stored value if set, otherwise returns the platform default.
    /// </summary>
    public string GetResolvedDataPath() =>
        string.IsNullOrWhiteSpace(DataPath)
            ? AppPaths.GetDefaultDataPath()
            : DataPath;
}
