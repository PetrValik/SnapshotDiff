namespace SnapshotDiff.Features.Scanner.Domain;

/// <summary>
/// The current phase of a running scan, reported through <see cref="ScanProgress"/>.
/// </summary>
public enum ScanPhase
{
    /// <summary>
    /// Phase 1 – quick directory count to establish total for a deterministic progress bar.
    /// </summary>
    Counting,
    /// <summary>
    /// Phase 2 – parallel traversal that collects file and directory metadata.
    /// </summary>
    Scanning
}

/// <summary>
/// Snapshot of scan progress reported by <see cref="Infrastructure.IScannerService"/> at regular intervals.
/// </summary>
public sealed record ScanProgress
{
    /// <summary>
    /// Number of files collected so far (only meaningful in <see cref="ScanPhase.Scanning"/>).
    /// </summary>
    public required int ProcessedFiles { get; init; }

    /// <summary>
    /// Absolute path of the directory currently being processed.
    /// </summary>
    public required string CurrentDirectory { get; init; }

    /// <summary>
    /// Current phase of the scan.
    /// </summary>
    public required ScanPhase Phase { get; init; }

    /// <summary>
    /// Number of directories visited so far.
    /// </summary>
    public int ProcessedDirectories { get; init; }

    /// <summary>
    /// Total number of directories discovered during <see cref="ScanPhase.Counting"/>.
    /// Zero while the counting phase is still running.
    /// </summary>
    public int TotalDirectories { get; init; }

    /// <summary>
    /// Wall-clock seconds elapsed since the scan started.
    /// </summary>
    public double ElapsedSeconds { get; init; }
}
