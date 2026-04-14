namespace SnapshotDiff.Features.Config.Domain;

/// <summary>
/// User-defined exclusion pattern stored in config.
/// </summary>
public sealed class UserExclusionPattern
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Pattern { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}
