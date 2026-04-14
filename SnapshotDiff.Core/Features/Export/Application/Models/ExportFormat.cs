namespace SnapshotDiff.Features.Export.Application.Models;

/// <summary>
/// Supported output file formats for data export.
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// Export as a structured JSON document.
    /// </summary>
    Json,

    /// <summary>
    /// Export as a comma-separated values (CSV) text file.
    /// </summary>
    Csv
}
