namespace SnapshotDiff.Features.Config.Domain;

/// <summary>
/// UI appearance settings.
/// </summary>
public sealed class AppearanceConfig
{
    /// <summary>
    /// Color theme. Values: "light", "dark", "system".
    /// </summary>
    public string Theme { get; set; } = "system";

    /// <summary>
    /// UI language. Values: "en", "cs".
    /// </summary>
    public string Language { get; set; } = "en";
}
