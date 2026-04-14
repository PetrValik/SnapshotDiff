namespace SnapshotDiff.Features.Scanner.Domain;

/// <summary>
/// Represents a node in the scanned directory tree, aggregating file counts and sizes
/// from all descendant files. Used by the UI to display a collapsible folder view.
/// </summary>
public sealed class DirectoryNode
{
    /// <summary>
    /// Directory name (the last path segment).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Path relative to the scan root. Empty string for the root node.
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// Number of files directly inside this directory (not in subdirectories).
    /// </summary>
    public int DirectFileCount { get; set; }

    /// <summary>
    /// Total size in bytes of files directly inside this directory.
    /// </summary>
    public long DirectSize { get; set; }

    /// <summary>
    /// Total number of files in this directory and all its descendants.
    /// </summary>
    public int TotalFileCount { get; set; }

    /// <summary>
    /// Total size in bytes of all files in this directory and all its descendants.
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// Immediate child directory nodes, sorted alphabetically by name.
    /// </summary>
    public List<DirectoryNode> Children { get; } = [];

    /// <summary>
    /// Whether this node is expanded in the tree view UI.
    /// </summary>
    public bool IsExpanded { get; set; }

    /// <summary>
    /// Builds a <see cref="DirectoryNode"/> tree from a flat list of scan entries.
    /// Only <see cref="ScanEntryType.File"/> entries contribute to counts and sizes.
    /// </summary>
    /// <param name="entries">Flat list of scan entries produced by the scanner.</param>
    /// <param name="rootName">Display name for the root node.</param>
    /// <returns>Root <see cref="DirectoryNode"/> with all descendants populated and sorted.</returns>
    public static DirectoryNode BuildFromEntries(IReadOnlyList<ScanEntry> entries, string rootName)
    {
        var root = new DirectoryNode
        {
            Name = rootName,
            RelativePath = string.Empty,
            IsExpanded = true
        };

        var nodeMap = new Dictionary<string, DirectoryNode>(StringComparer.OrdinalIgnoreCase)
        {
            [string.Empty] = root
        };

        foreach (var entry in entries)
        {
            if (entry.Type != ScanEntryType.File) continue;

            var dirPath = Path.GetDirectoryName(entry.RelativePath) ?? string.Empty;
            var node = GetOrCreate(nodeMap, dirPath);
            node.DirectFileCount++;
            node.DirectSize += entry.Size;
        }

        Aggregate(root);
        SortChildren(root);
        return root;
    }

    private static DirectoryNode GetOrCreate(Dictionary<string, DirectoryNode> map, string relativePath)
    {
        if (map.TryGetValue(relativePath, out var existing))
            return existing;

        var parentPath = Path.GetDirectoryName(relativePath) ?? string.Empty;
        var parent = GetOrCreate(map, parentPath);

        var node = new DirectoryNode
        {
            Name = Path.GetFileName(relativePath),
            RelativePath = relativePath
        };
        parent.Children.Add(node);
        map[relativePath] = node;

        return node;
    }

    private static void Aggregate(DirectoryNode node)
    {
        node.TotalFileCount = node.DirectFileCount;
        node.TotalSize = node.DirectSize;

        foreach (var child in node.Children)
        {
            Aggregate(child);
            node.TotalFileCount += child.TotalFileCount;
            node.TotalSize += child.TotalSize;
        }
    }

    private static void SortChildren(DirectoryNode node)
    {
        node.Children.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        foreach (var child in node.Children)
            SortChildren(child);
    }
}
