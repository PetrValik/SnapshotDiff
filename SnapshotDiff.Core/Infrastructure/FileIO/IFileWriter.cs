namespace SnapshotDiff.Infrastructure.FileIO;

/// <summary>
/// Abstraction for writing files with true streaming support
/// Uses callback pattern to avoid intermediate buffering
/// </summary>
public interface IFileWriter
{
    /// <summary>
    /// Writes content to a file using a callback for direct streaming
    /// The callback receives a file stream and should write content directly to it
    /// </summary>
    /// <param name="fileName">The name of the file to create</param>
    /// <param name="writeContent">Callback that writes content to the provided stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of (FilePath, FileSize)</returns>
    Task<(string FilePath, long FileSize)> WriteAsync(
        string fileName,
        Func<Stream, CancellationToken, Task> writeContent,
        CancellationToken cancellationToken = default);
}
