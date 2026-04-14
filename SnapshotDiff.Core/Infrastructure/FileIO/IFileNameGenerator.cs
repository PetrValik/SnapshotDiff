namespace SnapshotDiff.Infrastructure.FileIO;

/// <summary>
/// Generates safe file names with timestamps and sanitization
/// </summary>
public interface IFileNameGenerator
{
    /// <summary>
    /// Generates a file name with timestamp
    /// </summary>
    /// <param name="prefix">File name prefix</param>
    /// <param name="identifier">Additional identifier (e.g., root path)</param>
    /// <param name="extension">File extension without dot</param>
    /// <returns>Safe file name</returns>
    string Generate(string prefix, string identifier, string extension);
}
