using FluentAssertions;
using SnapshotDiff.Features.Scanner.Application.Queries;
using SnapshotDiff.Features.Scanner.Domain;
using SnapshotDiff.Features.Scanner.Infrastructure;

namespace SnapshotDiff.Tests.Features.Scanner;

public class FilterEntriesHandlerTests
{
    private static ScanResult BuildResult(IEnumerable<ScanEntry> entries) => new()
    {
        RootPath = @"C:\test",
        ScannedAt = DateTime.UtcNow,
        Duration = TimeSpan.Zero,
        Entries = entries.ToList(),
    };

    private static ScanEntry File(string name, long size, DateTimeOffset lastWrite, string ext = "") => new()
    {
        FullPath = $@"C:\test\{name}",
        RelativePath = name,
        Name = name,
        Size = size,
        LastWriteTime = lastWrite,
        Type = ScanEntryType.File,
        Extension = string.IsNullOrEmpty(ext) ? System.IO.Path.GetExtension(name) : ext,
    };

    private FilterEntriesHandler BuildHandler(ScanResult result)
    {
        var state = new InMemoryScanStateService();
        state.Store(result);
        return new FilterEntriesHandler(state);
    }

    private static FilterEntriesQuery DefaultQuery(
        FileAgeFilter age = FileAgeFilter.All,
        int? stale = null,
        int? newWithin = null,
        long? minSize = null,
        string? ext = null,
        string? name = null) => new(
            DirectoryPath: @"C:\test",
            AgeFilter: age,
            StaleAfterDays: stale,
            NewWithinDays: newWithin,
            MinSizeBytes: minSize,
            ExtensionFilter: ext,
            NameSearch: name,
            SortBy: SortField.Name,
            SortAscending: true);

    // ── No filters returns all files ─────────────────────────────────────────

    [Fact]
    public void NoFilters_ReturnsAllFileEntries()
    {
        var now = DateTimeOffset.UtcNow;
        var entries = new[]
        {
            File("a.txt", 100, now.AddDays(-5)),
            File("b.log", 200, now.AddDays(-1)),
            File("c.zip", 300, now.AddDays(-60)),
        };
        var handler = BuildHandler(BuildResult(entries));

        var result = handler.Handle(DefaultQuery());

        result.Should().HaveCount(3);
    }

    [Fact]
    public void NoFilters_DirectoryEntriesAreExcluded()
    {
        var now = DateTimeOffset.UtcNow;
        var entries = new ScanEntry[]
        {
            File("a.txt", 100, now.AddDays(-5)),
            new()
            {
                FullPath = @"C:\test\subdir",
                RelativePath = "subdir",
                Name = "subdir",
                Size = 0,
                LastWriteTime = now,
                Type = ScanEntryType.Directory,
                Extension = "",
            },
        };
        var handler = BuildHandler(BuildResult(entries));

        var result = handler.Handle(DefaultQuery());

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("a.txt");
    }

    // ── Stale filter ─────────────────────────────────────────────────────────

    [Fact]
    public void StaleFilter_ReturnsOnlyFilesOlderThanThreshold()
    {
        var now = DateTimeOffset.UtcNow;
        var entries = new[]
        {
            File("old.txt", 100, now.AddDays(-31)),
            File("recent.txt", 100, now.AddDays(-5)),
        };
        var handler = BuildHandler(BuildResult(entries));

        var result = handler.Handle(DefaultQuery(age: FileAgeFilter.Stale, stale: 30));

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("old.txt");
    }

    [Fact]
    public void StaleFilter_ExactBoundary_IsNotIncluded()
    {
        var now = DateTimeOffset.UtcNow;
        var entries = new[]
        {
            // File is 29 hours short of 30 days — clearly NOT stale with a 30-day threshold
            File("nearly_30d.txt", 100, now.AddDays(-30).AddHours(29)),
        };
        var handler = BuildHandler(BuildResult(entries));

        var result = handler.Handle(DefaultQuery(age: FileAgeFilter.Stale, stale: 30));

        result.Should().BeEmpty();
    }

    // ── New filter ───────────────────────────────────────────────────────────

