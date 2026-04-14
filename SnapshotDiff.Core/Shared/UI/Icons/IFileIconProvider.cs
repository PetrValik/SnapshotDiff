namespace SnapshotDiff.Shared.UI.Icons;

/// <summary>
/// Maps file extensions and entry types to display icons.
/// </summary>
public interface IFileIconProvider
{
    /// <summary>
    /// Returns an icon (emoji) for a given file extension.
    /// </summary>
    string GetFileIcon(string extension);

    /// <summary>
    /// Returns a CSS class for color-coding by file category.
    /// </summary>
    string GetIconClass(string extension);

    /// <summary>
    /// Icon for a directory entry.
    /// </summary>
    string DirectoryIcon { get; }
}
