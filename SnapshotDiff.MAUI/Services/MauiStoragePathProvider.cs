using SnapshotDiff.Infrastructure.Storage;

namespace SnapshotDiff.MAUI.Services;

/// <summary>
/// IStoragePathProvider implementation using MAUI's FileSystem.AppDataDirectory.
/// Works on Windows, Android, iOS, and macOS without any platform-specific code.
/// </summary>
public sealed class MauiStoragePathProvider : IStoragePathProvider
{
    public string AppDataDirectory =>
        Path.Combine(Microsoft.Maui.Storage.FileSystem.AppDataDirectory, "SnapshotDiff");
}
