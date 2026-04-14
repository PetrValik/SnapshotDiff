using SnapshotDiff.Features.Scanner.Domain;

namespace SnapshotDiff.Features.Scanner.Infrastructure;

/// <summary>
/// Thread-safe in-memory implementation of <see cref="IScanStateService"/>.
/// Keyed by the scan root path (case-insensitive). The entire collection is protected by a
/// single <see langword="lock"/> because individual scan results are replaced atomically.
/// </summary>
public sealed class InMemoryScanStateService : IScanStateService
{
    private readonly Dictionary<string, ScanResult> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    public void Store(ScanResult result)
    {
        lock (_lock)
            _cache[result.RootPath] = result;
    }

    public ScanResult? Get(string rootPath)
    {
        lock (_lock)
            return _cache.TryGetValue(rootPath, out var r) ? r : null;
    }

    public void Clear(string rootPath)
    {
        lock (_lock)
            _cache.Remove(rootPath);
    }

    public void ClearAll()
    {
        lock (_lock)
            _cache.Clear();
    }
}
