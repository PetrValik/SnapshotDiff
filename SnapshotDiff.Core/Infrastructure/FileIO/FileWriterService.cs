using Microsoft.Extensions.Logging;

namespace SnapshotDiff.Infrastructure.FileIO;

/// <summary>
/// Server-side file writer - writes files directly to disk
/// ⚠️ DEPRECATED for web applications - use BrowserFileWriter instead
/// Only suitable for desktop apps or unit tests
/// </summary>
[Obsolete("Use BrowserFileWriter for web applications. This implementation is only for desktop apps or tests.")]
public sealed class FileWriterService(ILogger<FileWriterService> logger) : IFileWriter
{
    private static readonly string DefaultOutputDirectory = EnsureDirectory(
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"));

    private static string EnsureDirectory(string path) { Directory.CreateDirectory(path); return path; }

    public async Task<(string FilePath, long FileSize)> WriteAsync(
        string fileName,
        Func<Stream, CancellationToken, Task> writeContent,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(writeContent);

        var filePath = Path.GetFullPath(Path.Combine(DefaultOutputDirectory, fileName));

        // Prevent path traversal — resolved path must stay within output directory
        if (!filePath.StartsWith(DefaultOutputDirectory, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException($"Path traversal detected: {fileName}");

        try
        {
            await using var fileStream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            await writeContent(fileStream, cancellationToken);
            await fileStream.FlushAsync(cancellationToken);

            var fileSize = fileStream.Length;

            logger.LogInformation(
                "File written to server: {FilePath} ({Size} bytes)",
                filePath,
                fileSize);

            return (filePath, fileSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write file: {FilePath}", filePath);
            throw;
        }
    }
}
