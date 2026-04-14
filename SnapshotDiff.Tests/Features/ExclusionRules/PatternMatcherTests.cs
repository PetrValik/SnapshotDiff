using FluentAssertions;
using SnapshotDiff.Features.ExclusionRules.Infrastructure;

namespace SnapshotDiff.Tests.Features.ExclusionRules;

public class PatternMatcherTests
{
    // ── Exact match ──────────────────────────────────────────────────────────

    [Fact]
    public void ExactMatch_NameMatchesPattern_ReturnsTrue()
    {
        PatternMatcher.Matches("node_modules", "node_modules", @"C:\project\node_modules")
            .Should().BeTrue();
    }

    [Fact]
    public void ExactMatch_NameDoesNotMatch_ReturnsFalse()
    {
        PatternMatcher.Matches("node_modules", "node_modules_old", @"C:\project\node_modules_old")
            .Should().BeFalse();
    }

    [Fact]
    public void ExactMatch_IsCaseInsensitive()
    {
        PatternMatcher.Matches("NODE_MODULES", "node_modules", @"C:\project\node_modules")
            .Should().BeTrue();
    }

    // ── Wildcard * (within segment) ──────────────────────────────────────────

    [Fact]
    public void StarWildcard_MatchesAnySequence_ReturnsTrue()
    {
        PatternMatcher.Matches("*.tmp", "somefile.tmp", @"C:\temp\somefile.tmp")
            .Should().BeTrue();
    }

    [Fact]
    public void StarWildcard_MatchesEmptySequence_ReturnsTrue()
    {
        PatternMatcher.Matches("*.tmp", ".tmp", @"C:\temp\.tmp")
            .Should().BeTrue();
    }

    [Fact]
    public void StarWildcard_DoesNotMatchDifferentExtension_ReturnsFalse()
    {
        PatternMatcher.Matches("*.tmp", "file.txt", @"C:\temp\file.txt")
            .Should().BeFalse();
    }

    [Fact]
    public void StarWildcard_PrefixPattern_Matches()
    {
        PatternMatcher.Matches("log*", "log_2024.txt", @"C:\logs\log_2024.txt")
            .Should().BeTrue();
    }

    [Fact]
    public void StarWildcard_MiddlePattern_Matches()
    {
        PatternMatcher.Matches("error*log", "error_app_log", @"C:\logs\error_app_log")
            .Should().BeTrue();
    }

    // ── Wildcard ** (cross-segment) ──────────────────────────────────────────

    [Fact]
    public void DoubleStarWildcard_MatchesMultipleSegments_ReturnsTrue()
    {
        // Pattern with leading * to match the drive/root prefix before src
        PatternMatcher.Matches(@"*\src\**\*.cs", "Program.cs", @"C:\project\src\features\ui\Program.cs")
            .Should().BeTrue();
    }

    [Fact]
    public void DoubleStarWildcard_AtEnd_MatchesAnything()
    {
        // Pattern with leading * to match drive prefix before project
        PatternMatcher.Matches(@"*\project\**", "file.txt", @"C:\project\deep\nested\file.txt")
            .Should().BeTrue();
    }

    // ── Wildcard ? ───────────────────────────────────────────────────────────

    [Fact]
    public void QuestionMarkWildcard_MatchesSingleChar_ReturnsTrue()
    {
        PatternMatcher.Matches("file?.txt", "file1.txt", @"C:\dir\file1.txt")
            .Should().BeTrue();
    }

    [Fact]
    public void QuestionMarkWildcard_DoesNotMatchMultipleChars_ReturnsFalse()
    {
        PatternMatcher.Matches("file?.txt", "file12.txt", @"C:\dir\file12.txt")
            .Should().BeFalse();
    }

    [Fact]
    public void QuestionMarkWildcard_DoesNotMatchZeroChars_ReturnsFalse()
    {
        PatternMatcher.Matches("file?.txt", "file.txt", @"C:\dir\file.txt")
            .Should().BeFalse();
    }

    // ── Absolute path ────────────────────────────────────────────────────────

    [Fact]
    public void AbsolutePath_MatchesFullPathPrefix_ReturnsTrue()
    {
        PatternMatcher.Matches(@"C:\Windows\System32", "ntdll.dll", @"C:\Windows\System32\ntdll.dll")
            .Should().BeTrue();
    }

    [Fact]
    public void AbsolutePath_DifferentPath_ReturnsFalse()
    {
        PatternMatcher.Matches(@"C:\Windows\System32", "explorer.exe", @"C:\Windows\explorer.exe")
            .Should().BeFalse();
    }

    // ── Non-matching ─────────────────────────────────────────────────────────

    [Fact]
    public void EmptyPattern_ReturnsFalse()
    {
        PatternMatcher.Matches("", "file.txt", @"C:\dir\file.txt")
            .Should().BeFalse();
    }

    [Fact]
    public void WhitespacePattern_ReturnsFalse()
    {
        PatternMatcher.Matches("   ", "file.txt", @"C:\dir\file.txt")
            .Should().BeFalse();
    }

    [Fact]
    public void NoWildcard_PartialNameDoesNotMatch_ReturnsFalse()
    {
        PatternMatcher.Matches("file", "filename.txt", @"C:\dir\filename.txt")
            .Should().BeFalse();
    }

    // ── Path-segment patterns ────────────────────────────────────────────────

    [Fact]
    public void PathSegmentPattern_MatchesFullPath()
    {
        // Use a leading * to match the drive/root prefix before dist
        PatternMatcher.Matches(@"*\dist\*.js", "bundle.js", @"C:\project\dist\bundle.js")
            .Should().BeTrue();
    }

    [Fact]
    public void PathSegmentPattern_DoesNotMatchWhenSegmentDiffers()
    {
        PatternMatcher.Matches(@"*\dist\*.js", "bundle.js", @"C:\project\build\bundle.js")
            .Should().BeFalse();
    }

    // ── Theory: multiple wildcard patterns ───────────────────────────────────

    [Theory]
    [InlineData("*.log", "app.log")]
    [InlineData("*.log", "error.log")]
    [InlineData("temp*", "temp_file")]
    [InlineData("temp*", "temporary")]
    [InlineData("*cache*", "localcache")]
    [InlineData("*cache*", "cache_v2")]
    public void StarWildcard_Theory_MatchesExpected(string pattern, string name)
    {
        PatternMatcher.Matches(pattern, name, $@"C:\dir\{name}")
            .Should().BeTrue();
    }
}
