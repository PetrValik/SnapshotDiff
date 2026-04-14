using SnapshotDiff.Features.Config.Domain;

namespace SnapshotDiff.Features.Scanner.Application.Commands;

/// <summary>
/// Requests a full scan of the given directory, optionally using per-directory filter overrides.
/// </summary>
/// <param name="DirectoryPath">Absolute path to the root directory to scan.</param>
/// <param name="CustomFilter">Optional per-directory filter overrides (age, size, extensions). When <see langword="null"/> the global defaults are used.</param>
public sealed record ScanDirectoryCommand(
    string DirectoryPath,
    DirectoryCustomFilter? CustomFilter = null);
