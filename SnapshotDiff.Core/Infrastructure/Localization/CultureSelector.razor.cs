using System.Globalization;

namespace SnapshotDiff.Infrastructure.Localization;

public partial class CultureSelector
{
    private string _displayCulture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

    protected override void OnInitialized()
    {
        _displayCulture = CultureState.Culture;
        CultureState.OnChanged += HandleCultureChanged;
    }

    private async Task ToggleAsync()
    {
        var newCulture = _displayCulture == "cs" ? "en" : "cs";
        await CultureService.SetCultureAsync(newCulture);
    }

    private void HandleCultureChanged()
    {
        _displayCulture = CultureState.Culture;
        _ = InvokeAsync(StateHasChanged);
    }

    private string GetFullName() => _displayCulture switch
    {
        "cs" => "Čeština",
        "en" => "English",
        _ => _displayCulture
    };

    public void Dispose()
    {
        CultureState.OnChanged -= HandleCultureChanged;
    }
}