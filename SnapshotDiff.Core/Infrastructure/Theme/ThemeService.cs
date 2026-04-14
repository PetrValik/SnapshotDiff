using Microsoft.JSInterop;
using SnapshotDiff.Features.Config.Infrastructure;

namespace SnapshotDiff.Infrastructure.Theme;

/// <summary>
/// Applies and persists the colour theme by toggling CSS classes on &lt;html&gt;.
/// </summary>
public sealed class ThemeService(IConfigService config, IJSRuntime js) : IThemeService, IAsyncDisposable
{
    private IJSObjectReference? _module;

    /// <summary>
    /// Apply the theme stored in config. Call once after Blazor renders.
    /// </summary>
    public async Task ApplyAsync()
    {
        try
        {
            _module ??= await js.InvokeAsync<IJSObjectReference>(
                "import", "./_content/SnapshotDiff.App/js/theme.js");

            var theme = config.Current.Appearance.Theme;
            var resolved = theme == "system"
                ? await _module.InvokeAsync<bool>("prefersLight") ? "light" : "dark"
                : theme;

            await _module.InvokeVoidAsync("applyTheme", resolved);
        }
        catch (JSDisconnectedException) { }
        catch (ObjectDisposedException) { }
    }

    /// <summary>
    /// Change theme, persist to config, and apply immediately.
    /// </summary>
    public async Task SetThemeAsync(string theme)
    {
        config.Current.Appearance.Theme = theme;
        await config.SaveAsync();
        await ApplyAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException) { }
            catch (ObjectDisposedException) { }
        }
    }
}
