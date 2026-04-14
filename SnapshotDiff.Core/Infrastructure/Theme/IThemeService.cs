namespace SnapshotDiff.Infrastructure.Theme;

/// <summary>
/// Applies and persists the colour theme for the application.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Apply the theme stored in config. Call once after Blazor renders.
    /// </summary>
    Task ApplyAsync();

    /// <summary>
    /// Change theme, persist to config, and apply immediately.
    /// </summary>
    Task SetThemeAsync(string theme);
}
