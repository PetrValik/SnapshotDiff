using FluentAssertions;
using SnapshotDiff.Shared.Formatting;

namespace SnapshotDiff.Tests.Shared;

public class FileSizeFormatterTests
{
    // ── bytes ────────────────────────────────────────────────────────────────

    [Fact]
    public void Format_Zero_ReturnsZeroBytes()
    {
        FileSizeFormatter.Format(0).Should().Be("0 B");
    }

    [Fact]
    public void Format_LessThan1024_ReturnsBytes()
    {
        FileSizeFormatter.Format(512).Should().Be("512 B");
    }

    [Fact]
    public void Format_Exactly1023_ReturnsBytes()
    {
        FileSizeFormatter.Format(1023).Should().Be("1023 B");
    }

    // ── kilobytes ────────────────────────────────────────────────────────────

    [Fact]
    public void Format_Exactly1024_ReturnsOneKilobyte()
    {
        FileSizeFormatter.Format(1024).Should().Be("1 KB");
    }

    [Fact]
    public void Format_1536Bytes_Returns1Point5KB()
    {
        FileSizeFormatter.Format(1536).Should().Be("1.5 KB");
    }

    [Fact]
    public void Format_MaxKilobytes_ReturnsKB()
    {
        FileSizeFormatter.Format(1024 * 1023).Should().Contain("KB");
    }

    // ── megabytes ────────────────────────────────────────────────────────────

    [Fact]
    public void Format_Exactly1MB_ReturnsOneMegabyte()
    {
        FileSizeFormatter.Format(1024 * 1024).Should().Be("1 MB");
    }

    [Fact]
    public void Format_2MB_ReturnsTwoMegabytes()
    {
        FileSizeFormatter.Format(2 * 1024 * 1024).Should().Be("2 MB");
    }

    // ── gigabytes ────────────────────────────────────────────────────────────

    [Fact]
    public void Format_Exactly1GB_ReturnsOneGigabyte()
    {
        FileSizeFormatter.Format(1024L * 1024 * 1024).Should().Be("1 GB");
    }

    // ── terabytes ────────────────────────────────────────────────────────────

    [Fact]
    public void Format_Exactly1TB_ReturnsOneTerabyte()
    {
        FileSizeFormatter.Format(1024L * 1024 * 1024 * 1024).Should().Be("1 TB");
    }

    // ── Theory ───────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0, "B")]
    [InlineData(100, "B")]
    [InlineData(1024, "KB")]
    [InlineData(1048576, "MB")]
    [InlineData(1073741824L, "GB")]
    [InlineData(1099511627776L, "TB")]
    public void Format_Theory_ContainsCorrectUnit(long bytes, string expectedUnit)
    {
        FileSizeFormatter.Format(bytes).Should().Contain(expectedUnit);
    }

    [Theory]
    [InlineData(1024, "1 KB")]
    [InlineData(2048, "2 KB")]
    [InlineData(1048576, "1 MB")]
    [InlineData(1073741824L, "1 GB")]
    public void Format_Theory_ExactPowersOf1024(long bytes, string expected)
    {
        FileSizeFormatter.Format(bytes).Should().Be(expected);
    }
}
