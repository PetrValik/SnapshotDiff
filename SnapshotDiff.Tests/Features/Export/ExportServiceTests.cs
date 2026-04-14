using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SnapshotDiff.Features.Export.Application.Models;
using SnapshotDiff.Features.Export.Infrastructure;
using SnapshotDiff.Features.Scanner.Domain;
using SnapshotDiff.Infrastructure.FileIO;

namespace SnapshotDiff.Tests.Features.Export;

public sealed class ExportServiceTests
{
    private readonly IFileWriter _fileWriter;
    private readonly ExportService _sut;

    public ExportServiceTests()
    {
        _fileWriter = Substitute.For<IFileWriter>();
        var nameGen = Substitute.For<IFileNameGenerator>();
        nameGen.Generate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
               .Returns(ci => $"test.{ci.ArgAt<string>(2)}");

        var logger = Substitute.For<ILogger<ExportService>>();
        _sut = new ExportService(_fileWriter, nameGen, logger);
    }

    private static ScanEntry MakeEntry(
        string name = "file.txt",
        string ext = ".txt",
        long size = 1024,
        string fullPath = @"C:\test\file.txt") =>
        new()
        {
            FullPath = fullPath,
            RelativePath = name,
            Name = name,
            Size = size,
            LastWriteTime = new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero),
            Type = ScanEntryType.File,
            Extension = ext
        };

    private void SetupFileWriter(MemoryStream ms)
    {
        _fileWriter.WriteAsync(
            Arg.Any<string>(),
            Arg.Any<Func<Stream, CancellationToken, Task>>(),
            Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                var cb = ci.ArgAt<Func<Stream, CancellationToken, Task>>(1);
                await cb(ms, CancellationToken.None);
                return ("out.csv", ms.Length);
            });
    }

    // ── CSV Tests ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExportCsv_AllFieldsQuoted()
    {
        var ms = new MemoryStream();
        SetupFileWriter(ms);

        var entries = new List<ScanEntry> { MakeEntry() };
        var result = await _sut.ExportAsync(entries, ExportFormat.Csv);

        result.IsSuccess.Should().BeTrue();
        var csv = Encoding.UTF8.GetString(ms.ToArray());

        // Every data field (including Extension, Size, LastWriteTime, Type) should be quoted
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCountGreaterOrEqualTo(2, "header + at least 1 data row");

        var dataLine = lines[1];
        // All 6 fields should start with quote
        var fields = ParseCsvLine(dataLine);
        fields.Should().HaveCount(6);
        foreach (var field in fields)
        {
            field.Should().StartWith("\"").And.EndWith("\"",
                because: "all CSV fields must be quoted to prevent CSV injection");
        }
    }

    [Fact]
    public async Task ExportCsv_CsvInjection_EscapedProperly()
    {
        var ms = new MemoryStream();
        SetupFileWriter(ms);

        var malicious = MakeEntry(
            name: "=cmd|'/c calc'!A0",
            ext: "=HYPERLINK(\"evil\")",
            fullPath: @"C:\test\=cmd|'/c calc'!A0");

        var result = await _sut.ExportAsync([malicious], ExportFormat.Csv);

        result.IsSuccess.Should().BeTrue();
        var csv = Encoding.UTF8.GetString(ms.ToArray());

        // The malicious content should be inside quotes, not executable
        csv.Should().Contain("\"=cmd|'/c calc'!A0\"");
        csv.Should().Contain("\"=HYPERLINK(\"\"evil\"\")\"");
    }

    [Fact]
    public async Task ExportCsv_EmptyEntries_OnlyHeader()
    {
        var ms = new MemoryStream();
        SetupFileWriter(ms);

        var result = await _sut.ExportAsync([], ExportFormat.Csv);

        result.IsSuccess.Should().BeTrue();
        var csv = Encoding.UTF8.GetString(ms.ToArray()).TrimStart('\uFEFF'); // trim BOM
        csv.Trim().Should().Be("FullPath,Name,Extension,SizeBytes,LastWriteTimeUtc,Type");
    }

    // ── JSON Tests ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ExportJson_ProducesValidJson()
    {
        var ms = new MemoryStream();
        SetupFileWriter(ms);

        var entries = new List<ScanEntry> { MakeEntry(), MakeEntry(name: "other.log", ext: ".log") };
        var result = await _sut.ExportAsync(entries, ExportFormat.Json);

        result.IsSuccess.Should().BeTrue();
        var json = Encoding.UTF8.GetString(ms.ToArray());

        var doc = JsonDocument.Parse(json);
        doc.RootElement.GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task ExportAsync_WriterThrows_ReturnsFailure()
    {
        _fileWriter.WriteAsync(
            Arg.Any<string>(),
            Arg.Any<Func<Stream, CancellationToken, Task>>(),
            Arg.Any<CancellationToken>())
            .Throws(new IOException("disk full"));

        var result = await _sut.ExportAsync([MakeEntry()], ExportFormat.Csv);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disk full");
    }

    // ── Helpers ─────────────────────────────────────────────────────────────────

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
                current.Append(c);
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else if (c is not '\r' and not '\n')
            {
                current.Append(c);
            }
        }

        if (current.Length > 0) fields.Add(current.ToString());
        return fields;
    }
}
