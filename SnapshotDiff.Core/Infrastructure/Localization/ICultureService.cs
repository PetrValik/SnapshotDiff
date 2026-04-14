namespace SnapshotDiff.Infrastructure.Localization;

public interface ICultureService
{
    Task SetCultureAsync(string culture);
}

/// <summary>
/// Shared culture state that signals UI components to re-render when culture changes.
/// Registered as singleton; used by CultureSelector and Routes components.
/// </summary>
public sealed class CultureState
{
    public int Version { get; private set; }
    public string Culture { get; private set; } = SupportedCultures.Default;
    public event Action? OnChanged;

    public void NotifyChanged(string culture)
    {
        Culture = culture;
        Version++;
        OnChanged?.Invoke();
    }
}
