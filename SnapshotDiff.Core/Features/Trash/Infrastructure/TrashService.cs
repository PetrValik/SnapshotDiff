using Microsoft.Extensions.Logging;
using SnapshotDiff.Features.Trash.Domain;
using SnapshotDiff.Infrastructure.Storage;

namespace SnapshotDiff.Features.Trash.Infrastructure;

/// <summary>
/// Moves files and directories to an application-managed trash directory and tracks them
/// in a SQLite database via <see cref="ITrashRepository"/>.
/// <para>
/// <b>Atomicity guarantee:</b> the database record is inserted <em>before</em> the file is moved.
/// If the move fails the record is rolled back. This ensures that orphaned files (moved but not
/// tracked) cannot occur, while an orphaned record (tracked but file missing) is survivable.
/// </para>
/// <para>
/// <b>Retention:</b> items expire after 30 days and are purged by <see cref="TrashPurgeService"/>.
/// </para>
/// </summary>
public sealed class TrashService(
    IStoragePathProvider storagePathProvider,
    ITrashRepository repository,
    ILogger<TrashService> logger) : ITrashService
{
    private readonly string _filesDir =
        Path.Combine(storagePathProvider.AppDataDirectory, "trash", "files");
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(30);

    public async Task<string> MoveToTrashAsync(string fullPath, CancellationToken ct = default)
    {
        if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
            throw new FileNotFoundException($"Path not found: {fullPath}");

        Directory.CreateDirectory(_filesDir);

        var id = Guid.NewGuid().ToString();
        var target = Path.Combine(_filesDir, id);

        var isDirectory = Directory.Exists(fullPath);
        long sizeBytes = isDirectory ? GetDirectorySize(fullPath, logger) : new FileInfo(fullPath).Length;

        // Insert metadata FIRST — if the app crashes after move but before insert,
        // the file would be orphaned with no way to restore it.
        var meta = new TrashItemMeta
        {
            Id = id,
            OriginalPath = Path.GetFullPath(fullPath),
            Name = Path.GetFileName(fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)),
            DeletedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow + RetentionPeriod,
            IsDirectory = isDirectory,
            SizeBytes = sizeBytes,
        };

        await repository.InsertAsync(meta, ct);

        try
        {
            if (isDirectory)
                MoveDirectory(fullPath, target);
            else
                MoveFile(fullPath, target);
        }
        catch
        {
            // Rollback the DB record if the move failed
            await repository.DeleteAsync(id, ct);
            throw;
        }

        logger.LogInformation("Moved to trash: {Path} → id={Id}", fullPath, id);
        return id;
    }

    public async Task RestoreAsync(string id, CancellationToken ct = default)
    {
        var meta = await repository.GetAsync(id, ct)
            ?? throw new InvalidOperationException($"Trash item not found: {id}");

        // Validate that OriginalPath doesn't escape expected boundaries (path traversal guard).
        // Use case-sensitive comparison on Linux where the filesystem is case-sensitive.
        var resolvedPath = Path.GetFullPath(meta.OriginalPath);
        var pathComparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (!resolvedPath.Equals(meta.OriginalPath, pathComparison))
            throw new InvalidOperationException(
                $"Path traversal attempt detected: stored='{meta.OriginalPath}', resolved='{resolvedPath}'");

        var sourcePath = Path.Combine(_filesDir, id);
        if (!File.Exists(sourcePath) && !Directory.Exists(sourcePath))
            throw new FileNotFoundException($"Trash content missing for id={id}");

        var parentDir = Path.GetDirectoryName(resolvedPath);
        if (!string.IsNullOrEmpty(parentDir))
            Directory.CreateDirectory(parentDir);

        if (meta.IsDirectory)
            Directory.Move(sourcePath, resolvedPath);
        else
            File.Move(sourcePath, resolvedPath, overwrite: false);

        await repository.DeleteAsync(id, ct);
        logger.LogInformation("Restored from trash: id={Id} → {Path}", id, resolvedPath);
    }

    public async Task DeletePermanentlyAsync(string id, CancellationToken ct = default)
    {
        var contentPath = Path.Combine(_filesDir, id);
        try
        {
            if (Directory.Exists(contentPath))
                Directory.Delete(contentPath, recursive: true);
            else if (File.Exists(contentPath))
                File.Delete(contentPath);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to delete trash content for id={Id}", id);
        }

        await repository.DeleteAsync(id, ct);
        logger.LogInformation("Permanently deleted trash item id={Id}", id);
    }

    public async Task EmptyTrashAsync(CancellationToken ct = default)
    {
        var items = await repository.GetAllAsync(ct);
        foreach (var item in items)
        {
            ct.ThrowIfCancellationRequested();
            var contentPath = Path.Combine(_filesDir, item.Id);
            try
            {
                if (Directory.Exists(contentPath))
                    Directory.Delete(contentPath, recursive: true);
                else if (File.Exists(contentPath))
                    File.Delete(contentPath);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to delete trash content for id={Id}", item.Id);
            }
        }
        await repository.DeleteAllAsync(ct);
        logger.LogInformation("Trash emptied: {Count} items removed", items.Count);
    }

    public async Task PurgeExpiredAsync(CancellationToken ct = default)
    {
        var expired = await repository.GetExpiredAsync(DateTime.UtcNow, ct);
        foreach (var item in expired)
        {
            ct.ThrowIfCancellationRequested();
            await DeletePermanentlyAsync(item.Id, ct);
        }
        if (expired.Count > 0)
            logger.LogInformation("Purged {Count} expired trash items", expired.Count);
    }

    public async Task<IReadOnlyList<TrashItemMeta>> GetItemsAsync(CancellationToken ct = default)
        => await repository.GetAllAsync(ct);

    public async Task<TrashItemMeta?> GetItemAsync(string id, CancellationToken ct = default)
        => await repository.GetAsync(id, ct);

    private static long GetDirectorySize(string path, ILogger logger)
    {
        long total = 0;
        try
        {
            foreach (var file in new DirectoryInfo(path)
                         .EnumerateFiles("*", SearchOption.AllDirectories))
            {
                try { total += file.Length; }
                catch (Exception ex)
                {
                    logger.LogDebug(ex, "Skipping file in size calc: {File}", file.FullName);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Directory size calculation failed for: {Path}", path);
        }
        return total;
    }

    // ── Cross-volume move helpers ────────────────────────────────────────────────

    /// <summary>
    /// Moves a file, falling back to copy-then-delete when source and destination
    /// are on different volumes (e.g. scanning D:\ with AppData on C:\).
    /// </summary>
    private static void MoveFile(string source, string destination)
    {
        try
        {
            File.Move(source, destination);
        }
        catch (IOException)
        {
            // Cross-volume fallback: copy first, then delete source
            try
            {
                File.Copy(source, destination, overwrite: false);
                File.Delete(source);
            }
            catch
            {
                // Clean up a partial copy so the filesystem is left consistent
                try { if (File.Exists(destination)) File.Delete(destination); } catch { /* best effort */ }
                throw;
            }
        }
    }

    /// <summary>
    /// Moves a directory tree, falling back to recursive copy-then-delete across volumes.
    /// </summary>
    private static void MoveDirectory(string source, string destination)
    {
        try
        {
            Directory.Move(source, destination);
        }
        catch (IOException)
        {
            try
            {
                CopyDirectoryRecursive(source, destination);
                Directory.Delete(source, recursive: true);
            }
            catch
            {
                try { if (Directory.Exists(destination)) Directory.Delete(destination, recursive: true); } catch { /* best effort */ }
                throw;
            }
        }
    }

    private static void CopyDirectoryRecursive(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (var file in Directory.EnumerateFiles(source))
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)));
        foreach (var subDir in Directory.EnumerateDirectories(source))
            CopyDirectoryRecursive(subDir, Path.Combine(destination, Path.GetFileName(subDir)));
    }
}
