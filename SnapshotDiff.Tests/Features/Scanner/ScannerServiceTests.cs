using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SnapshotDiff.Features.Scanner.Domain;
using SnapshotDiff.Features.Scanner.Infrastructure;

namespace SnapshotDiff.Tests.Features.Scanner;

public sealed class ScannerServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ScannerService _sut;

    public ScannerServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ScanTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        var logger = Substitute.For<ILogger<ScannerService>>();
        _sut = new ScannerService(logger);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); }
        catch { /* best effort */ }
    }

    private string CreateFile(string relativePath, string content = "x")
    {
        var full = Path.Combine(_tempDir, relativePath);
        var dir = Path.GetDirectoryName(full)!;
        Directory.CreateDirectory(dir);
        File.WriteAllText(full, content);
        return full;
    }

    // ── Basic scanning ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ScanAsync_FindsFilesAndDirectories()
    {
        CreateFile("a.txt");
        CreateFile(Path.Combine("sub", "b.log"));

        var result = await _sut.ScanAsync(new ScanOptions { RootPath = _tempDir });

        result.Entries.Should().Contain(e => e.Name == "a.txt" && e.Type == ScanEntryType.File);
        result.Entries.Should().Contain(e => e.Name == "b.log" && e.Type == ScanEntryType.File);
        result.Entries.Should().Contain(e => e.Name == "sub" && e.Type == ScanEntryType.Directory);
    }

    [Fact]
    public async Task ScanAsync_EmptyDirectory_ReturnsNoEntries()
    {
        var result = await _sut.ScanAsync(new ScanOptions { RootPath = _tempDir });

        result.Entries.Should().BeEmpty();
        result.InaccessiblePaths.Should().BeEmpty();
    }

    [Fact]
    public async Task ScanAsync_RecordsCorrectFileSize()
    {
        var content = new string('A', 512);
        CreateFile("sized.txt", content);

        var result = await _sut.ScanAsync(new ScanOptions { RootPath = _tempDir });

        var entry = result.Entries.Should().ContainSingle(e => e.Name == "sized.txt").Subject;
        entry.Size.Should().Be(512);
    }

    [Fact]
    public async Task ScanAsync_RecordsExtensionLowerCase()
    {
        CreateFile("Report.PDF");

        var result = await _sut.ScanAsync(new ScanOptions { RootPath = _tempDir });

        var entry = result.Entries.Should().ContainSingle(e => e.Type == ScanEntryType.File).Subject;
        entry.Extension.Should().Be(".pdf");
    }

    // ── Exclusions ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ScanAsync_RespectsExclusionEvaluator()
    {
        CreateFile("keep.txt");
        CreateFile("skip.log");

        var evaluator = Substitute.For<SnapshotDiff.Features.ExclusionRules.Infrastructure.IExclusionEvaluator>();
        evaluator.IsExcluded(Arg.Any<string>(), Arg.Any<bool>())
                 .Returns(ci =>
                 {
                     var path = ci.ArgAt<string>(0);
                     return path.EndsWith(".log");
                 });

        var result = await _sut.ScanAsync(new ScanOptions
        {
            RootPath = _tempDir,
            ExclusionEvaluator = evaluator
        });

        result.Entries.Should().Contain(e => e.Name == "keep.txt");
        result.Entries.Should().NotContain(e => e.Name == "skip.log");
    }

    // ── Cancellation ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ScanAsync_CancellationRequested_ThrowsOCE()
    {
        CreateFile("a.txt");

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => _sut.ScanAsync(new ScanOptions { RootPath = _tempDir }, ct: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── Progress reporting ─────────────────────────────────────────────────────

    [Fact]
    public async Task ScanAsync_ReportsProgress()
    {
        CreateFile("one.txt");
        CreateFile("two.txt");

        var reported = new List<ScanProgress>();
        var progress = new Progress<ScanProgress>(p => reported.Add(p));

        await _sut.ScanAsync(new ScanOptions { RootPath = _tempDir }, progress);

        // Allow a small delay for Progress<T> to post
        await Task.Delay(100);
        reported.Should().HaveCountGreaterOrEqualTo(1);
        reported.Should().Contain(p => p.Phase == ScanPhase.Scanning);
    }

    [Fact]
    public async Task ScanAsync_ReportsCountingPhase_WhenSubdirectoriesExist()
    {
        CreateFile(Path.Combine("sub1", "a.txt"));
        CreateFile(Path.Combine("sub2", "b.txt"));

        var reported = new List<ScanProgress>();
        var progress = new Progress<ScanProgress>(p => reported.Add(p));

        await _sut.ScanAsync(new ScanOptions { RootPath = _tempDir }, progress);
        await Task.Delay(100);

        // Final report should have TotalDirectories > 0
        var scanReports = reported.Where(p => p.Phase == ScanPhase.Scanning).ToList();
        scanReports.Should().Contain(p => p.TotalDirectories > 0);
    }

    [Fact]
    public async Task ScanAsync_PopulatesLastAccessTime()
    {
        CreateFile("test.txt");

        var result = await _sut.ScanAsync(new ScanOptions { RootPath = _tempDir });

        var entry = result.Entries.Should().ContainSingle(e => e.Name == "test.txt").Subject;
        entry.LastAccessTime.Should().NotBe(default);
    }

    [Fact]
    public async Task ScanAsync_NoEntryCap_ScansAllFiles()
    {
        for (int i = 0; i < 50; i++)
            CreateFile($"file{i}.txt");

        var result = await _sut.ScanAsync(new ScanOptions { RootPath = _tempDir });

        result.Entries.Count(e => e.Type == ScanEntryType.File).Should().Be(50);
    }

    // ── Invalid input ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ScanAsync_InvalidRootPath_Throws(string? path)
    {
        var act = () => _sut.ScanAsync(new ScanOptions { RootPath = path! });

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
