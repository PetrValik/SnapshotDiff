using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SnapshotDiff.Features.Config.Domain;
using SnapshotDiff.Features.Config.Infrastructure;
using SnapshotDiff.Features.ExclusionRules.Application.Commands;
using SnapshotDiff.Features.ExclusionRules.Application.Queries;
using SnapshotDiff.Features.ExclusionRules.Domain;
using SnapshotDiff.Infrastructure.Notifications;

namespace SnapshotDiff.Features.ExclusionRules.UI.Pages;

public partial class ExclusionRulesPage : ComponentBase
{
    [Inject] private IStringLocalizer<ExclusionResources> Loc { get; set; } = default!;
    [Inject] private GetExclusionRulesHandler QueryHandler { get; set; } = default!;
    [Inject] private AddGlobalRuleHandler AddGlobalHandler { get; set; } = default!;
    [Inject] private RemoveGlobalRuleHandler RemoveGlobalHandler { get; set; } = default!;
    [Inject] private ToggleGlobalRuleHandler ToggleGlobalHandler { get; set; } = default!;
    [Inject] private AddPerDirectoryPatternHandler AddPerDirHandler { get; set; } = default!;
    [Inject] private RemovePerDirectoryPatternHandler RemovePerDirHandler { get; set; } = default!;
    [Inject] private IConfigService Config { get; set; } = default!;
    [Inject] private INotificationService Notify { get; set; } = default!;

    private IReadOnlyList<ExclusionRule> _systemRules = [];
    private IReadOnlyList<ExclusionRule> _globalRules = [];
    private IReadOnlyList<string> _perDirPatterns = [];
    private List<WatchedDirectory> _watchedDirs = [];

    private string _newGlobalPattern = string.Empty;
    private string _newGlobalDescription = string.Empty;
    private string _newDirPattern = string.Empty;

    private string _selectedDirPath = string.Empty;

    protected override void OnInitialized()
    {
        _watchedDirs = Config.Current.WatchedDirectories;
        Refresh();
    }

    private void Refresh(string? dirPath = null)
    {
        var result = QueryHandler.Handle(new GetExclusionRulesQuery(dirPath ?? _selectedDirPath));
        _systemRules = result.SystemRules;
        _globalRules = result.GlobalUserRules;
        _perDirPatterns = result.PerDirectoryPatterns;
    }

    // Triggered when user picks a different directory from the dropdown
    private string SelectedDirPath
    {
        get => _selectedDirPath;
        set
        {
            _selectedDirPath = value;
            _newDirPattern = string.Empty;
            Refresh(value);
        }
    }

    private async Task AddGlobalRule()
    {
        if (string.IsNullOrWhiteSpace(_newGlobalPattern)) return;
        await AddGlobalHandler.HandleAsync(new AddGlobalRuleCommand(_newGlobalPattern.Trim(), _newGlobalDescription.Trim()));
        _newGlobalPattern = string.Empty;
        _newGlobalDescription = string.Empty;
        Refresh();
        Notify.ShowSuccess(Loc["Toast_RuleAdded"]);
    }

    private async Task RemoveGlobalRule(string ruleId)
    {
        await RemoveGlobalHandler.HandleAsync(new RemoveGlobalRuleCommand(ruleId));
        Refresh();
        Notify.ShowSuccess(Loc["Toast_RuleRemoved"]);
    }

    private async Task ToggleGlobal(string ruleId, bool enabled)
    {
        await ToggleGlobalHandler.HandleAsync(new ToggleGlobalRuleCommand(ruleId, enabled));
        Refresh();
    }

    private async Task AddDirPattern()
    {
        if (string.IsNullOrWhiteSpace(_newDirPattern) || string.IsNullOrWhiteSpace(_selectedDirPath)) return;
        await AddPerDirHandler.HandleAsync(new AddPerDirectoryPatternCommand(_selectedDirPath, _newDirPattern.Trim()));
        _newDirPattern = string.Empty;
        Refresh(_selectedDirPath);
        Notify.ShowSuccess(Loc["Toast_PatternAdded"]);
    }

    private async Task RemoveDirPattern(string pattern)
    {
        await RemovePerDirHandler.HandleAsync(new RemovePerDirectoryPatternCommand(_selectedDirPath, pattern));
        Refresh(_selectedDirPath);
        Notify.ShowSuccess(Loc["Toast_PatternRemoved"]);
    }

    /// <summary>
    /// Returns the localized description for a system rule if available,
    /// otherwise falls back to the English description from DefaultExclusionProvider.
    /// Resource key format: SysDesc_{rule.Id with hyphens replaced by underscores}
    /// </summary>
    private string GetLocalizedDescription(ExclusionRule rule)
    {
        var key = $"SysDesc_{rule.Id.Replace('-', '_')}";
        var localized = Loc[key];
        return localized.ResourceNotFound ? rule.Description : localized.Value;
    }
}
