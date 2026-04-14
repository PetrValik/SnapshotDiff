namespace SnapshotDiff.Shared.Formatting;

internal static class FileSizeFormatter
{
    public static string Format(long bytes)
    {
        ReadOnlySpan<string> sizes = ["B", "KB", "MB", "GB", "TB"];

        var order = 0;
        var len = (double)bytes;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}
