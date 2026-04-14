using SnapshotDiff.Features.ExclusionRules.Infrastructure;

namespace SnapshotDiff.Features.ExclusionRules.Application.Commands;

/// <summary>
/// Adds a new user-defined global exclusion pattern that applies to all scans.
/// </summary>
/// <param name="Pattern">Glob pattern (e.g. <c>*.tmp</c>, <c>node_modules</c>).</param>
/// <param name="Description">Optional human-readable description of the rule.</param>
public sealed record AddGlobalRuleCommand(string Pattern, string Description = "");

/// <summary>
/// Handles <see cref="AddGlobalRuleCommand"/>.
/// </summary>
public sealed class AddGlobalRuleHandler(IExclusionService service)
{
    /// <summary>
    /// Validates and persists the new global exclusion pattern.
    /// </summary>
    public Task HandleAsync(AddGlobalRuleCommand cmd, CancellationToken ct = default) =>
        service.AddGlobalRuleAsync(cmd.Pattern, cmd.Description, ct);
}

/// <summary>
/// Removes an existing global exclusion rule by its ID.
/// </summary>
/// <param name="RuleId">Unique ID of the rule to remove.</param>
public sealed record RemoveGlobalRuleCommand(string RuleId);

/// <summary>
/// Handles <see cref="RemoveGlobalRuleCommand"/>.
/// </summary>
public sealed class RemoveGlobalRuleHandler(IExclusionService service)
{
    /// <summary>
    /// Removes the global exclusion rule identified by <see cref="RemoveGlobalRuleCommand.RuleId"/>.
    /// </summary>
    public Task HandleAsync(RemoveGlobalRuleCommand cmd, CancellationToken ct = default) =>
        service.RemoveGlobalRuleAsync(cmd.RuleId, ct);
}

/// <summary>
/// Enables or disables a global exclusion rule without removing it.
/// </summary>
/// <param name="RuleId">ID of the rule to toggle.</param>
/// <param name="IsEnabled">The new enabled state.</param>
public sealed record ToggleGlobalRuleCommand(string RuleId, bool IsEnabled);

/// <summary>
/// Handles <see cref="ToggleGlobalRuleCommand"/>.
/// </summary>
public sealed class ToggleGlobalRuleHandler(IExclusionService service)
{
    /// <summary>
    /// Persists the new enabled state for the specified global rule.
    /// </summary>
    public Task HandleAsync(ToggleGlobalRuleCommand cmd, CancellationToken ct = default) =>
        service.ToggleGlobalRuleAsync(cmd.RuleId, cmd.IsEnabled, ct);
}

/// <summary>
/// Adds an exclusion pattern scoped to a specific watched directory.
/// </summary>
/// <param name="DirectoryPath">Absolute path of the watched directory.</param>
/// <param name="Pattern">Glob pattern to exclude within that directory.</param>
public sealed record AddPerDirectoryPatternCommand(string DirectoryPath, string Pattern);

/// <summary>
/// Handles <see cref="AddPerDirectoryPatternCommand"/>.
/// </summary>
public sealed class AddPerDirectoryPatternHandler(IExclusionService service)
{
    /// <summary>
    /// Adds the per-directory pattern and persists the updated config.
    /// </summary>
    public Task HandleAsync(AddPerDirectoryPatternCommand cmd, CancellationToken ct = default) =>
        service.AddPerDirectoryPatternAsync(cmd.DirectoryPath, cmd.Pattern, ct);
}

/// <summary>
/// Removes a per-directory exclusion pattern.
/// </summary>
/// <param name="DirectoryPath">Absolute path of the watched directory.</param>
/// <param name="Pattern">Pattern to remove.</param>
public sealed record RemovePerDirectoryPatternCommand(string DirectoryPath, string Pattern);

/// <summary>
/// Handles <see cref="RemovePerDirectoryPatternCommand"/>.
/// </summary>
public sealed class RemovePerDirectoryPatternHandler(IExclusionService service)
{
    /// <summary>
    /// Removes the per-directory pattern and persists the updated config.
    /// </summary>
    public Task HandleAsync(RemovePerDirectoryPatternCommand cmd, CancellationToken ct = default) =>
        service.RemovePerDirectoryPatternAsync(cmd.DirectoryPath, cmd.Pattern, ct);
}
