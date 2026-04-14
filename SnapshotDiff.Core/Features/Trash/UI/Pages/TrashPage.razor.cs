using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SnapshotDiff.Features.Trash.Application.Commands;
using SnapshotDiff.Features.Trash.Application.Queries;
using SnapshotDiff.Features.Trash.Domain;
using SnapshotDiff.Infrastructure.Notifications;
using SnapshotDiff.Shared.Formatting;

namespace SnapshotDiff.Features.Trash.UI.Pages;

public partial class TrashPage
{
    [Inject] private GetTrashItemsHandler GetItemsHandler { get; set; } = default!;
    [Inject] private RestoreFromTrashHandler RestoreHandler { get; set; } = default!;
    [Inject] private DeletePermanentlyHandler DeleteHandler { get; set; } = default!;
    [Inject] private EmptyTrashHandler EmptyHandler { get; set; } = default!;
    [Inject] private INotificationService Notifications { get; set; } = default!;
    [Inject] private IStringLocalizer<TrashResources> Loc { get; set; } = default!;

    private IReadOnlyList<TrashItemMeta> _items = [];
    private bool _loading = true;
    private bool _showEmptyConfirm;
    private bool _showDeleteConfirm;
    private TrashItemMeta? _deleteTarget;

    protected override async Task OnInitializedAsync()
    {
        await LoadItemsAsync();
    }

    private async Task LoadItemsAsync()
    {
        _loading = true;
        _items = await GetItemsHandler.HandleAsync(new GetTrashItemsQuery());
        _loading = false;
    }

    private async Task RestoreItem(string id)
    {
        try
        {
            await RestoreHandler.HandleAsync(new RestoreFromTrashCommand(id));
            Notifications.ShowSuccess(Loc["Notify_Restored"]);
            await LoadItemsAsync();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Notifications.ShowError(ex.Message);
        }
    }

    private void AskDeletePermanently(TrashItemMeta item)
    {
        _deleteTarget = item;
        _showDeleteConfirm = true;
    }

    private async Task ConfirmDeletePermanently()
    {
        if (_deleteTarget is null) return;
        try
        {
            await DeleteHandler.HandleAsync(new DeletePermanentlyCommand(_deleteTarget.Id));
            Notifications.ShowSuccess(Loc["Notify_Deleted"]);
            await LoadItemsAsync();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Notifications.ShowError(ex.Message);
        }
        finally
        {
            _showDeleteConfirm = false;
            _deleteTarget = null;
        }
    }

    private void AskEmptyTrash()
    {
        _showEmptyConfirm = true;
    }

    private async Task ConfirmEmptyTrash()
    {
        try
        {
            await EmptyHandler.HandleAsync(new EmptyTrashCommand());
            Notifications.ShowSuccess(Loc["Notify_Emptied"]);
            await LoadItemsAsync();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Notifications.ShowError(ex.Message);
        }
        finally
        {
            _showEmptyConfirm = false;
        }
    }

    private static string FormatSize(long bytes) => FileSizeFormatter.Format(bytes);
}
