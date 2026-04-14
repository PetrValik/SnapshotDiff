using Microsoft.AspNetCore.Components;

namespace SnapshotDiff.Shared.UI.Controls;

public partial class CustomSlider
{
    [Parameter]
    public int Value { get; set; }

    [Parameter]
    public int Min { get; set; } = 0;

    [Parameter]
    public int Max { get; set; } = 100;

    [Parameter]
    public int Step { get; set; } = 1;

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public EventCallback<int> ValueChanged { get; set; }

    [Parameter]
    public EventCallback<int> OnValueChange { get; set; }

    private async Task HandleInput(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var newValue))
        {
            Value = newValue;
            await ValueChanged.InvokeAsync(Value);
            await OnValueChange.InvokeAsync(Value);
        }
    }

    private double GetPercentage()
    {
        if (Max == Min) return 0;
        return ((double)(Value - Min) / (Max - Min)) * 100;
    }
}
