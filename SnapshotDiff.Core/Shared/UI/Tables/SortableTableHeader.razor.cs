using Microsoft.AspNetCore.Components;

namespace SnapshotDiff.Shared.UI.Tables;

public partial class SortableTableHeader
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public SortDirection SortDirection { get; set; } = SortDirection.None;

    [Parameter]
    public EventCallback OnSort { get; set; }

    private async Task OnClick()
    {
        await OnSort.InvokeAsync();
    }

    private string GetSortIcon() => SortDirection switch
    {
        SortDirection.Ascending => "↑",
        SortDirection.Descending => "↓",
        _ => ""
    };
}
