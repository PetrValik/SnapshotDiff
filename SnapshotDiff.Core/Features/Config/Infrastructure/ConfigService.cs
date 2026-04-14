using System.Text.Json;
using Microsoft.Extensions.Logging;
using SnapshotDiff.Features.Config.Domain;
using SnapshotDiff.Infrastructure.Storage;

namespace SnapshotDiff.Features.Config.Infrastructure;

/// <summary>
/// JSON-based config service. Stores config in the platform-specific app data directory.
/// Thread-safe: uses SemaphoreSlim for async-safe file access.
/// </summary>
public sealed class ConfigService : IConfigService, IDisposable
{
    private readonly string _configPath;
    private readonly IStoragePathProvider? _storagePathProvider;
    private readonly ILogger<ConfigService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private AppConfig _current = new();
    private bool _disposed;

    public AppConfig Current => _current;
    public event EventHandler? ConfigChanged;

    public ConfigService(ILogger<ConfigService> logger, IStoragePathProvider? storagePathProvider = null)
    {
        _logger = logger;
        _storagePathProvider = storagePathProvider;
        _configPath = storagePathProvider is not null
            ? Path.Combine(storagePathProvider.AppDataDirectory, "config.json")
            : AppPaths.GetDefaultConfigPath();
    }

    public async Task<AppConfig> LoadAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (!File.Exists(_configPath))
            {
                _current = new AppConfig();
                FixUpDataPath(_current);
                _logger.LogInformation("No config file found at {Path}, using defaults", _configPath);
                return _current;
            }

            var bytes = await File.ReadAllBytesAsync(_configPath, ct);
            _current = JsonSerializer.Deserialize<AppConfig>(bytes, _jsonOptions) ?? new AppConfig();
            FixUpDataPath(_current);
            _logger.LogInformation("Config loaded from {Path}", _configPath);
            return _current;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Config file corrupted at {Path}, using defaults", _configPath);
            _current = new AppConfig();
            FixUpDataPath(_current);
            return _current;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var dir = Path.GetDirectoryName(_configPath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var bytes = JsonSerializer.SerializeToUtf8Bytes(_current, _jsonOptions);
            var tmpPath = _configPath + ".tmp";
            await File.WriteAllBytesAsync(tmpPath, bytes, ct);
            File.Move(tmpPath, _configPath, overwrite: true);

            _logger.LogDebug("Config saved to {Path}", _configPath);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task AddWatchedDirectoryAsync(string path, string label = "", CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var normalized = Path.GetFullPath(path);

        if (_current.WatchedDirectories.Any(d => string.Equals(d.Path, normalized, StringComparison.OrdinalIgnoreCase)))
        {
            _logger.LogDebug("Directory already watched: {Path}", normalized);
            return;
        }

        _current.WatchedDirectories.Add(new WatchedDirectory
        {
            Path = normalized,
            Label = label,
            Enabled = true,
            AddedAtUtc = DateTime.UtcNow
        });

        await SaveAsync(ct);
        ConfigChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task RemoveWatchedDirectoryAsync(string path, CancellationToken ct = default)
    {
        var normalized = Path.GetFullPath(path);
        var removed = _current.WatchedDirectories.RemoveAll(
            d => string.Equals(d.Path, normalized, StringComparison.OrdinalIgnoreCase));

        if (removed > 0)
        {
            await SaveAsync(ct);
            ConfigChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task UpdateWatchedDirectoryAsync(string path, Action<WatchedDirectory> update, CancellationToken ct = default)
    {
        var normalized = Path.GetFullPath(path);
        var dir = _current.WatchedDirectories.FirstOrDefault(
            d => string.Equals(d.Path, normalized, StringComparison.OrdinalIgnoreCase));

        if (dir is null)
        {
            _logger.LogWarning("UpdateWatchedDirectory: directory not found: {Path}", normalized);
            return;
        }

        update(dir);
        await SaveAsync(ct);
        ConfigChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _lock.Dispose();
            _disposed = true;
        }
    }

    public async Task ResetAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (File.Exists(_configPath))
                File.Delete(_configPath);
            _current = new AppConfig();
            FixUpDataPath(_current);
            _logger.LogInformation("Config reset to defaults");
        }
        finally
        {
            _lock.Release();
        }
        ConfigChanged?.Invoke(this, EventArgs.Empty);
    }

    private void FixUpDataPath(AppConfig config)
    {
        if (string.IsNullOrEmpty(config.DataPath))
            config.DataPath = _storagePathProvider is not null
                ? Path.Combine(_storagePathProvider.AppDataDirectory, "data")
                : AppPaths.GetDefaultDataPath();
    }
}
