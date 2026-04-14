using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SnapshotDiff.Features.Export.Application;
using SnapshotDiff.Features.Export.Application.Models;
using SnapshotDiff.Features.Scanner.Domain;
using SnapshotDiff.Infrastructure.Common;
using SnapshotDiff.Infrastructure.FileIO;

namespace SnapshotDiff.Features.Export.Infrastructure;

public sealed class ExportService(
    IFileWriter fileWriter,
    IFileNameGenerator fileNameGenerator,
    ILogger<ExportService> logger) : IExportService
{

    public async Task<Result<ExportResult>> ExportAsync(
        IReadOnlyList<ScanEntry> entries,
        ExportFormat format,
        string? suggestedFileName = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entries);

        try
        {
            var ext = format == ExportFormat.Json ? "json" : "csv";
            var fileName = string.IsNullOrWhiteSpace(suggestedFileName)
                ? fileNameGenerator.Generate("scan", "export", ext)
                : suggestedFileName.EndsWith($".{ext}", StringComparison.OrdinalIgnoreCase)
                    ? suggestedFileName
                    : $"{suggestedFileName}.{ext}";

            var (filePath, fileSize) = await fileWriter.WriteAsync(
                fileName,
                async (stream, innerCt) =>
                {
                    if (format == ExportFormat.Json)
                        await WriteJsonAsync(entries, stream, innerCt);
                    else
                        await WriteCsvAsync(entries, stream, innerCt);
                },
                ct);

            return Result<ExportResult>.Success(new ExportResult
            {
                FilePath = filePath,
                FileSize = fileSize,
                ExportedAt = DateTime.UtcNow,
                Format = format,
                EntryCount = entries.Count
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Export failed");
            return Result<ExportResult>.Failure($"Export error: {ex.Message}");
        }
    }

    private static async Task WriteJsonAsync(
        IReadOnlyList<ScanEntry> entries,
        Stream output,
        CancellationToken ct)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var records = entries.Select(e => new
        {
            e.FullPath,
            e.RelativePath,
            e.Name,
            e.Extension,
            SizeBytes = e.Size,
            LastWriteTimeUtc = e.LastWriteTime,
            Type = e.Type.ToString()
        });

        await JsonSerializer.SerializeAsync(output, records, options, ct);
    }

    private static async Task WriteCsvAsync(
        IReadOnlyList<ScanEntry> entries,
        Stream output,
        CancellationToken ct)
    {
        await using var writer = new StreamWriter(output, Encoding.UTF8, leaveOpen: true);
        await writer.WriteLineAsync("FullPath,Name,Extension,SizeBytes,LastWriteTimeUtc,Type");

        foreach (var e in entries)
        {
            ct.ThrowIfCancellationRequested();
            await writer.WriteLineAsync(
                $"\"{e.FullPath.Replace("\"", "\"\"")}\"," +
                $"\"{e.Name.Replace("\"", "\"\"")}\"," +
                $"\"{e.Extension.Replace("\"", "\"\"")}\"," +
                $"\"{e.Size}\"," +
                $"\"{e.LastWriteTime:O}\"," +
                $"\"{e.Type}\"");
        }

        await writer.FlushAsync(ct);
    }
}
