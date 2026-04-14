namespace SnapshotDiff.Infrastructure.Storage;

/// <summary>
/// Cross-platform folder/directory picker service.
/// </summary>
public interface IFolderPickerService
{
    /// <summary>
    /// Opens a folder picker dialog and returns the selected path,
    /// or null if the user cancels.
    /// </summary>
    Task<string?> PickFolderAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns true if the current platform supports a native folder picker UI.
    /// </summary>
    bool IsSupported { get; }
}
