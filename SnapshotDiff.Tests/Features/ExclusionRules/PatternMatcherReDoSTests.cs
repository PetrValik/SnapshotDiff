using FluentAssertions;
using SnapshotDiff.Features.ExclusionRules.Infrastructure;

namespace SnapshotDiff.Tests.Features.ExclusionRules;

/// <summary>
/// Regression tests ensuring the iterative wildcard matcher doesn't exhibit
/// exponential backtracking (ReDoS) on pathological patterns.
/// </summary>
public sealed class PatternMatcherReDoSTests
{
    [Fact]
    public async Task PathologicalPattern_CompletesWithinTimeout()
    {
        var pattern = "a*b*c*d*e*f*g*h*i*j*zzz";
        var input = new string('a', 100);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var result = await Task.Run(() => PatternMatcher.Matches(pattern, input, input), cts.Token);

        result.Should().BeFalse();
    }

    [Fact]
    public void ManyConsecutiveStars_TreatedAsSingle()
    {
        var pattern = "**********test";
        var result = PatternMatcher.Matches(pattern, "mytest", "mytest");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task LongInputWithStar_DoesNotHang()
    {
        var pattern = "*end";
        var input = new string('x', 10_000) + "end";

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var result = await Task.Run(() => PatternMatcher.Matches(pattern, input, input), cts.Token);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task LongInputNoMatch_DoesNotHang()
    {
        var pattern = "*end";
        var input = new string('x', 10_000);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        var result = await Task.Run(() => PatternMatcher.Matches(pattern, input, input), cts.Token);

        result.Should().BeFalse();
    }

    [Fact]
    public void ComplexPatternWithMultipleStars_CorrectResult()
    {
        var pattern = "*.tmp.*";
        PatternMatcher.Matches(pattern, "report.tmp.bak", "report.tmp.bak").Should().BeTrue();
        PatternMatcher.Matches(pattern, "report.tmp", "report.tmp").Should().BeFalse();
        PatternMatcher.Matches(pattern, ".tmp.x", ".tmp.x").Should().BeTrue();
    }
}
