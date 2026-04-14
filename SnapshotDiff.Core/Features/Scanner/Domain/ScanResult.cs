namespace SnapshotDiff.Features.Scanner.Domain;

/// <summary>
/// Immutable result of a completed directory scan, produced by <see cref="Infrastructure.IScannerService"/>.
/// </summary>
public sealed record ScanResult
{
    /// <summary>
    /// Absolute path of the directory that was scanned.
    /// </summary>
    public required string RootPath { get; init; }

    /// <summary>
    /// UTC timestamp at which the scan completed.
    /// </summary>
    public required DateTime ScannedAt { get; init; }

    /// <summary>
    /// All file and directory entries discovered during the scan.
    /// </summary>
    public required List<ScanEntry> Entries { get; init; }

    /// <summary>
    /// Wall-clock time the scan took from start to finish.
    /// </summary>
    public required TimeSpan Duration { get; init; }

    /// <summary>
    /// Full paths that could not be read due to permission errors or I/O issues.
    /// The scan continues despite these; callers may surface them as warnings.
    /// </summary>
    public List<string> InaccessiblePaths { get; init; } = [];
}
