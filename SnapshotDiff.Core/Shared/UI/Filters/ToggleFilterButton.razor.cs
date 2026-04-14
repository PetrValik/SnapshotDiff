using Microsoft.AspNetCore.Components;

namespace SnapshotDiff.Shared.UI.Filters;

public partial class ToggleFilterButton
{
    [Parameter, EditorRequired]
    public string Label { get; set; } = string.Empty;

    [Parameter]
    public int Count { get; set; }

    [Parameter]
    public bool IsActive { get; set; }

    [Parameter]
    public string ColorClass { get; set; } = "default";

    [Parameter]
    public EventCallback<bool> OnToggle { get; set; }

    private async Task HandleClick()
    {
        await OnToggle.InvokeAsync(!IsActive);
    }
}
