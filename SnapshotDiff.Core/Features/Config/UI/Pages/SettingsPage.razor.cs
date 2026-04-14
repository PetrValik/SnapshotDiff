using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Localization;
using SnapshotDiff.Features.Config.Domain;
using SnapshotDiff.Features.Config.Infrastructure;
using SnapshotDiff.Features.Trash.Infrastructure;
using SnapshotDiff.Infrastructure.Notifications;

namespace SnapshotDiff.Features.Config.UI.Pages;

public partial class SettingsPage : ComponentBase, IDisposable
{
    [Inject] private IConfigService ConfigService { get; set; } = default!;
    [Inject] private IStringLocalizer<SettingsResources> Loc { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ITrashService TrashService { get; set; } = default!;
    [Inject] private INotificationService Notifications { get; set; } = default!;

    private bool _showResetDialog;
    private int _editStaleAfterDays;
    private int _editNewWithinDays;

    private bool FiltersChanged =>
        _editStaleAfterDays != ConfigService.Current.DefaultStaleAfterDays ||
        _editNewWithinDays != ConfigService.Current.DefaultNewWithinDays;

    protected override async Task OnInitializedAsync()
    {
        await ConfigService.LoadAsync();
        _editStaleAfterDays = ConfigService.Current.DefaultStaleAfterDays;
        _editNewWithinDays = ConfigService.Current.DefaultNewWithinDays;
        ConfigService.ConfigChanged += OnConfigChanged;
    }

    private void OnConfigChanged(object? sender, EventArgs e) => InvokeAsync(StateHasChanged);

    private async Task AddDirectory(string path)
    {
        await ConfigService.AddWatchedDirectoryAsync(path);
        StateHasChanged();
    }

    private async Task RemoveDirectory(string path)
    {
        await ConfigService.RemoveWatchedDirectoryAsync(path);
        StateHasChanged();
    }

    private async Task RenameDirectory(string path, string newLabel)
    {
        await ConfigService.UpdateWatchedDirectoryAsync(path, d => d.Label = newLabel);
        StateHasChanged();
    }

    private async Task SaveDefaultFiltersAsync()
    {
        ConfigService.Current.DefaultStaleAfterDays = _editStaleAfterDays;
        ConfigService.Current.DefaultNewWithinDays = _editNewWithinDays;
        await ConfigService.SaveAsync();
        Notifications.ShowSuccess(Loc["SettingsSaved"]);
        StateHasChanged();
    }

    private async Task SaveAppearance(AppearanceConfig appearance)
    {
        ConfigService.Current.Appearance = appearance;
        await ConfigService.SaveAsync();
        StateHasChanged();
    }

    private void ShowResetDialog() => _showResetDialog = true;
    private void HideResetDialog() => _showResetDialog = false;

    private async Task ResetApplicationAsync()
    {
        _showResetDialog = false;
        await TrashService.EmptyTrashAsync();
        await ConfigService.ResetAsync();
        NavigationManager.NavigateTo("/", forceLoad: true);
    }

    public void Dispose()
    {
        ConfigService.ConfigChanged -= OnConfigChanged;
    }
}