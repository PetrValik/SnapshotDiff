namespace SnapshotDiff.Infrastructure.Storage;

/// <summary>
/// Provides platform-specific storage paths for the application.
/// Implemented separately per host (MAUI, Linux/Photino, tests).
/// </summary>
public interface IStoragePathProvider
{
    /// <summary>
    /// Root application data directory. Platform-specific:
    /// - Windows: %AppData%\SnapshotDiff
    /// - Linux: $XDG_DATA_HOME/SnapshotDiff or ~/.local/share/SnapshotDiff
    /// - macOS: ~/Library/Application Support/SnapshotDiff
    /// - Android/iOS: FileSystem.AppDataDirectory/SnapshotDiff
    /// </summary>
    string AppDataDirectory { get; }
}
