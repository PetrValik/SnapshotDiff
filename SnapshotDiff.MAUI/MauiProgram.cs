using System.Globalization;
using Microsoft.Extensions.Logging;
using SnapshotDiff.Features.Config;
using SnapshotDiff.Features.ExclusionRules.Infrastructure;
using SnapshotDiff.Features.Export.Infrastructure;
using SnapshotDiff.Features.Scanner.Infrastructure;
using SnapshotDiff.Features.Trash.Infrastructure;
using SnapshotDiff.Infrastructure;
using SnapshotDiff.Infrastructure.Localization;
using SnapshotDiff.Infrastructure.Storage;
using SnapshotDiff.MAUI.Services;

namespace SnapshotDiff.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddMauiBlazorWebView();

        // MAUI storage path provider
        builder.Services.AddSingleton<IStoragePathProvider, MauiStoragePathProvider>();

        // MAUI cross-platform folder picker
        builder.Services.AddSingleton<IFolderPickerService, MauiFolderPickerService>();

        // Core services shared across all platforms
        builder.Services.AddInfrastructure();
        builder.Services.AddConfig();
        builder.Services.AddScanner();
        builder.Services.AddExport();
        builder.Services.AddLocalization();
        builder.Services.AddTrash();
        builder.Services.AddExclusionRules();

        // MAUI-specific: culture switcher
        builder.Services.AddScoped<ICultureService, MauiCultureService>();

        // Apply language preference saved from a previous session
        ApplySavedCulture();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void ApplySavedCulture()
    {
        try
        {
            var configPath = AppPaths.GetDefaultConfigPath();
            if (!File.Exists(configPath)) return;

            using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(configPath));
            if (doc.RootElement.TryGetProperty("Appearance", out var appearance) &&
                appearance.TryGetProperty("Language", out var langProp))
            {
                var lang = langProp.GetString();
                if (!string.IsNullOrEmpty(lang) && SupportedCultures.All.Contains(lang))
                    MauiCultureService.ApplyCulture(lang);
            }
        }
        catch
        {
            // Default culture (en) is used
        }
    }
}