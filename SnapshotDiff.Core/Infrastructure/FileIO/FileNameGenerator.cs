namespace SnapshotDiff.Infrastructure.FileIO;

/// <summary>
/// Default implementation of file name generator
/// </summary>
public sealed class FileNameGenerator : IFileNameGenerator
{
    public FileNameGenerator() { }

    public string Generate(string prefix, string identifier, string extension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(extension);
        
        var sanitizedIdentifier = string.IsNullOrWhiteSpace(identifier) 
            ? string.Empty 
            : $"_{SanitizeFileName(identifier)}";
        
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        return $"{prefix}{sanitizedIdentifier}_{timestamp}.{extension}";
    }

    private static string SanitizeFileName(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        ReadOnlySpan<char> invalidChars = Path.GetInvalidFileNameChars();
        Span<char> result = stackalloc char[input.Length];
        var index = 0;

        foreach (var c in input.AsSpan())
        {
            result[index++] = invalidChars.Contains(c) ? '_' : c;
        }

        return new string(result[..index]);
    }
}
