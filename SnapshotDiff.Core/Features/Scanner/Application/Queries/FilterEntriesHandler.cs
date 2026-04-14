using SnapshotDiff.Features.Scanner.Domain;
using SnapshotDiff.Features.Scanner.Infrastructure;

namespace SnapshotDiff.Features.Scanner.Application.Queries;

/// <summary>
/// Filters and sorts scan results stored in <see cref="IScanStateService"/> according to
/// the criteria in a <see cref="FilterEntriesQuery"/>. Only file entries are returned
/// (directories are excluded from filtering).
/// </summary>
public sealed class FilterEntriesHandler(IScanStateService state)
{
    /// <summary>
    /// Applies all active filters from <paramref name="query"/> and returns a sorted list of matching entries.
    /// Returns an empty list when no scan result exists for the requested directory.
    /// </summary>
    public List<ScanEntry> Handle(FilterEntriesQuery query)
    {
        var result = state.Get(query.DirectoryPath);
        if (result is null)
            return [];

        var now = DateTimeOffset.UtcNow;
        var entries = result.Entries.AsEnumerable();

        // Only files for age/size/extension filtering
        entries = entries.Where(e => e.Type == ScanEntryType.File);

        // Directory filter — show files within selected directory (and subdirs)
        if (!string.IsNullOrEmpty(query.SubDirectoryPath))
        {
            var prefix = query.SubDirectoryPath;
            entries = entries.Where(e =>
            {
                var entryDir = Path.GetDirectoryName(e.RelativePath) ?? string.Empty;
                return entryDir.Equals(prefix, StringComparison.OrdinalIgnoreCase)
                    || entryDir.StartsWith(prefix + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
            });
        }

        // Age filter
        entries = query.AgeFilter switch
        {
            FileAgeFilter.Stale when query.StaleAfterDays.HasValue =>
                entries.Where(e => (now - e.LastWriteTime).TotalDays > query.StaleAfterDays.Value),
            FileAgeFilter.New when query.NewWithinDays.HasValue =>
                entries.Where(e => (now - e.LastWriteTime).TotalDays <= query.NewWithinDays.Value),
            _ => entries
        };

        // Min size
        if (query.MinSizeBytes.HasValue)
            entries = entries.Where(e => e.Size >= query.MinSizeBytes.Value);

        // Extension filter (comma-separated, e.g. ".zip, .log")
        if (!string.IsNullOrWhiteSpace(query.ExtensionFilter))
        {
            var exts = query.ExtensionFilter
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(e => e.StartsWith('.') ? e.ToLowerInvariant() : "." + e.ToLowerInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            entries = entries.Where(e => exts.Contains(e.Extension));
        }

        // Name search
        if (!string.IsNullOrWhiteSpace(query.NameSearch))
            entries = entries.Where(e => e.Name.Contains(query.NameSearch, StringComparison.OrdinalIgnoreCase));

        // Sort
        entries = (query.SortBy, query.SortAscending) switch
        {
            (SortField.Name, true)            => entries.OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase),
            (SortField.Name, false)           => entries.OrderByDescending(e => e.Name, StringComparer.OrdinalIgnoreCase),
            (SortField.Size, true)            => entries.OrderBy(e => e.Size),
            (SortField.Size, false)           => entries.OrderByDescending(e => e.Size),
            (SortField.LastWriteTime, true)   => entries.OrderBy(e => e.LastWriteTime),
            (SortField.LastWriteTime, false)  => entries.OrderByDescending(e => e.LastWriteTime),
            (SortField.Extension, true)       => entries.OrderBy(e => e.Extension, StringComparer.OrdinalIgnoreCase),
            (SortField.Extension, false)      => entries.OrderByDescending(e => e.Extension, StringComparer.OrdinalIgnoreCase),
            (SortField.LastAccessTime, true)  => entries.OrderBy(e => e.LastAccessTime),
            (SortField.LastAccessTime, false) => entries.OrderByDescending(e => e.LastAccessTime),
            _                                 => entries.OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
        };

        return [.. entries];
    }
}
