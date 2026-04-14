using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SnapshotDiff.Features.Config.Domain;
using SnapshotDiff.Features.Config.Infrastructure;
using SnapshotDiff.Features.ExclusionRules.Domain;
using SnapshotDiff.Features.ExclusionRules.Infrastructure;

namespace SnapshotDiff.Tests.Features.ExclusionRules;

public sealed class ExclusionServiceTests
{
    private readonly IDefaultExclusionProvider _defaults;
    private readonly IConfigService _config;
    private readonly ILogger<ExclusionService> _logger;
    private readonly AppConfig _appConfig;

    public ExclusionServiceTests()
    {
        _defaults = Substitute.For<IDefaultExclusionProvider>();
        _config = Substitute.For<IConfigService>();
        _logger = Substitute.For<ILogger<ExclusionService>>();
        _appConfig = new AppConfig();
        _config.Current.Returns(_appConfig);
    }

    private ExclusionService CreateSut() => new(_defaults, _config, _logger);

    // ── GetSystemRules ───────────────────────────────────────────────────────

    [Fact]
    public void GetSystemRules_DelegatesToProvider()
    {
        var rules = new List<ExclusionRule>
        {
            new() { Id = "sys1", Pattern = "*.sys", Type = ExclusionRuleType.System, Scope = ExclusionScope.Global, IsEnabled = true }
        };
        _defaults.GetSystemRules().Returns(rules);

        var sut = CreateSut();
        sut.GetSystemRules().Should().BeEquivalentTo(rules);
    }

    // ── GetGlobalUserRules ───────────────────────────────────────────────────

    [Fact]
    public void GetGlobalUserRules_MapsFromConfig()
    {
        _appConfig.GlobalExclusionPatterns.Add(new UserExclusionPattern
        {
            Pattern = "*.tmp",
            Description = "Temp files",
            IsEnabled = true
        });

        var sut = CreateSut();
        var rules = sut.GetGlobalUserRules();

        rules.Should().ContainSingle()
             .Which.Pattern.Should().Be("*.tmp");
        rules[0].Type.Should().Be(ExclusionRuleType.User);
        rules[0].Scope.Should().Be(ExclusionScope.Global);
    }

    // ── GetPerDirectoryPatterns ──────────────────────────────────────────────

    [Fact]
    public void GetPerDirectoryPatterns_ReturnsPatterns()
    {
        _appConfig.WatchedDirectories.Add(new WatchedDirectory
        {
            Path = @"C:\Data",
            ExclusionPatterns = ["*.log", "*.bak"]
        });

        var sut = CreateSut();
        sut.GetPerDirectoryPatterns(@"C:\Data").Should().BeEquivalentTo("*.log", "*.bak");
    }

    [Fact]
    public void GetPerDirectoryPatterns_UnknownDir_ReturnsEmpty()
    {
        var sut = CreateSut();
        sut.GetPerDirectoryPatterns(@"C:\Unknown").Should().BeEmpty();
    }

    // ── GetEvaluatorForScan ──────────────────────────────────────────────────

    [Fact]
    public void GetEvaluatorForScan_CombinesAllRuleSources()
    {
        _defaults.GetSystemRules().Returns(new List<ExclusionRule>
        {
            new() { Id = "sys1", Pattern = "*.sys", Type = ExclusionRuleType.System, Scope = ExclusionScope.Global, IsEnabled = true }
        });
        _appConfig.GlobalExclusionPatterns.Add(new UserExclusionPattern
        {
            Pattern = "*.tmp",
            IsEnabled = true
        });
        _appConfig.WatchedDirectories.Add(new WatchedDirectory
        {
            Path = @"C:\Data",
            ExclusionPatterns = ["debug.log"]
        });

        var sut = CreateSut();
        var evaluator = sut.GetEvaluatorForScan(@"C:\Data");

        evaluator.Should().NotBeNull();
        // System rule matches .sys files
        evaluator.IsExcluded(@"C:\Data\driver.sys", false).Should().BeTrue();
        // Global user rule matches .tmp files
        evaluator.IsExcluded(@"C:\Data\temp.tmp", false).Should().BeTrue();
        // Per-dir pattern matches exact name
        evaluator.IsExcluded(@"C:\Data\debug.log", false).Should().BeTrue();
        // Non-matching file is not excluded
        evaluator.IsExcluded(@"C:\Data\readme.txt", false).Should().BeFalse();
    }

