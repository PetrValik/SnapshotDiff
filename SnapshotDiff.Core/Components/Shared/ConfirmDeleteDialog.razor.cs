using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using SnapshotDiff.Components.Layout;

namespace SnapshotDiff.Components.Shared;

public partial class ConfirmDeleteDialog
{
    [Inject] private IStringLocalizer<LayoutResources> Loc { get; set; } = default!;

    /// <summary>
    /// Name (or path) of the file/item being deleted. Displayed in the dialog.
    /// </summary>
    [Parameter] public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// When true the dialog shows a system-protected warning and requires the user
    /// to type <see cref="ConfirmWord"/> before the delete button is enabled.
    /// </summary>
    [Parameter] public bool IsSystemProtected { get; set; }

    /// <summary>
    /// The word the user must type when <see cref="IsSystemProtected"/> is true.
    /// </summary>
    [Parameter] public string ConfirmWord { get; set; } = "DELETE";

    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback OnConfirm { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    private string _typedWord = string.Empty;

    private bool CanConfirm =>
        !IsSystemProtected ||
        string.Equals(_typedWord.Trim(), ConfirmWord, StringComparison.OrdinalIgnoreCase);

    private async Task Confirm()
    {
        if (!CanConfirm) return;
        _typedWord = string.Empty;
        await OnConfirm.InvokeAsync();
    }

    private async Task Cancel()
    {
        _typedWord = string.Empty;
        await OnCancel.InvokeAsync();
    }

    private async Task HandleBackdropClick() => await Cancel();

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && CanConfirm) await Confirm();
        if (e.Key == "Escape") await Cancel();
    }
}
