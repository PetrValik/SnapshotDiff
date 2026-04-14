using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace SnapshotDiff.Infrastructure.FileIO;

/// <summary>
/// Browser-based file writer using JavaScript Interop
/// Uses File System Access API when available, falls back to standard download
/// Suitable for files up to ~50MB
/// </summary>
public sealed class BrowserFileWriter(IJSRuntime jsRuntime, ILogger<BrowserFileWriter> logger) : IFileWriter
{

    public async Task<(string FilePath, long FileSize)> WriteAsync(
        string fileName,
        Func<Stream, CancellationToken, Task> writeContent,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(writeContent);

        const long MaxFileSizeBytes = 52_428_800; // 50 MB

        try
        {
            // Write content to MemoryStream (only during download preparation)
            await using var memoryStream = new MemoryStream();
            await writeContent(memoryStream, cancellationToken);

            if (memoryStream.Length > MaxFileSizeBytes)
                throw new InvalidOperationException(
                    $"File size ({memoryStream.Length:N0} bytes) exceeds the 50 MB browser download limit.");

            var fileSize = memoryStream.Length;
            memoryStream.Position = 0;

            // Convert to byte array for JS Interop
            var bytes = memoryStream.ToArray();

            // Trigger browser download
            // - Chrome/Edge: Shows "Save as" dialog (File System Access API)
            // - Safari/Firefox: Auto-downloads to default Downloads folder
            await jsRuntime.InvokeVoidAsync(
                "SnapshotDiff.downloadFile",
                cancellationToken,
                fileName,
                bytes);

            logger.LogInformation(
                "Browser download triggered: {FileName} ({Size} bytes)",
                fileName,
                fileSize);

            // Return virtual path (browser handles actual storage)
            return (fileName, fileSize);
        }
        catch (JSException ex)
        {
            logger.LogError(ex, "JavaScript interop failed for file download: {FileName}", fileName);
            throw new InvalidOperationException($"Failed to trigger download: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to prepare file for download: {FileName}", fileName);
            throw;
        }
    }
}
