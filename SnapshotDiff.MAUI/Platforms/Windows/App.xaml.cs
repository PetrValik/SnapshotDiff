using Microsoft.UI.Xaml;

namespace SnapshotDiff.MAUI.WinUI;

public partial class App : MauiWinUIApplication
{
    private TrayIconService? _trayIconService;

    public App()
    {
        this.InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
        _trayIconService = new TrayIconService(this);
        _trayIconService.Initialize();
    }
}

