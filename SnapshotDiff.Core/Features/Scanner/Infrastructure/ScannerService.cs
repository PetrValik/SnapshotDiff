using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SnapshotDiff.Features.Scanner.Domain;

namespace SnapshotDiff.Features.Scanner.Infrastructure;

/// <summary>
/// Two-phase parallel directory scanner.
/// <para>
/// <b>Phase 1 – Counting:</b> A quick single-threaded traversal counts the total number of
/// directories so the UI can show a deterministic progress percentage.
/// </para>
/// <para>
/// <b>Phase 2 – Scanning:</b> A parallel walk (up to 8 threads) collects file and directory
/// metadata into a thread-safe <see cref="ConcurrentBag{T}"/>. Progress is reported at most
/// once every 150 ms to avoid flooding the UI with updates.
/// </para>
/// Symlinks and junctions (<see cref="FileAttributes.ReparsePoint"/>) are silently skipped to
/// prevent infinite loops. Inaccessible paths are recorded in the result but do not abort the scan.
/// </summary>
public sealed class ScannerService(ILogger<ScannerService> logger) : IScannerService
{
    private const int ProgressIntervalMs = 150;

    public async Task<ScanResult> ScanAsync(
        ScanOptions options,
        IProgress<ScanProgress>? progress = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.RootPath);

        var sw = Stopwatch.StartNew();
        var state = new ScanState(sw);

        await Task.Run(() =>
        {
            // Phase 1: Count directories for deterministic progress bar
            var totalDirs = CountDirectories(options.RootPath, options, state, progress, ct);
            state.TotalDirectories = totalDirs;

            // Phase 2: Actual scan with progress percentage
            WalkDirectory(options.RootPath, options.RootPath, options, state, progress, ct);
        }, ct);

        sw.Stop();

        // Final progress report
        progress?.Report(new ScanProgress
        {
            ProcessedFiles = Volatile.Read(ref state.FileCount),
            ProcessedDirectories = Volatile.Read(ref state.DirectoryCount),
            TotalDirectories = state.TotalDirectories,
            CurrentDirectory = string.Empty,
            Phase = ScanPhase.Scanning,
            ElapsedSeconds = sw.Elapsed.TotalSeconds
        });

