using System.Globalization;
using SnapshotDiff.Infrastructure.Localization;

namespace SnapshotDiff.Services;

/// <summary>
/// Web host culture service. Applies CultureInfo on current thread and notifies CultureState.
/// </summary>
public sealed class WebCultureService(CultureState cultureState) : ICultureService
{
    public Task SetCultureAsync(string culture)
    {
        if (!SupportedCultures.All.Contains(culture, StringComparer.OrdinalIgnoreCase))
            culture = SupportedCultures.Default;

        var ci = new CultureInfo(culture);
        Thread.CurrentThread.CurrentCulture = ci;
        Thread.CurrentThread.CurrentUICulture = ci;
        CultureInfo.DefaultThreadCurrentCulture = ci;
        CultureInfo.DefaultThreadCurrentUICulture = ci;

        cultureState.NotifyChanged(culture);
        return Task.CompletedTask;
    }
}
