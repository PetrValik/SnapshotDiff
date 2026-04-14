using SnapshotDiff.Infrastructure.Storage;

namespace SnapshotDiff.Linux.Services;

/// <summary>
/// IStoragePathProvider for Linux, respecting the XDG Base Directory specification.
/// Uses $XDG_DATA_HOME/SnapshotDiff or falls back to ~/.local/share/SnapshotDiff.
/// </summary>
public sealed class LinuxStoragePathProvider : IStoragePathProvider
{
    public string AppDataDirectory
    {
        get
        {
            var xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
            var dataHome = !string.IsNullOrWhiteSpace(xdgDataHome)
                ? xdgDataHome
                : Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".local", "share");

            return Path.Combine(dataHome, "SnapshotDiff");
        }
    }
}
