namespace SnapshotDiff.Infrastructure.Storage;

/// <summary>
/// Fallback IStoragePathProvider that uses OS-level environment detection.
/// Used by the Linux (Photino) host and in tests when no platform provider is registered.
/// </summary>
public sealed class FallbackStoragePathProvider : IStoragePathProvider
{
    public string AppDataDirectory => AppPaths.GetDefaultAppDataDirectory();
}
