using Microsoft.AspNetCore.Components;

namespace SnapshotDiff.Shared.UI.Display;

public partial class VersionChange
{
    [Parameter]
    public int? PreviousVersion { get; set; }

    [Parameter]
    public int CurrentVersion { get; set; }

    private bool HasChange => PreviousVersion.HasValue && PreviousVersion.Value != CurrentVersion;
}
