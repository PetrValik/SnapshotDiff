using global::Android.Content.PM;
using global::Android.OS;
using Microsoft.Maui.ApplicationModel;
using SnapshotDiff.Infrastructure.Permissions;

namespace SnapshotDiff.MAUI.Platforms.Android.Services;

/// <summary>
/// Android runtime permission service for storage access.
/// On Android 11+ uses MANAGE_EXTERNAL_STORAGE, on older versions READ_EXTERNAL_STORAGE.
/// </summary>
public sealed class AndroidPermissionService : IPlatformPermissionService
{
    public bool RequiresExplicitPermission => true;

    public async Task<bool> RequestStorageReadAsync(CancellationToken ct = default)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(30))
        {
            // Android 11+: request MANAGE_EXTERNAL_STORAGE via settings
            if (!global::Android.OS.Environment.IsExternalStorageManager)
            {
                var intent = new global::Android.Content.Intent(
                    global::Android.Provider.Settings.ActionManageAllFilesAccessPermission);
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Platform.CurrentActivity?.StartActivity(intent));

                // Give the user time to grant, check result
                await Task.Delay(500, ct);
                return global::Android.OS.Environment.IsExternalStorageManager;
            }
            return true;
        }
        else
        {
            // Android < 11: use standard permission request
            var status = await Permissions.RequestAsync<Permissions.StorageRead>();
            return status == PermissionStatus.Granted;
        }
    }

    public async Task<bool> HasStorageReadAccessAsync(CancellationToken ct = default)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(30))
            return global::Android.OS.Environment.IsExternalStorageManager;

        var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        return status == PermissionStatus.Granted;
    }
}
