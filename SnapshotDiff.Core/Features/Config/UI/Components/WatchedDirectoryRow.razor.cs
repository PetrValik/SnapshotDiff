using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SnapshotDiff.Features.Config.Domain;

namespace SnapshotDiff.Features.Config.UI.Components;

public partial class WatchedDirectoryRow : ComponentBase
{
    [Parameter, EditorRequired] public required WatchedDirectory Directory { get; set; }
    [Parameter] public EventCallback OnRemove { get; set; }
    [Parameter] public EventCallback<string> OnRename { get; set; }

    private bool _editing;
    private bool _showConfirm;
    private string _editLabel = string.Empty;

    private void StartEdit()
    {
        _editLabel = Directory.Label;
        _editing = true;
    }

    private async Task ConfirmRename()
    {
        _editing = false;
        await OnRename.InvokeAsync(_editLabel);
    }

    private void CancelEdit() => _editing = false;

    private async Task ConfirmRemove()
    {
        _showConfirm = false;
        await OnRemove.InvokeAsync();
    }

    private async Task OnLabelKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") await ConfirmRename();
        else if (e.Key == "Escape") CancelEdit();
    }
}