    [Fact]
    public void GetEvaluatorForScan_DisabledGlobalRule_IsNotIncluded()
    {
        _defaults.GetSystemRules().Returns(new List<ExclusionRule>());
        _appConfig.GlobalExclusionPatterns.Add(new UserExclusionPattern
        {
            Pattern = "*.tmp",
            IsEnabled = false
        });

        var sut = CreateSut();
        var evaluator = sut.GetEvaluatorForScan(@"C:\Data");

        evaluator.IsExcluded(@"C:\Data\test.tmp", false).Should().BeFalse();
    }

    // ── AddGlobalRuleAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task AddGlobalRule_AddsAndSaves()
    {
        var sut = CreateSut();
        await sut.AddGlobalRuleAsync("*.log", "Log files");

        _appConfig.GlobalExclusionPatterns.Should().ContainSingle()
                  .Which.Pattern.Should().Be("*.log");
        await _config.Received(1).SaveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddGlobalRule_Duplicate_DoesNotAddTwice()
    {
        _appConfig.GlobalExclusionPatterns.Add(new UserExclusionPattern { Pattern = "*.log" });

        var sut = CreateSut();
        await sut.AddGlobalRuleAsync("*.log");

        _appConfig.GlobalExclusionPatterns.Should().HaveCount(1);
        await _config.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddGlobalRule_EmptyPattern_Throws()
    {
        var sut = CreateSut();
        await sut.Invoking(s => s.AddGlobalRuleAsync("  "))
                 .Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AddGlobalRule_TooManyWildcards_Throws()
    {
        var sut = CreateSut();
        var badPattern = string.Join("", Enumerable.Repeat("*a", 11));
        await sut.Invoking(s => s.AddGlobalRuleAsync(badPattern))
                 .Should().ThrowAsync<ArgumentException>()
                 .WithMessage("*wildcards*");
    }

    [Fact]
    public async Task AddGlobalRule_PatternTooLong_Throws()
    {
        var sut = CreateSut();
        var longPattern = new string('a', 261);
        await sut.Invoking(s => s.AddGlobalRuleAsync(longPattern))
                 .Should().ThrowAsync<ArgumentException>()
                 .WithMessage("*maximum length*");
    }

    // ── RemoveGlobalRuleAsync ────────────────────────────────────────────────

    [Fact]
    public async Task RemoveGlobalRule_RemovesById()
    {
        var pattern = new UserExclusionPattern { Pattern = "*.log" };
        _appConfig.GlobalExclusionPatterns.Add(pattern);

        var sut = CreateSut();
        await sut.RemoveGlobalRuleAsync(pattern.Id);

        _appConfig.GlobalExclusionPatterns.Should().BeEmpty();
        await _config.Received(1).SaveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveGlobalRule_NotFound_DoesNotSave()
    {
        var sut = CreateSut();
        await sut.RemoveGlobalRuleAsync("nonexistent");

        await _config.DidNotReceive().SaveAsync(Arg.Any<CancellationToken>());
    }

    // ── ToggleGlobalRuleAsync ────────────────────────────────────────────────

    [Fact]
    public async Task ToggleGlobalRule_TogglesEnabled()
    {
        var pattern = new UserExclusionPattern { Pattern = "*.log", IsEnabled = true };
        _appConfig.GlobalExclusionPatterns.Add(pattern);

        var sut = CreateSut();
        await sut.ToggleGlobalRuleAsync(pattern.Id, false);

        pattern.IsEnabled.Should().BeFalse();
        await _config.Received(1).SaveAsync(Arg.Any<CancellationToken>());
    }

    // ── Per-directory patterns ────────────────────────────────────────────────

    [Fact]
    public async Task AddPerDirectoryPattern_AddsViaConfigUpdate()
    {
        var sut = CreateSut();
        await sut.AddPerDirectoryPatternAsync(@"C:\Data", "*.bak");

        await _config.Received(1).UpdateWatchedDirectoryAsync(
            @"C:\Data", Arg.Any<Action<WatchedDirectory>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemovePerDirectoryPattern_RemovesViaConfigUpdate()
    {
        var sut = CreateSut();
        await sut.RemovePerDirectoryPatternAsync(@"C:\Data", "*.bak");

        await _config.Received(1).UpdateWatchedDirectoryAsync(
            @"C:\Data", Arg.Any<Action<WatchedDirectory>>(), Arg.Any<CancellationToken>());
    }
}
