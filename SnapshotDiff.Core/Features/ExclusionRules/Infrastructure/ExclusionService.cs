using Microsoft.Extensions.Logging;
using SnapshotDiff.Features.Config.Domain;
using SnapshotDiff.Features.Config.Infrastructure;
using SnapshotDiff.Features.ExclusionRules.Domain;

namespace SnapshotDiff.Features.ExclusionRules.Infrastructure;

/// <summary>
/// Manages all exclusion rules (system, global user-defined, and per-directory).
/// <para>
/// Reads and writes rules via <see cref="IConfigService"/>; does not hold its own state –
/// the config is the single source of truth.
/// </para>
/// Pattern validation enforces a maximum length of 260 characters and at most 10 wildcards
/// to protect against ReDoS-prone patterns being stored.
/// </summary>
public sealed class ExclusionService(
    IDefaultExclusionProvider defaults,
    IConfigService config,
    ILogger<ExclusionService> logger) : IExclusionService
{

    // ── Reads ──────────────────────────────────────────────────────────────────

    public IReadOnlyList<ExclusionRule> GetSystemRules() =>
        defaults.GetSystemRules();

    public IReadOnlyList<ExclusionRule> GetGlobalUserRules() =>
        config.Current.GlobalExclusionPatterns
               .Select(ToRule)
               .ToList();

    public IReadOnlyList<string> GetPerDirectoryPatterns(string directoryPath)
    {
        var dir = FindWatchedDir(directoryPath);
        return dir?.ExclusionPatterns ?? [];
    }

    public IExclusionEvaluator GetEvaluatorForScan(string watchedDirectoryPath)
    {
        var rules = new List<ExclusionRule>(defaults.GetSystemRules());

        foreach (var p in config.Current.GlobalExclusionPatterns)
            if (p.IsEnabled) rules.Add(ToRule(p));

        var dir = FindWatchedDir(watchedDirectoryPath);
        if (dir is not null)
        {
            foreach (var pattern in dir.ExclusionPatterns)
                rules.Add(new ExclusionRule
                {
                    Id = $"perdir-{pattern}",
                    Pattern = pattern,
                    Type = ExclusionRuleType.User,
                    Scope = ExclusionScope.PerDirectory,
                    Description = string.Empty,
                    IsEnabled = true
                });
        }

        logger.LogDebug("ExclusionEvaluator built with {Count} rules for {Dir}",
                         rules.Count, watchedDirectoryPath);
        return new ExclusionEvaluator(rules);
    }

    // ── Global rule mutations ──────────────────────────────────────────────────

    public async Task AddGlobalRuleAsync(string pattern, string description = "", CancellationToken ct = default)
    {
        pattern = pattern.Trim();
        ValidatePattern(pattern);

        if (config.Current.GlobalExclusionPatterns.Any(
            p => string.Equals(p.Pattern, pattern, StringComparison.OrdinalIgnoreCase)))
        {
            logger.LogDebug("Global exclusion pattern already exists: {Pattern}", pattern);
            return;
        }

        config.Current.GlobalExclusionPatterns.Add(new UserExclusionPattern
        {
            Pattern = pattern,
            Description = description,
            IsEnabled = true
        });
        await config.SaveAsync(ct);
    }

    public async Task RemoveGlobalRuleAsync(string ruleId, CancellationToken ct = default)
    {
        var removed = config.Current.GlobalExclusionPatterns.RemoveAll(p => p.Id == ruleId);
        if (removed > 0) await config.SaveAsync(ct);
    }

    public async Task ToggleGlobalRuleAsync(string ruleId, bool isEnabled, CancellationToken ct = default)
    {
        var rule = config.Current.GlobalExclusionPatterns.FirstOrDefault(p => p.Id == ruleId);
        if (rule is null) return;
        rule.IsEnabled = isEnabled;
        await config.SaveAsync(ct);
    }

    // ── Per-directory pattern mutations ───────────────────────────────────────

    public async Task AddPerDirectoryPatternAsync(string directoryPath, string pattern, CancellationToken ct = default)
    {
        pattern = pattern.Trim();
        ValidatePattern(pattern);
        await config.UpdateWatchedDirectoryAsync(directoryPath, dir =>
        {
            if (!dir.ExclusionPatterns.Contains(pattern, StringComparer.OrdinalIgnoreCase))
                dir.ExclusionPatterns.Add(pattern);
        }, ct);
    }

    public async Task RemovePerDirectoryPatternAsync(string directoryPath, string pattern, CancellationToken ct = default)
    {
        await config.UpdateWatchedDirectoryAsync(directoryPath, dir =>
            dir.ExclusionPatterns.RemoveAll(p =>
                string.Equals(p, pattern, StringComparison.OrdinalIgnoreCase)), ct);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private WatchedDirectory? FindWatchedDir(string path) =>
        config.Current.WatchedDirectories.FirstOrDefault(
            d => string.Equals(d.Path, path, StringComparison.OrdinalIgnoreCase));

    private static ExclusionRule ToRule(UserExclusionPattern p) => new()
    {
        Id = p.Id,
        Pattern = p.Pattern,
        Type = ExclusionRuleType.User,
        Scope = ExclusionScope.Global,
        Description = p.Description,
        IsEnabled = p.IsEnabled
    };

    private const int MaxPatternLength = 260;
    private const int MaxWildcards = 10;

    private static void ValidatePattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Pattern must not be empty.", nameof(pattern));

        if (pattern.Length > MaxPatternLength)
            throw new ArgumentException($"Pattern exceeds maximum length of {MaxPatternLength} characters.", nameof(pattern));

        var wildcardCount = pattern.Count(c => c is '*' or '?');
        if (wildcardCount > MaxWildcards)
            throw new ArgumentException($"Pattern contains too many wildcards ({wildcardCount}). Maximum is {MaxWildcards}.", nameof(pattern));
    }
}
