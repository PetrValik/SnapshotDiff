using Microsoft.AspNetCore.Components;
using SnapshotDiff.Features.Config.Domain;
using SnapshotDiff.Infrastructure.Theme;

namespace SnapshotDiff.Features.Config.UI.Components;

public partial class AppearanceForm : ComponentBase
{
    [Parameter, EditorRequired] public required AppearanceConfig Config { get; set; }
    [Parameter] public EventCallback<AppearanceConfig> OnSave { get; set; }
    [Inject] private IThemeService ThemeService { get; set; } = default!;

    private string _theme = "system";

    protected override void OnParametersSet() => _theme = Config.Theme;

    private async Task OnThemeChangedAsync()
    {
        await ThemeService.SetThemeAsync(_theme);
        await OnSave.InvokeAsync(new AppearanceConfig { Theme = _theme, Language = Config.Language });
    }
}
