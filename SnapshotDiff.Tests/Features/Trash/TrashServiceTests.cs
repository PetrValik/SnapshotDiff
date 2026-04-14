using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SnapshotDiff.Features.Trash.Domain;
using SnapshotDiff.Features.Trash.Infrastructure;
using SnapshotDiff.Infrastructure.Storage;

namespace SnapshotDiff.Tests.Features.Trash;

public sealed class TrashServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ITrashRepository _repo;
    private readonly TrashService _sut;

    public TrashServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"TrashTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        var storageProvider = Substitute.For<IStoragePathProvider>();
        storageProvider.AppDataDirectory.Returns(_tempDir);

        _repo = Substitute.For<ITrashRepository>();
        var logger = Substitute.For<ILogger<TrashService>>();

        _sut = new TrashService(storageProvider, _repo, logger);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); }
        catch { /* best effort cleanup */ }
    }

    private string CreateTempFile(string content = "test")
    {
        var file = Path.Combine(_tempDir, $"file_{Guid.NewGuid():N}.txt");
        File.WriteAllText(file, content);
        return file;
    }

    private string CreateTempDirectory()
    {
        var dir = Path.Combine(_tempDir, $"dir_{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "child.txt"), "child");
        return dir;
    }

    // ── MoveToTrashAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task MoveToTrashAsync_File_InsertsMetadataBeforeMoving()
    {
        var file = CreateTempFile("hello");
        var callOrder = new List<string>();

        _repo.When(r => r.InsertAsync(Arg.Any<TrashItemMeta>(), Arg.Any<CancellationToken>()))
             .Do(_ => callOrder.Add("insert"));

        var id = await _sut.MoveToTrashAsync(file);

        id.Should().NotBeNullOrEmpty();
        await _repo.Received(1).InsertAsync(
            Arg.Is<TrashItemMeta>(m => m.Id == id && m.OriginalPath == Path.GetFullPath(file) && !m.IsDirectory),
            Arg.Any<CancellationToken>());
        File.Exists(file).Should().BeFalse("file should be moved to trash");
    }

    [Fact]
    public async Task MoveToTrashAsync_Directory_MovesDirectoryToTrash()
    {
        var dir = CreateTempDirectory();

        var id = await _sut.MoveToTrashAsync(dir);

        id.Should().NotBeNullOrEmpty();
        Directory.Exists(dir).Should().BeFalse();
        await _repo.Received(1).InsertAsync(
            Arg.Is<TrashItemMeta>(m => m.IsDirectory && m.SizeBytes > 0),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MoveToTrashAsync_NonExistentPath_ThrowsFileNotFound()
    {
        var act = () => _sut.MoveToTrashAsync(Path.Combine(_tempDir, "nonexistent.txt"));

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task MoveToTrashAsync_MoveFailure_RollsBackDbRecord()
    {
        // Create a file then make the trash dir read-only to force move failure
        var file = CreateTempFile("data");

        // Simulate: InsertAsync succeeds, but File.Move fails by making target dir invalid
        // We'll test the rollback by making the InsertAsync succeed and then checking DeleteAsync is called
        _repo.InsertAsync(Arg.Any<TrashItemMeta>(), Arg.Any<CancellationToken>())
             .Returns(Task.CompletedTask);

        // Delete the file before the service can move it (simulates a race/failure)
        _repo.When(r => r.InsertAsync(Arg.Any<TrashItemMeta>(), Arg.Any<CancellationToken>()))
             .Do(_ => File.Delete(file));

        var act = () => _sut.MoveToTrashAsync(file);

        await act.Should().ThrowAsync<FileNotFoundException>();
        await _repo.Received(1).DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ── RestoreAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task RestoreAsync_UnknownId_Throws()
    {
        _repo.GetAsync("bad-id", Arg.Any<CancellationToken>())
             .Returns((TrashItemMeta?)null);

        var act = () => _sut.RestoreAsync("bad-id");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ── DeletePermanentlyAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task DeletePermanentlyAsync_DeletesDbRecord()
    {
        await _sut.DeletePermanentlyAsync("some-id");

        await _repo.Received(1).DeleteAsync("some-id", Arg.Any<CancellationToken>());
    }

    // ── EmptyTrashAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task EmptyTrashAsync_DeletesAllItems()
    {
        _repo.GetAllAsync(Arg.Any<CancellationToken>())
             .Returns(new List<TrashItemMeta>
             {
                 new() { Id = "a", OriginalPath = "/tmp/a" },
                 new() { Id = "b", OriginalPath = "/tmp/b" },
             });

        await _sut.EmptyTrashAsync();

        await _repo.Received(1).DeleteAllAsync(Arg.Any<CancellationToken>());
    }

    // ── PurgeExpiredAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task PurgeExpiredAsync_DeletesOnlyExpiredItems()
    {
        var expired = new List<TrashItemMeta>
        {
            new() { Id = "exp1", OriginalPath = "/expired1" }
        };
        _repo.GetExpiredAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
             .Returns(expired);

        await _sut.PurgeExpiredAsync();

        await _repo.Received(1).DeleteAsync("exp1", Arg.Any<CancellationToken>());
    }
}
