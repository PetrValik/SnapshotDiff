using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SnapshotDiff.Infrastructure.Persistence;

/// <summary>
/// Stores <typeparamref name="TState"/> as a JSON file on disk.
/// Thread-safe implementation with atomic file replacement.
/// Includes an in-memory read cache: after the first load (or any save),
/// subsequent <see cref="LoadAsync"/> calls return the cached value without disk I/O.
/// Cache is invalidated on every <see cref="SaveAsync"/> to guarantee consistency.
/// </summary>
/// <typeparam name="TState">The state type. Must be a class with a public parameterless constructor.</typeparam>
public sealed class JsonStateStorage<TState>(
    string filePath,
    JsonSerializerOptions? jsonOptions = null,
    ILogger? logger = null) : IStateStorage<TState>, IDisposable
    where TState : class, new()
{
    private readonly string _filePath = string.IsNullOrWhiteSpace(filePath)
        ? throw new ArgumentException("File path must not be empty.", nameof(filePath))
        : filePath;
    private readonly JsonSerializerOptions _jsonOptions = jsonOptions ?? new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly ILogger _logger = logger ?? NullLogger.Instance;
    private TState? _cache;
    private bool _disposed;

    /// <summary>
    /// Loads the persisted state. Subsequent calls return the in-memory cached value without disk I/O.
    /// Thread-safe.
    /// </summary>
    public async Task<TState> LoadAsync(CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Fast path: return cached value without acquiring the lock
        var cached = Volatile.Read(ref _cache);
        if (cached is not null)
            return cached;

        // Slow path: load from disk under lock
        // 5s timeout guards against deadlock — not a performance constraint.
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        if (!ct.IsCancellationRequested)
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            await _writeLock.WaitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException)
        {
            ct.ThrowIfCancellationRequested();
            throw;
        }

        try
        {
            // Double-check after acquiring lock
            cached = Volatile.Read(ref _cache);
            if (cached is not null)
                return cached;

            var loaded = await LoadFromDiskAsync(ct);
            Volatile.Write(ref _cache, loaded);
            return loaded;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <summary>
    /// Persists <paramref name="state"/> to disk atomically and updates the in-memory cache.
    /// Thread-safe.
    /// </summary>
    public async Task SaveAsync(TState state, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(state);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        if (!ct.IsCancellationRequested)
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            await _writeLock.WaitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException)
        {
            ct.ThrowIfCancellationRequested();
            throw;
        }

        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            var bytes = JsonSerializer.SerializeToUtf8Bytes(state, _jsonOptions);

            // Atomic write: write to temp file then move
            var tmpPath = _filePath + ".tmp";
            await File.WriteAllBytesAsync(tmpPath, bytes, ct);

            try
            {
                // File.Move with overwrite is atomic on NTFS/ext4
                File.Move(tmpPath, _filePath, overwrite: true);
            }
            catch
            {
                try { File.Delete(tmpPath); } catch { /* ignore */ }
                throw;
            }

            // Update cache after successful write
            Volatile.Write(ref _cache, state);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    private async Task<TState> LoadFromDiskAsync(CancellationToken ct)
    {
        if (!File.Exists(_filePath))
            return new TState();

        try
        {
            var bytes = await File.ReadAllBytesAsync(_filePath, ct);
            return JsonSerializer.Deserialize<TState>(bytes, _jsonOptions) ?? new TState();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Corrupted JSON in {File}, returning empty state", _filePath);
            return new TState();
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Cannot read {File}, returning empty state", _filePath);
            return new TState();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _disposed = true;
        _writeLock.Dispose();
    }
}
