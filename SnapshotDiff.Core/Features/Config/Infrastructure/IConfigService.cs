using SnapshotDiff.Features.Config.Domain;

namespace SnapshotDiff.Features.Config.Infrastructure;

/// <summary>
/// Manages loading and saving of AppConfig.
/// </summary>
public interface IConfigService
{
    /// <summary>
    /// Returns the currently loaded config (cached in memory).
    /// </summary>
    AppConfig Current { get; }

    /// <summary>
    /// Loads config from disk. Call once at startup.
    /// </summary>
    Task<AppConfig> LoadAsync(CancellationToken ct = default);

    /// <summary>
    /// Persists the current config to disk.
    /// </summary>
    Task SaveAsync(CancellationToken ct = default);

    /// <summary>
    /// Adds a directory to the watch list and saves.
    /// </summary>
    Task AddWatchedDirectoryAsync(string path, string label = "", CancellationToken ct = default);

    /// <summary>
    /// Removes a watched directory by path and saves.
    /// </summary>
    Task RemoveWatchedDirectoryAsync(string path, CancellationToken ct = default);

    /// <summary>
    /// Updates a watched directory's settings and saves.
    /// </summary>
    Task UpdateWatchedDirectoryAsync(string path, Action<WatchedDirectory> update, CancellationToken ct = default);

    /// <summary>
    /// Raised whenever the config changes (add/remove/update of watched dirs, settings changes).
    /// </summary>
    event EventHandler? ConfigChanged;

    /// <summary>
    /// Deletes the config file and resets to defaults in memory.
    /// </summary>
    Task ResetAsync(CancellationToken ct = default);
}
