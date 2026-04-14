using System.Runtime.InteropServices;

namespace SnapshotDiff.MAUI.WinUI;

/// <summary>
/// Manages a Windows system tray icon using Win32 Shell_NotifyIcon P/Invoke.
/// WinForms/WPF cannot be mixed with MAUI XAML, so we use the Win32 API directly.
/// </summary>
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
internal sealed class TrayIconService : IDisposable
{
    private const uint NIM_ADD = 0x00000000;
    private const uint NIM_DELETE = 0x00000002;
    private const uint NIM_SETVERSION = 0x00000004;
    private const uint NIF_MESSAGE = 0x00000001;
    private const uint NIF_ICON = 0x00000002;
    private const uint NIF_TIP = 0x00000004;
    private const uint NOTIFYICON_VERSION_4 = 4;
    private const uint WM_APP = 0x8000;
    private const uint WM_LBUTTONDBLCLK = 0x0203;
    private const uint WM_RBUTTONUP = 0x0205;
    private const uint WM_CONTEXTMENU = 0x007B;

    private readonly Microsoft.UI.Xaml.Application _app;
#pragma warning disable CS0169 // Stub — will be used when tray icon is fully implemented
    private NotifyIconData _notifyData;
#pragma warning restore CS0169
    private bool _disposed;

    public TrayIconService(Microsoft.UI.Xaml.Application app)
    {
        _app = app;
    }

    public void Initialize()
    {
        // Basic tray icon setup - full implementation requires a message-pump window
        // For now this is a stub that will be completed in the Scheduler feature milestone
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NotifyIconData
    {
        public uint cbSize;
        public nint hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public nint hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public uint dwState;
        public uint dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public uint uVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public uint dwInfoFlags;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern bool Shell_NotifyIcon(uint dwMessage, ref NotifyIconData lpData);
}
