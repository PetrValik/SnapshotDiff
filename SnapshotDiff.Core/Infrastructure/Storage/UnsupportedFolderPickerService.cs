namespace SnapshotDiff.Infrastructure.Storage;

/// <summary>
/// Fallback IFolderPickerService for platforms where no native picker is available (unit tests).
/// Always returns null and reports IsSupported = false.
/// </summary>
internal sealed class UnsupportedFolderPickerService : IFolderPickerService
{
    public bool IsSupported => false;

    public Task<string?> PickFolderAsync(CancellationToken ct = default) =>
        Task.FromResult<string?>(null);
}
