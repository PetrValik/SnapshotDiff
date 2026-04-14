namespace SnapshotDiff.Features.Scanner.Application.Queries;

/// <summary>
/// Determines how to filter scan entries by file age.
/// </summary>
public enum FileAgeFilter
{
    /// <summary>
    /// No age filter applied; all files are included.
    /// </summary>
    All,
    /// <summary>
    /// Only files whose last-write time is older than <see cref="FilterEntriesQuery.StaleAfterDays"/>.
    /// </summary>
    Stale,
    /// <summary>
    /// Only files whose last-write time is within the last <see cref="FilterEntriesQuery.NewWithinDays"/> days.
    /// </summary>
    New
}

/// <summary>
/// Column by which filtered scan results can be sorted.
/// </summary>
public enum SortField { Name, Size, LastWriteTime, Extension, LastAccessTime }

/// <summary>
/// Parameters for filtering and sorting a previously stored <see cref="Domain.ScanResult"/>.
/// All filter fields are optional; omitting them returns all entries for the given directory.
/// </summary>
/// <param name="DirectoryPath">Root path of the scan whose results should be filtered.</param>
/// <param name="AgeFilter">Age-based filter mode.</param>
/// <param name="StaleAfterDays">Used when <paramref name="AgeFilter"/> is <see cref="FileAgeFilter.Stale"/>.</param>
/// <param name="NewWithinDays">Used when <paramref name="AgeFilter"/> is <see cref="FileAgeFilter.New"/>.</param>
/// <param name="MinSizeBytes">Minimum file size in bytes; smaller files are excluded.</param>
/// <param name="ExtensionFilter">Comma-separated list of extensions to include (e.g. <c>.zip, .log</c>).</param>
/// <param name="NameSearch">Case-insensitive substring match against the file name.</param>
/// <param name="SortBy">Column to sort by.</param>
/// <param name="SortAscending"><see langword="true"/> for ascending, <see langword="false"/> for descending.</param>
/// <param name="SubDirectoryPath">When set, limits results to this subdirectory and its descendants (relative path).</param>
public sealed record FilterEntriesQuery(
    string DirectoryPath,
    FileAgeFilter AgeFilter,
    int? StaleAfterDays,
    int? NewWithinDays,
    long? MinSizeBytes,
    string? ExtensionFilter,
    string? NameSearch,
    SortField SortBy,
    bool SortAscending,
    string? SubDirectoryPath = null);
