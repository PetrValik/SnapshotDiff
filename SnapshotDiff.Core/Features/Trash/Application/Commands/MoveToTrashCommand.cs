namespace SnapshotDiff.Features.Trash.Application.Commands;

/// <summary>
/// Moves a file or directory to the application trash for safe, reversible deletion.
/// </summary>
/// <param name="FullPath">Absolute path of the file or directory to trash.</param>
public record MoveToTrashCommand(string FullPath);
