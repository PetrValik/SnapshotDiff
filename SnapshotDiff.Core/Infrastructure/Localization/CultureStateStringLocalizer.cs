using System.Globalization;
using Microsoft.Extensions.Localization;

namespace SnapshotDiff.Infrastructure.Localization;

/// <summary>
/// Wrapper around IStringLocalizer that ensures thread culture matches
/// CultureState before every string lookup. Prevents stale thread culture
/// from displaying localized strings in the wrong language after async hops.
/// </summary>
public sealed class CultureStateStringLocalizer<T>(IStringLocalizerFactory factory, CultureState cultureState) : IStringLocalizer<T>
{
    private readonly IStringLocalizer _inner = factory.Create(typeof(T));

    public LocalizedString this[string name]
    {
        get
        {
            SyncCulture();
            return _inner[name];
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            SyncCulture();
            return _inner[name, arguments];
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        SyncCulture();
        return _inner.GetAllStrings(includeParentCultures);
    }

    private void SyncCulture()
    {
        var desired = cultureState.Culture;
        if (string.Equals(Thread.CurrentThread.CurrentUICulture.Name, desired, StringComparison.OrdinalIgnoreCase))
            return;
        try
        {
            var ci = new CultureInfo(desired);
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }
        catch (CultureNotFoundException) { /* keep current culture on invalid/unsupported culture string */ }
    }
}