        return new ScanResult
        {
            RootPath = options.RootPath,
            ScannedAt = DateTime.UtcNow,
            Entries = [.. state.Entries],
            Duration = sw.Elapsed,
            InaccessiblePaths = [.. state.Inaccessible]
        };
    }

    // ── Phase 1: Quick directory count ──────────────────────────────────────────

    private int CountDirectories(string rootPath, ScanOptions options, ScanState state,
        IProgress<ScanProgress>? progress, CancellationToken ct)
    {
        int count = 1; // root counts as 1
        CountDirsRecursive(rootPath, options, state, progress, ref count, ct);
        return count;
    }

    private void CountDirsRecursive(string directory, ScanOptions options, ScanState state,
        IProgress<ScanProgress>? progress, ref int count, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        IEnumerable<string> subdirs;
        try { subdirs = Directory.EnumerateDirectories(directory); }
        catch { return; }

        foreach (var subdir in subdirs)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var attrs = File.GetAttributes(subdir);
                if ((attrs & FileAttributes.ReparsePoint) != 0) continue;
            }
            catch { continue; }

            if (options.ExclusionEvaluator?.IsExcluded(subdir, isDirectory: true) == true)
                continue;

            count++;

            if (count % 500 == 0)
            {
                var now = state.Stopwatch.ElapsedMilliseconds;
                var last = Interlocked.Read(ref state.LastReportMs);
                if (now - last >= ProgressIntervalMs &&
                    Interlocked.CompareExchange(ref state.LastReportMs, now, last) == last)
                {
                    progress?.Report(new ScanProgress
                    {
                        ProcessedFiles = 0,
                        ProcessedDirectories = count,
                        TotalDirectories = 0,
                        CurrentDirectory = subdir,
                        Phase = ScanPhase.Counting,
                        ElapsedSeconds = state.Stopwatch.Elapsed.TotalSeconds
                    });
                }
            }

            CountDirsRecursive(subdir, options, state, progress, ref count, ct);
        }
    }

    // ── Phase 2: Actual scan ────────────────────────────────────────────────────

    private void WalkDirectory(
        string rootPath,
        string directory,
        ScanOptions options,
        ScanState state,
        IProgress<ScanProgress>? progress,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (options.ExclusionEvaluator?.IsExcluded(directory, isDirectory: true) == true)
            return;

        Interlocked.Increment(ref state.DirectoryCount);

        // Add the directory entry itself (except for root)
        if (!string.Equals(directory, rootPath, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var dirInfo = new DirectoryInfo(directory);
                state.Entries.Add(new ScanEntry
                {
                    FullPath = directory,
                    RelativePath = Path.GetRelativePath(rootPath, directory),
                    Name = dirInfo.Name,
                    Size = 0,
                    LastWriteTime = dirInfo.LastWriteTimeUtc,
                    LastAccessTime = dirInfo.LastAccessTimeUtc,
                    Type = ScanEntryType.Directory,
                    Extension = string.Empty
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Cannot read directory info: {Dir}", directory);
                state.Inaccessible.Add(directory);
            }
        }

        // Enumerate files
        IEnumerable<string> files;
        try
        {
            files = Directory.EnumerateFiles(directory);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cannot enumerate files in: {Dir}", directory);
            state.Inaccessible.Add(directory);
            return;
        }

        foreach (var file in files)
        {
            ct.ThrowIfCancellationRequested();

            if (options.ExclusionEvaluator?.IsExcluded(file, isDirectory: false) == true)
                continue;

            try
            {
                var fi = new FileInfo(file);
                state.Entries.Add(new ScanEntry
                {
                    FullPath = file,
                    RelativePath = Path.GetRelativePath(rootPath, file),
                    Name = fi.Name,
                    Size = fi.Length,
                    LastWriteTime = fi.LastWriteTimeUtc,
                    LastAccessTime = fi.LastAccessTimeUtc,
                    Type = ScanEntryType.File,
                    Extension = fi.Extension.ToLowerInvariant()
                });

                Interlocked.Increment(ref state.FileCount);
                ThrottledReport(state, directory, progress);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Cannot read file info: {File}", file);
                state.Inaccessible.Add(file);
            }
        }

        // Collect valid subdirectories
        IEnumerable<string> subdirs;
        try
        {
            subdirs = Directory.EnumerateDirectories(directory);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cannot enumerate subdirectories in: {Dir}", directory);
            state.Inaccessible.Add(directory);
            return;
        }

        var validDirs = new List<string>();
        foreach (var subdir in subdirs)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var attrs = File.GetAttributes(subdir);
                if ((attrs & FileAttributes.ReparsePoint) != 0)
                {
                    logger.LogDebug("Skipping symlink/junction: {Dir}", subdir);
                    continue;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Cannot read attributes for: {Dir}", subdir);
                state.Inaccessible.Add(subdir);
                continue;
            }

            validDirs.Add(subdir);
        }

        if (validDirs.Count > 1)
        {
            Parallel.ForEach(validDirs,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 8),
                    CancellationToken = ct
                },
                subdir => WalkDirectory(rootPath, subdir, options, state, progress, ct));
        }
        else
        {
            foreach (var subdir in validDirs)
                WalkDirectory(rootPath, subdir, options, state, progress, ct);
        }
    }

    // ── Progress reporting ──────────────────────────────────────────────────────

    private static void ThrottledReport(ScanState state, string currentDir, IProgress<ScanProgress>? progress)
    {
        if (progress is null) return;

        var now = state.Stopwatch.ElapsedMilliseconds;
        var last = Interlocked.Read(ref state.LastReportMs);

        if (now - last < ProgressIntervalMs) return;
        if (Interlocked.CompareExchange(ref state.LastReportMs, now, last) != last) return;

        progress.Report(new ScanProgress
        {
            ProcessedFiles = Volatile.Read(ref state.FileCount),
            ProcessedDirectories = Volatile.Read(ref state.DirectoryCount),
            TotalDirectories = state.TotalDirectories,
            CurrentDirectory = currentDir,
            Phase = ScanPhase.Scanning,
            ElapsedSeconds = state.Stopwatch.Elapsed.TotalSeconds
        });
    }

    private sealed class ScanState
    {
        public readonly ConcurrentBag<ScanEntry> Entries = new();
        public readonly ConcurrentBag<string> Inaccessible = new();
        public readonly Stopwatch Stopwatch;
        public int FileCount;
        public int DirectoryCount;
        public int TotalDirectories;
        public long LastReportMs = -ProgressIntervalMs;

        public ScanState(Stopwatch stopwatch) => Stopwatch = stopwatch;
    }
}
