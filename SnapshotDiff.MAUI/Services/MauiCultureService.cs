using System.Globalization;
using SnapshotDiff.Features.Config.Infrastructure;
using SnapshotDiff.Infrastructure.Localization;

namespace SnapshotDiff.MAUI.Services;

/// <summary>
/// MAUI-specific culture switcher: applies the culture directly on the thread,
/// persists to config, and notifies the shared CultureState so all components re-render.
/// </summary>
public sealed class MauiCultureService(IConfigService configService, CultureState cultureState) : ICultureService
{
    public async Task SetCultureAsync(string culture)
    {
        if (!SupportedCultures.All.Contains(culture, StringComparer.OrdinalIgnoreCase))
            culture = SupportedCultures.Default;

        ApplyCulture(culture);
        configService.Current.Appearance.Language = culture;

        try
        {
            await configService.SaveAsync();
        }
        catch
        {
            // Persist failed but culture is already applied on the thread — continue
        }

        cultureState.NotifyChanged(culture);
    }

    internal static void ApplyCulture(string culture)
    {
        var ci = new CultureInfo(culture);
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        CultureInfo.DefaultThreadCurrentCulture = ci;
        CultureInfo.DefaultThreadCurrentUICulture = ci;
    }
}
