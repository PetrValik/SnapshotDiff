using FluentAssertions;
using SnapshotDiff.Infrastructure.Persistence;
using System.Text.Json;

namespace SnapshotDiff.Tests.Infrastructure;

public class JsonStateStorageTests : IDisposable
{
    private readonly string _tempDir;
    private readonly List<string> _filesToClean = new();

    public JsonStateStorageTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"JsonStateStorageTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { /* ignore */ }
    }

    // ── constructor validation ────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NullOrWhitespacePath_ThrowsArgumentException(string? path)
    {
        var act = () => new JsonStateStorage<TestState>(path!);

        act.Should().Throw<ArgumentException>();
    }

    // ── LoadAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadAsync_FileDoesNotExist_ReturnsDefaultInstance()
    {
        var path = FilePath("nonexistent.json");
        using var storage = new JsonStateStorage<TestState>(path);

        var result = await storage.LoadAsync();

        result.Should().NotBeNull();
        result.Name.Should().Be(new TestState().Name);
    }

    [Fact]
    public async Task LoadAsync_ValidJson_DeserializesCorrectly()
    {
        var path = FilePath("valid.json");
        var expected = new TestState { Name = "Alice", Value = 99 };
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(expected));

        using var storage = new JsonStateStorage<TestState>(path);
        var result = await storage.LoadAsync();

        result.Name.Should().Be("Alice");
        result.Value.Should().Be(99);
    }

    [Fact]
    public async Task LoadAsync_CorruptJson_ReturnsDefaultInstance()
    {
        var path = FilePath("corrupt.json");
        await File.WriteAllTextAsync(path, "{ not valid json !!!}");

        using var storage = new JsonStateStorage<TestState>(path);
        var result = await storage.LoadAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadAsync_EmptyFile_ReturnsDefaultInstance()
    {
        var path = FilePath("empty.json");
        await File.WriteAllTextAsync(path, "");

        using var storage = new JsonStateStorage<TestState>(path);
        var result = await storage.LoadAsync();

        result.Should().NotBeNull();
    }

    // ── SaveAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_WritesFileToExpectedPath()
    {
        var path = FilePath("save_test.json");
        using var storage = new JsonStateStorage<TestState>(path);
        var state = new TestState { Name = "Bob", Value = 7 };

        await storage.SaveAsync(state);

        File.Exists(path).Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_ThenLoad_RoundtripsData()
    {
        var path = FilePath("roundtrip.json");
        using var storage = new JsonStateStorage<TestState>(path);
        var state = new TestState { Name = "Charlie", Value = 42 };

        await storage.SaveAsync(state);
        var loaded = await storage.LoadAsync();

        loaded.Name.Should().Be("Charlie");
        loaded.Value.Should().Be(42);
    }

    [Fact]
    public async Task SaveAsync_NullState_ThrowsArgumentNullException()
    {
        var path = FilePath("null_test.json");
        using var storage = new JsonStateStorage<TestState>(path);

        var act = () => storage.SaveAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SaveAsync_CreatesDirectoryIfNotExists()
    {
        var subDir = Path.Combine(_tempDir, "subdir", "nested");
        var path = Path.Combine(subDir, "state.json");
        using var storage = new JsonStateStorage<TestState>(path);

        await storage.SaveAsync(new TestState { Name = "DirTest", Value = 1 });

        Directory.Exists(subDir).Should().BeTrue();
        File.Exists(path).Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_OverwritesPreviousFile()
    {
        var path = FilePath("overwrite.json");
        using var storage = new JsonStateStorage<TestState>(path);

        await storage.SaveAsync(new TestState { Name = "First", Value = 1 });
        await storage.SaveAsync(new TestState { Name = "Second", Value = 2 });

        var loaded = await storage.LoadAsync();
        loaded.Name.Should().Be("Second");
        loaded.Value.Should().Be(2);
    }

    // ── Dispose ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task LoadAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var path = FilePath("disposed.json");
        var storage = new JsonStateStorage<TestState>(path);
        storage.Dispose();

        var act = () => storage.LoadAsync();

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task SaveAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var path = FilePath("disposed_save.json");
        var storage = new JsonStateStorage<TestState>(path);
        storage.Dispose();

        var act = () => storage.SaveAsync(new TestState());

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private string FilePath(string name) => Path.Combine(_tempDir, name);

    public class TestState
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