    [Fact]
    public void NewFilter_ReturnsOnlyFilesWithinThreshold()
    {
        var now = DateTimeOffset.UtcNow;
        var entries = new[]
        {
            File("new.txt", 100, now.AddDays(-2)),
            File("old.txt", 100, now.AddDays(-10)),
        };
        var handler = BuildHandler(BuildResult(entries));

        var result = handler.Handle(DefaultQuery(age: FileAgeFilter.New, newWithin: 7));

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("new.txt");
    }

    // ── Extension filter ─────────────────────────────────────────────────────

    [Fact]
    public void ExtensionFilter_ReturnOnlyMatchingExtensions()
    {
        var now = DateTimeOffset.UtcNow;
        var entries = new[]
        {
            File("a.log", 100, now),
            File("b.txt", 100, now),
            File("c.zip", 100, now),
        };
        var handler = BuildHandler(BuildResult(entries));

        var result = handler.Handle(DefaultQuery(ext: ".log, .zip"));

        result.Should().HaveCount(2);
        result.Select(r => r.Name).Should().BeEquivalentTo(["a.log", "c.zip"]);
    }

    [Fact]
    public void ExtensionFilter_WithoutDot_StillMatches()
    {
        var now = DateTimeOffset.UtcNow;
        var entries = new[]
        {
            File("a.log", 100, now),
            File("b.txt", 100, now),
        };
        var handler = BuildHandler(BuildResult(entries));

        var result = handler.Handle(DefaultQuery(ext: "log"));

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("a.log");
    }

    // ── Min size filter ──────────────────────────────────────────────────────

    [Fact]
    public void MinSizeFilter_ReturnsOnlyFilesAtOrAboveThreshold()
    {
        var now = DateTimeOffset.UtcNow;
        var entries = new[]
        {
            File("small.txt", 50, now),
            File("large.txt", 1000, now),
        };
        var handler = BuildHandler(BuildResult(entries));

        var result = handler.Handle(DefaultQuery(minSize: 100));

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("large.txt");
    }

    [Fact]
    public void MinSizeFilter_ExactBoundary_IsIncluded()
    {
        var now = DateTimeOffset.UtcNow;
        var entries = new[]
        {
            File("exact.txt", 100, now),
        };
        var handler = BuildHandler(BuildResult(entries));

        var result = handler.Handle(DefaultQuery(minSize: 100));

        result.Should().HaveCount(1);
    }

    // ── Name search filter ───────────────────────────────────────────────────

    [Fact]
    public void NameSearch_ReturnsOnlyMatchingNames()
    {
        var now = DateTimeOffset.UtcNow;
        var entries = new[]
        {
            File("error_2024.log", 100, now),
            File("access.log", 100, now),
            File("readme.txt", 100, now),
        };
        var handler = BuildHandler(BuildResult(entries));

        var result = handler.Handle(DefaultQuery(name: "error"));

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("error_2024.log");
    }

    [Fact]
    public void NameSearch_IsCaseInsensitive()
    {
        var now = DateTimeOffset.UtcNow;
        var entries = new[]
        {
            File("MyFile.TXT", 100, now),
            File("other.txt", 100, now),
        };
        var handler = BuildHandler(BuildResult(entries));

        var result = handler.Handle(DefaultQuery(name: "myfile"));

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("MyFile.TXT");
    }

    // ── Combined filters ─────────────────────────────────────────────────────

    [Fact]
    public void CombinedFilters_AllMustMatch()
    {
        var now = DateTimeOffset.UtcNow;
        var entries = new[]
        {
            File("old_large.log", 1000, now.AddDays(-40)),   // matches stale + ext + size
            File("old_small.log", 10, now.AddDays(-40)),     // matches stale + ext, not size
            File("new_large.log", 1000, now.AddDays(-2)),    // not stale
            File("old_large.txt", 1000, now.AddDays(-40)),   // wrong ext
        };
        var handler = BuildHandler(BuildResult(entries));

        var result = handler.Handle(DefaultQuery(
            age: FileAgeFilter.Stale,
            stale: 30,
            minSize: 100,
            ext: ".log"));

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("old_large.log");
    }

    // ── No scan result ───────────────────────────────────────────────────────

    [Fact]
    public void NoScanResult_ReturnsEmptyList()
    {
        var state = new InMemoryScanStateService();
        var handler = new FilterEntriesHandler(state);

        var result = handler.Handle(DefaultQuery());

        result.Should().BeEmpty();
    }
}
