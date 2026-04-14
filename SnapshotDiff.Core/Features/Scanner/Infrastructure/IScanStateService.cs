using SnapshotDiff.Features.Scanner.Domain;

namespace SnapshotDiff.Features.Scanner.Infrastructure;

/// <summary>
/// Holds the most-recent scan result per directory path, in memory only.
/// </summary>
public interface IScanStateService
{
    /// <summary>
    /// Stores or replaces the scan result for its root path.
    /// </summary>
    void Store(ScanResult result);

    /// <summary>
    /// Returns the most recent scan result for <paramref name="rootPath"/>, or <see langword="null"/> if none exists.
    /// </summary>
    ScanResult? Get(string rootPath);

    /// <summary>
    /// Removes the cached result for <paramref name="rootPath"/>.
    /// </summary>
    void Clear(string rootPath);

    /// <summary>
    /// Removes all cached results.
    /// </summary>
    void ClearAll();
}
