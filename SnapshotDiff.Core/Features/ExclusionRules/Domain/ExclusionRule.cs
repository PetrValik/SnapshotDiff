namespace SnapshotDiff.Features.ExclusionRules.Domain;

/// <summary>
/// A single exclusion rule that tells the scanner to skip matching files or directories.
/// </summary>
public sealed record ExclusionRule
{
    /// <summary>
    /// Unique identifier for this rule (GUID string for user rules, fixed key for system rules).
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Glob pattern matched against file/directory name, or absolute path prefix.
    /// Supports * and ? wildcards. Examples: "$RECYCLE.BIN", "*.tmp", "NTUSER.DAT*".
    /// </summary>
    public required string Pattern { get; init; }

    /// <summary>
    /// System rules ship with the app and cannot be deleted.
    /// </summary>
    public required ExclusionRuleType Type { get; init; }

    /// <summary>
    /// Whether this rule applies globally or only to a specific directory.
    /// </summary>
    public required ExclusionScope Scope { get; init; }

    /// <summary>
    /// Human-readable explanation shown in the exclusion rules UI.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Disabled rules are stored but not evaluated during scans.
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// When true the pattern is only applied to directories, not files.
    /// </summary>
    public bool IsDirectoryOnly { get; init; } = false;
}

public enum ExclusionRuleType
{
    /// <summary>
    /// Shipped with the app, read-only, platform-specific.
    /// </summary>
    System,
    /// <summary>
    /// Added by the user.
    /// </summary>
    User
}

/// <summary>
/// Defines the scope over which an exclusion rule is applied.
/// </summary>
public enum ExclusionScope
{
    /// <summary>
    /// The rule is evaluated for every scanned directory.
    /// </summary>
    Global,
    /// <summary>
    /// The rule is evaluated only for the specific watched directory it belongs to.
    /// </summary>
    PerDirectory
}
