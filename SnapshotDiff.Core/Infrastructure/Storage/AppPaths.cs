namespace SnapshotDiff.Infrastructure.Storage;

/// <summary>
/// Static helper for resolving default platform-specific paths.
/// Used as a fallback when no IStoragePathProvider is registered (tests, Linux host).
/// </summary>
public static class AppPaths
{
    /// <summary>
    /// Returns the default application data directory for the current OS.
    /// </summary>
    public static string GetDefaultAppDataDirectory()
    {
        if (OperatingSystem.IsWindows())
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SnapshotDiff");

        if (OperatingSystem.IsMacOS() || OperatingSystem.IsMacCatalyst())
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Library", "Application Support", "SnapshotDiff");

        // Linux and others: follow XDG Base Directory spec
        var xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        if (!string.IsNullOrWhiteSpace(xdgDataHome))
            return Path.Combine(xdgDataHome, "SnapshotDiff");

        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".local", "share", "SnapshotDiff");
    }

    public static string GetDefaultConfigPath() =>
        Path.Combine(GetDefaultAppDataDirectory(), "config.json");

    public static string GetDefaultDataPath() =>
        Path.Combine(GetDefaultAppDataDirectory(), "data");
}
