using FluentAssertions;
using SnapshotDiff.Features.Scanner.Domain;

namespace SnapshotDiff.Tests.Features.Scanner;

public sealed class DirectoryNodeTests
{
    [Fact]
    public void BuildFromEntries_EmptyList_ReturnsEmptyRoot()
    {
        var root = DirectoryNode.BuildFromEntries([], "Root");

        root.Name.Should().Be("Root");
        root.RelativePath.Should().BeEmpty();
        root.TotalFileCount.Should().Be(0);
        root.Children.Should().BeEmpty();
    }

    [Fact]
    public void BuildFromEntries_FilesInRoot_CountsDirectly()
    {
        var entries = new List<ScanEntry>
        {
            MakeFile("a.txt", 100),
            MakeFile("b.txt", 200),
        };

        var root = DirectoryNode.BuildFromEntries(entries, "Test");

        root.TotalFileCount.Should().Be(2);
        root.DirectFileCount.Should().Be(2);
        root.TotalSize.Should().Be(300);
        root.Children.Should().BeEmpty();
    }

    [Fact]
    public void BuildFromEntries_NestedDirectories_BuildsTree()
    {
        var entries = new List<ScanEntry>
        {
            MakeFile(Path.Combine("docs", "readme.md"), 50),
            MakeFile(Path.Combine("docs", "guide.md"), 150),
            MakeFile(Path.Combine("src", "app.cs"), 300),
            MakeFile(Path.Combine("src", "lib", "util.cs"), 400),
        };

        var root = DirectoryNode.BuildFromEntries(entries, "Project");

        root.TotalFileCount.Should().Be(4);
        root.TotalSize.Should().Be(900);
        root.Children.Should().HaveCount(2);

        var docs = root.Children.Should().ContainSingle(c => c.Name == "docs").Subject;
        docs.TotalFileCount.Should().Be(2);
        docs.DirectFileCount.Should().Be(2);
        docs.TotalSize.Should().Be(200);

        var src = root.Children.Should().ContainSingle(c => c.Name == "src").Subject;
        src.TotalFileCount.Should().Be(2);
        src.DirectFileCount.Should().Be(1);

        var lib = src.Children.Should().ContainSingle(c => c.Name == "lib").Subject;
        lib.TotalFileCount.Should().Be(1);
        lib.DirectFileCount.Should().Be(1);
        lib.TotalSize.Should().Be(400);
    }

    [Fact]
    public void BuildFromEntries_ChildrenAreSortedAlphabetically()
    {
        var entries = new List<ScanEntry>
        {
            MakeFile(Path.Combine("zebra", "z.txt"), 10),
            MakeFile(Path.Combine("alpha", "a.txt"), 10),
            MakeFile(Path.Combine("mid", "m.txt"), 10),
        };

        var root = DirectoryNode.BuildFromEntries(entries, "Root");

        root.Children.Select(c => c.Name).Should().BeInAscendingOrder();
    }

    [Fact]
    public void BuildFromEntries_SkipsDirectoryEntries()
    {
        var entries = new List<ScanEntry>
        {
            new()
            {
                FullPath = @"C:\test\sub",
                RelativePath = "sub",
                Name = "sub",
                Size = 0,
                LastWriteTime = DateTimeOffset.UtcNow,
                Type = ScanEntryType.Directory,
                Extension = string.Empty
            },
            MakeFile(Path.Combine("sub", "file.txt"), 100),
        };

        var root = DirectoryNode.BuildFromEntries(entries, "Root");

        root.TotalFileCount.Should().Be(1);
    }

    [Fact]
    public void BuildFromEntries_AggregatesUpward()
    {
        var entries = new List<ScanEntry>
        {
            MakeFile(Path.Combine("a", "b", "c", "deep.txt"), 1000),
            MakeFile("root.txt", 500),
        };

        var root = DirectoryNode.BuildFromEntries(entries, "R");

        root.TotalFileCount.Should().Be(2);
        root.TotalSize.Should().Be(1500);
        root.DirectFileCount.Should().Be(1);

        var a = root.Children.Should().ContainSingle().Subject;
        a.TotalFileCount.Should().Be(1);
        a.TotalSize.Should().Be(1000);
    }

    [Fact]
    public void BuildFromEntries_RootIsExpanded()
    {
        var root = DirectoryNode.BuildFromEntries([], "R");
        root.IsExpanded.Should().BeTrue();
    }

    private static ScanEntry MakeFile(string relativePath, long size) => new()
    {
        FullPath = Path.Combine(@"C:\test", relativePath),
        RelativePath = relativePath,
        Name = Path.GetFileName(relativePath),
        Size = size,
        LastWriteTime = DateTimeOffset.UtcNow,
        Type = ScanEntryType.File,
        Extension = Path.GetExtension(relativePath).ToLowerInvariant()
    };
}
