namespace SnapshotDiff.Infrastructure.Permissions;

/// <summary>
/// Default permission service for desktop platforms (Windows, Linux, macOS)
/// where storage access does not require explicit runtime grants.
/// </summary>
public sealed class DefaultPermissionService : IPlatformPermissionService
{
    public bool RequiresExplicitPermission => false;

    public Task<bool> RequestStorageReadAsync(CancellationToken ct = default) =>
        Task.FromResult(true);

    public Task<bool> HasStorageReadAccessAsync(CancellationToken ct = default) =>
        Task.FromResult(true);
}
