namespace SnapshotDiff.Infrastructure.Permissions;

/// <summary>
/// Runtime permission service for platforms that require explicit file system access grants
/// (Android, iOS). Desktop platforms return true immediately without showing any dialog.
/// </summary>
public interface IPlatformPermissionService
{
    /// <summary>
    /// Requests read access to external/shared storage.
    /// Returns true if access is granted or not required on this platform.
    /// </summary>
    Task<bool> RequestStorageReadAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns true if the app currently has storage read access.
    /// </summary>
    Task<bool> HasStorageReadAccessAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns true if this platform requires explicit runtime storage permission grants.
    /// </summary>
    bool RequiresExplicitPermission { get; }
}
