using SnapshotDiff.Infrastructure.Storage;

namespace SnapshotDiff.MAUI.Services;

/// <summary>
/// Cross-platform folder picker service for MAUI.
/// Windows: uses WinRT FolderPicker (Windows.Storage.Pickers).
/// Android: returns the external storage root so the Blazor tree dialog starts there.
/// iOS/macOS: returns the documents directory.
/// All other platforms: not supported (falls back to the Blazor tree dialog).
/// </summary>
public sealed class MauiFolderPickerService : IFolderPickerService
{
    public bool IsSupported =>
#if WINDOWS
        true;
#elif ANDROID || IOS || MACCATALYST
        true;
#else
        false;
#endif

    public async Task<string?> PickFolderAsync(CancellationToken ct = default)
    {
#if WINDOWS
        return await PickWindowsAsync(ct);
#elif ANDROID
        return PickAndroid();
#elif IOS || MACCATALYST
        return PickApple();
#else
        return await Task.FromResult<string?>(null);
#endif
    }

#if WINDOWS
    private static async Task<string?> PickWindowsAsync(CancellationToken ct)
    {
        var picker = new Windows.Storage.Pickers.FolderPicker();
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
        picker.FileTypeFilter.Add("*");

        // WinUI3 requires the picker to be initialized with the HWND
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(
            ((Microsoft.UI.Xaml.Window)Application.Current!.Windows[0].Handler!.PlatformView!));
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var folder = await picker.PickSingleFolderAsync().AsTask(ct);
        return folder?.Path;
    }
#endif

#if ANDROID
    private static string? PickAndroid()
    {
        // Returns external storage root; the Blazor tree dialog navigates from here.
        // Full SAF Activity-result callback integration is a future enhancement.
        return Android.OS.Environment.ExternalStorageDirectory?.AbsolutePath;
    }
#endif

#if IOS || MACCATALYST
    private static string? PickApple()
    {
        // Returns the user's documents directory on iOS/macOS.
        return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }
#endif
}
