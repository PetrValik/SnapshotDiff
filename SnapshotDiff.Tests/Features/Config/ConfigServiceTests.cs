using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SnapshotDiff.Features.Config.Domain;
using SnapshotDiff.Features.Config.Infrastructure;
using SnapshotDiff.Infrastructure.Storage;

namespace SnapshotDiff.Tests.Features.Config;

public sealed class ConfigServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly IStoragePathProvider _storagePath;
    private readonly ILogger<ConfigService> _logger;

    public ConfigServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"sd-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _storagePath = Substitute.For<IStoragePathProvider>();
        _storagePath.AppDataDirectory.Returns(_tempDir);
        _logger = Substitute.For<ILogger<ConfigService>>();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private ConfigService CreateSut() => new(_logger, _storagePath);

    // ── Load ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadAsync_NoFile_ReturnsDefaults()
    {
        using var sut = CreateSut();
        var config = await sut.LoadAsync();

        config.Should().NotBeNull();
        config.WatchedDirectories.Should().BeEmpty();
        config.DefaultStaleAfterDays.Should().Be(365);
        config.DefaultNewWithinDays.Should().Be(30);
    }

    [Fact]
    public async Task LoadAsync_CorruptedJson_ReturnsDefaults()
    {
        var configPath = Path.Combine(_tempDir, "config.json");
        await File.WriteAllTextAsync(configPath, "NOT VALID JSON {{{{");

        using var sut = CreateSut();
        var config = await sut.LoadAsync();

        config.Should().NotBeNull();
        config.WatchedDirectories.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_ValidFile_RestoresConfig()
    {
        using var sut = CreateSut();
        sut.Current.DefaultStaleAfterDays = 42;
        await sut.SaveAsync();

        using var sut2 = CreateSut();
        var config = await sut2.LoadAsync();

        config.DefaultStaleAfterDays.Should().Be(42);
    }

    // ── Save ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_CreatesJsonFile()
    {
        using var sut = CreateSut();
        await sut.SaveAsync();

        var configPath = Path.Combine(_tempDir, "config.json");
        File.Exists(configPath).Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_AtomicWrite_NoTmpLeftOver()
    {
        using var sut = CreateSut();
        await sut.SaveAsync();

        var tmpPath = Path.Combine(_tempDir, "config.json.tmp");
        File.Exists(tmpPath).Should().BeFalse();
    }

    // ── WatchedDirectories ───────────────────────────────────────────────────

    [Fact]
    public async Task AddWatchedDirectory_AddsAndPersists()
    {
        using var sut = CreateSut();
        await sut.AddWatchedDirectoryAsync(_tempDir, "Test");

        sut.Current.WatchedDirectories.Should().ContainSingle()
            .Which.Label.Should().Be("Test");

        // Verify it persisted
        using var sut2 = CreateSut();
        var config = await sut2.LoadAsync();
        config.WatchedDirectories.Should().ContainSingle();
    }

    [Fact]
    public async Task AddWatchedDirectory_Duplicate_DoesNotAddTwice()
    {
        using var sut = CreateSut();
        await sut.AddWatchedDirectoryAsync(_tempDir);
        await sut.AddWatchedDirectoryAsync(_tempDir);

        sut.Current.WatchedDirectories.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddWatchedDirectory_RaisesConfigChanged()
    {
        using var sut = CreateSut();
        var raised = false;
        sut.ConfigChanged += (_, _) => raised = true;

        await sut.AddWatchedDirectoryAsync(_tempDir);

        raised.Should().BeTrue();
    }

    [Fact]
    public async Task RemoveWatchedDirectory_RemovesAndPersists()
    {
        using var sut = CreateSut();
        await sut.AddWatchedDirectoryAsync(_tempDir);
        await sut.RemoveWatchedDirectoryAsync(_tempDir);

        sut.Current.WatchedDirectories.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveWatchedDirectory_NotFound_DoesNotThrow()
    {
        using var sut = CreateSut();
        await sut.Invoking(s => s.RemoveWatchedDirectoryAsync(@"C:\nonexistent"))
                 .Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateWatchedDirectory_AppliesMutation()
    {
        using var sut = CreateSut();
        await sut.AddWatchedDirectoryAsync(_tempDir, "Old");
        await sut.UpdateWatchedDirectoryAsync(_tempDir, d => d.Label = "New");

        sut.Current.WatchedDirectories.First().Label.Should().Be("New");
    }

    [Fact]
    public async Task UpdateWatchedDirectory_NotFound_DoesNotThrow()
    {
        using var sut = CreateSut();
        await sut.Invoking(s => s.UpdateWatchedDirectoryAsync(@"C:\nonexistent", _ => { }))
                 .Should().NotThrowAsync();
    }

    // ── Reset ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ResetAsync_DeletesFileAndRestoresDefaults()
    {
        using var sut = CreateSut();
        sut.Current.DefaultStaleAfterDays = 999;
        await sut.SaveAsync();
        await sut.ResetAsync();

        sut.Current.DefaultStaleAfterDays.Should().Be(365);
        File.Exists(Path.Combine(_tempDir, "config.json")).Should().BeFalse();
    }

    [Fact]
    public async Task ResetAsync_RaisesConfigChanged()
    {
        using var sut = CreateSut();
        var raised = false;
        sut.ConfigChanged += (_, _) => raised = true;

        await sut.ResetAsync();

        raised.Should().BeTrue();
    }

    // ── Dispose ──────────────────────────────────────────────────────────────

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var sut = CreateSut();
        sut.Dispose();
        sut.Invoking(s => s.Dispose()).Should().NotThrow();
    }
}
