using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using SnapshotDiff.Infrastructure.Storage;

namespace SnapshotDiff.Features.Config.UI.Components;

public partial class AddDirectoryForm : ComponentBase
{
    [Parameter] public EventCallback<string> OnAdd { get; set; }
    [Inject] private IFolderPickerService FolderPicker { get; set; } = default!;
    [Inject] private IStringLocalizer<SettingsResources> Loc { get; set; } = default!;

    private string _path = string.Empty;
    private string? _error;

    private bool IsValid => !string.IsNullOrWhiteSpace(_path);

    private async Task Submit()
    {
        _error = null;

        var trimmed = _path.Trim();

        if (string.IsNullOrWhiteSpace(trimmed))
        {
            _error = Loc["AddDir_Error_Empty"];
            return;
        }

        if (!Directory.Exists(trimmed))
        {
            _error = Loc["AddDir_Error_NotFound"];
            return;
        }

        await OnAdd.InvokeAsync(trimmed);
        _path = string.Empty;
    }

    private async Task BrowseAsync()
    {
        var folder = await FolderPicker.PickFolderAsync();
        if (!string.IsNullOrEmpty(folder))
        {
            _path = folder;
            _error = null;
            StateHasChanged();
        }
    }

    private async Task OnKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && IsValid) await Submit();
    }
}
