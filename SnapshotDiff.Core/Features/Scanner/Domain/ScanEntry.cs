namespace SnapshotDiff.Features.Scanner.Domain;

/// <summary>
/// Distinguishes between file and directory entries returned by the scanner.
/// </summary>
public enum ScanEntryType { File, Directory }

/// <summary>
/// Represents a single file or directory found during a scan, with its metadata.
/// </summary>
public sealed record ScanEntry
{
    /// <summary>
    /// Absolute path to the file or directory.
    /// </summary>
    public required string FullPath { get; init; }

    /// <summary>
    /// Path relative to the scanned root directory.
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// File or directory name without the path.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// File size in bytes. Always 0 for directories.
    /// </summary>
    public required long Size { get; init; }

    /// <summary>
    /// UTC timestamp of the last write (modification) to the file.
    /// </summary>
    public required DateTimeOffset LastWriteTime { get; init; }

    /// <summary>
    /// UTC timestamp of the last access to the file.
    /// </summary>
    public DateTimeOffset LastAccessTime { get; init; }

    /// <summary>
    /// Whether this entry represents a file or a directory.
    /// </summary>
    public required ScanEntryType Type { get; init; }

    /// <summary>
    /// Lowercase file extension including the dot (e.g. <c>.zip</c>). Empty for directories.
    /// </summary>
    public required string Extension { get; init; }

    /// <summary>
    /// Direct children – populated only when building a tree view, otherwise empty.
    /// </summary>
    public List<ScanEntry> Children { get; init; } = [];
}
