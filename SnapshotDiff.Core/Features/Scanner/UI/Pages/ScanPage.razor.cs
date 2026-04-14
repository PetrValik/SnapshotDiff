using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SnapshotDiff.Features.Config.Domain;
using SnapshotDiff.Features.Config.Infrastructure;
using SnapshotDiff.Features.ExclusionRules.Infrastructure;
using SnapshotDiff.Features.Export.Application;
using SnapshotDiff.Features.Export.Application.Models;
using SnapshotDiff.Features.Scanner.Application.Commands;
using SnapshotDiff.Features.Scanner.Application.Queries;
using SnapshotDiff.Features.Scanner.Domain;
using SnapshotDiff.Features.Scanner.Infrastructure;
using SnapshotDiff.Features.Trash.Infrastructure;
using SnapshotDiff.Infrastructure.Notifications;
using SnapshotDiff.Infrastructure.Storage;

namespace SnapshotDiff.Features.Scanner.UI.Pages;

public partial class ScanPage : ComponentBase, IDisposable
{
    [Inject] private IConfigService ConfigService { get; set; } = default!;
    [Inject] private IScannerService ScannerService { get; set; } = default!;
    [Inject] private IScanStateService ScanStateService { get; set; } = default!;
    [Inject] private ScanDirectoryHandler ScanHandler { get; set; } = default!;
    [Inject] private FilterEntriesHandler FilterHandler { get; set; } = default!;
    [Inject] private IExclusionService ExclusionService { get; set; } = default!;
    [Inject] private ITrashService TrashService { get; set; } = default!;
    [Inject] private INotificationService Notifications { get; set; } = default!;
    [Inject] private IExportService ExportService { get; set; } = default!;
    [Inject] private IStringLocalizer<ScanResources> Loc { get; set; } = default!;
    [Inject] private ILogger<ScanPage> Logger { get; set; } = default!;
    [Inject] private IFolderPickerService FolderPicker { get; set; } = default!;

    private List<WatchedDirectory> _watchedDirs = [];
    private string _selectedPath = string.Empty;
    private bool _isScanning;
    private CancellationTokenSource? _cts;
    private ScanProgress? _scanProgress;
    private ScanResult? _scanResult;
    private List<ScanEntry> _filteredEntries = [];
    private HashSet<string> _selectedPaths = new(StringComparer.OrdinalIgnoreCase);

    // Directory tree
    private DirectoryNode? _directoryTree;
    private string? _selectedSubDir;

    // Extension chips
    private List<ExtensionInfo> _topExtensions = [];
    private HashSet<string> _activeExtensions = new(StringComparer.OrdinalIgnoreCase);

    // Filters
    private FileAgeFilter _ageFilter = FileAgeFilter.All;
    private int? _staleAfterDays;
    private int? _newWithinDays;
    private long? _minSizeBytes;
    private string _extensionFilter = string.Empty;
    private string _nameSearch = string.Empty;
    private SortField _sortBy = SortField.Name;
    private bool _sortAscending = true;

    // Delete dialog
    private bool _showDeleteConfirm;
    private bool _hasSystemProtected;
    private string _deleteConfirmText = string.Empty;

    // Export dialog
    private bool _showExportDialog;
    private bool _isExporting;
    private string _exportFormat = "Json";
    private string _exportFileName = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await ConfigService.LoadAsync();
        ConfigService.ConfigChanged += OnConfigChanged;
        LoadDirectories();
        RestoreScanState();
    }

    private void LoadDirectories()
    {
        _watchedDirs = ConfigService.Current.WatchedDirectories.ToList();

        if (string.IsNullOrEmpty(_selectedPath) && _watchedDirs.Count > 0)
            _selectedPath = _watchedDirs[0].Path;
    }

    private void RestoreScanState()
    {
        if (string.IsNullOrEmpty(_selectedPath)) return;
        var cached = ScanStateService.Get(_selectedPath);
        if (cached is null) return;

        _scanResult = cached;
        var rootName = Path.GetFileName(_selectedPath);
        if (string.IsNullOrEmpty(rootName)) rootName = _selectedPath;
        _directoryTree = DirectoryNode.BuildFromEntries(_scanResult.Entries, rootName);
        ComputeTopExtensions();
        ApplyFilters();
    }

    private void OnConfigChanged(object? sender, EventArgs e)
    {
        LoadDirectories();
        InvokeAsync(StateHasChanged);
    }

    private async Task AddDirectoryAsync()
    {
        var folder = await FolderPicker.PickFolderAsync();
        if (string.IsNullOrEmpty(folder)) return;

        if (!Directory.Exists(folder)) return;

        var existing = ConfigService.Current.WatchedDirectories
            .Any(d => string.Equals(d.Path, folder, StringComparison.OrdinalIgnoreCase));
        if (existing)
        {
            Notifications.ShowInfo(Loc["DirAlreadyAdded"]);
            _selectedPath = folder;
            StateHasChanged();
            return;
        }

        await ConfigService.AddWatchedDirectoryAsync(folder);
        LoadDirectories();
        _selectedPath = folder;
        Notifications.ShowSuccess(Loc["DirAdded"]);
        StateHasChanged();
    }

    private async Task OnScanAsync()
    {
        if (string.IsNullOrEmpty(_selectedPath) || _isScanning) return;

        // Validate directory still exists
        if (!Directory.Exists(_selectedPath))
        {
            Notifications.ShowError(Loc["DirNotFound", _selectedPath]);
            await ConfigService.RemoveWatchedDirectoryAsync(_selectedPath);
            _selectedPath = ConfigService.Current.WatchedDirectories.FirstOrDefault()?.Path ?? string.Empty;
            StateHasChanged();
            return;
        }

        _cts = new CancellationTokenSource();
        _isScanning = true;
        _scanResult = null;
        _filteredEntries = [];
        _selectedPaths.Clear();
        _directoryTree = null;
        _selectedSubDir = null;
        _scanProgress = null;
        _topExtensions = [];
        _activeExtensions.Clear();
        _extensionFilter = string.Empty;
        StateHasChanged();

        try
        {
            var progress = new Progress<ScanProgress>(p =>
            {
                _scanProgress = p;
                InvokeAsync(StateHasChanged);
            });

            var cmd = new ScanDirectoryCommand(_selectedPath);
            _scanResult = await ScanHandler.HandleAsync(cmd, progress, _cts.Token);

            // Build directory tree from results
            var rootName = Path.GetFileName(_selectedPath);
            if (string.IsNullOrEmpty(rootName)) rootName = _selectedPath;
            _directoryTree = DirectoryNode.BuildFromEntries(_scanResult.Entries, rootName);

            // Compute top extensions for filter chips
            ComputeTopExtensions();

            // Update LastScannedAt in config
            await ConfigService.UpdateWatchedDirectoryAsync(
                _selectedPath, d => d.LastScannedAt = DateTime.UtcNow);

            ApplyFilters();
            Notifications.ShowSuccess(Loc["ScanComplete", _scanResult.Entries.Count]);
        }
        catch (OperationCanceledException)
        {
            Notifications.ShowInfo(Loc["ScanCancelled"]);
        }
        catch (Exception ex)
        {
            Notifications.ShowError(Loc["ScanFailed", ex.Message]);
        }
        finally
        {
            _isScanning = false;
            _cts?.Dispose();
            _cts = null;
            StateHasChanged();
        }
    }

    private void CancelScan() => _cts?.Cancel();

    private void OnSelectDirectory(string relativePath)
    {
        _selectedSubDir = string.IsNullOrEmpty(relativePath) ? null : relativePath;
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        if (_scanResult is null) return;

        var query = new FilterEntriesQuery(
            _selectedPath,
            _ageFilter,
            _staleAfterDays,
            _newWithinDays,
            _minSizeBytes,
            _extensionFilter,
            _nameSearch,
            _sortBy,
            _sortAscending,
            _selectedSubDir);

        _filteredEntries = FilterHandler.Handle(query);

        // Remove selections that are no longer visible
        var visible = _filteredEntries.Select(e => e.FullPath).ToHashSet(StringComparer.OrdinalIgnoreCase);
        _selectedPaths.IntersectWith(visible);

        StateHasChanged();
    }

    private void SetAgeFilter(FileAgeFilter filter)
    {
        _ageFilter = filter;
        ApplyFilters();
    }

    private void SetStaleDays(int days) { _staleAfterDays = days; ApplyFilters(); }
    private void SetNewDays(int days) { _newWithinDays = days; ApplyFilters(); }

    private void OnStaleDaysChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var v)) { _staleAfterDays = v; ApplyFilters(); }
    }

    private void OnNewDaysChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var v)) { _newWithinDays = v; ApplyFilters(); }
    }

    private void OnMinSizeChanged(ChangeEventArgs e)
    {
        if (long.TryParse(e.Value?.ToString(), out var v)) { _minSizeBytes = v > 0 ? v : null; ApplyFilters(); }
        else { _minSizeBytes = null; ApplyFilters(); }
    }

    private void ClearFilters()
    {
        _ageFilter = FileAgeFilter.All;
        _staleAfterDays = null;
        _newWithinDays = null;
        _minSizeBytes = null;
        _extensionFilter = string.Empty;
        _nameSearch = string.Empty;
        _selectedSubDir = null;
        _activeExtensions.Clear();
        ApplyFilters();
    }

    private void SetSort(SortField field)
    {
        if (_sortBy == field) _sortAscending = !_sortAscending;
        else { _sortBy = field; _sortAscending = true; }
        ApplyFilters();
    }

    private string SortIndicator(SortField field)
    {
        if (_sortBy != field) return "";
        return _sortAscending ? "↑" : "↓";
    }

    private string ThresholdClass(int days)
    {
        var active = (_ageFilter == FileAgeFilter.Stale && _staleAfterDays == days)
                  || (_ageFilter == FileAgeFilter.New && _newWithinDays == days);
        return active ? "app-btn app-btn--xs app-btn--primary" : "app-btn app-btn--xs app-btn--ghost";
    }

    // Selection
    private bool IsSelected(string path) => _selectedPaths.Contains(path);

    private void ToggleEntry(string path)
    {
        if (!_selectedPaths.Remove(path))
            _selectedPaths.Add(path);
    }

    private void ToggleSelectAll(ChangeEventArgs e)
    {
        var check = e.Value is bool b && b;
        if (check)
            foreach (var entry in _filteredEntries)
                _selectedPaths.Add(entry.FullPath);
        else
            _selectedPaths.Clear();
    }

    private long _selectedTotalSize => _filteredEntries
        .Where(e => _selectedPaths.Contains(e.FullPath))
        .Sum(e => e.Size);

    // Trash / Delete
    private async Task TrashSingleAsync(string path)
    {
        try
        {
            await TrashService.MoveToTrashAsync(path);
            _filteredEntries.RemoveAll(e => e.FullPath == path);
            _selectedPaths.Remove(path);

            if (_scanResult is not null)
            {
                _scanResult = _scanResult with
                {
                    Entries = _scanResult.Entries.Where(e => e.FullPath != path).ToList()
                };
                ScanStateService.Store(_scanResult);
            }

            Notifications.ShowSuccess(Loc["MovedToTrash"]);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Notifications.ShowError(ex.Message);
        }
    }

    private async Task MoveSelectionToTrashAsync()
    {
        var paths = _selectedPaths.ToList();
        var errors = 0;
        foreach (var path in paths)
        {
            try { await TrashService.MoveToTrashAsync(path); }
            catch { errors++; }
        }

        _filteredEntries.RemoveAll(e => paths.Contains(e.FullPath));
        _selectedPaths.Clear();

        if (_scanResult is not null)
        {
            _scanResult = _scanResult with
            {
                Entries = _scanResult.Entries.Where(e => !paths.Contains(e.FullPath)).ToList()
            };
            ScanStateService.Store(_scanResult);
        }

        if (errors > 0)
            Notifications.ShowWarning(Loc["TrashPartialError", errors]);
        else
            Notifications.ShowSuccess(Loc["MovedToTrashCount", paths.Count]);

        StateHasChanged();
    }

    private async Task DeleteSelectionPermanentlyAsync()
    {
        var evaluator = ExclusionService.GetEvaluatorForScan(_selectedPath);
        _hasSystemProtected = _selectedPaths.Any(p => evaluator.IsSystemProtected(p));
        _deleteConfirmText = string.Empty;
        _showDeleteConfirm = true;
        await InvokeAsync(StateHasChanged);
    }

    private void CancelDelete()
    {
        _showDeleteConfirm = false;
        _deleteConfirmText = string.Empty;
    }

    private async Task ConfirmDeleteAsync()
    {
        _showDeleteConfirm = false;
        var paths = _selectedPaths.ToList();
        var errors = 0;

        foreach (var path in paths)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
                else if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
                Logger.LogInformation("Permanently deleted: {Path}", path);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to permanently delete: {Path}", path);
                errors++;
            }
        }

        _filteredEntries.RemoveAll(e => paths.Contains(e.FullPath));
        _selectedPaths.Clear();

        // Keep _scanResult in sync so permanently deleted entries don't reappear after a filter change
        if (_scanResult is not null)
        {
            _scanResult = _scanResult with
            {
                Entries = _scanResult.Entries.Where(e => !paths.Contains(e.FullPath)).ToList()
            };
            ScanStateService.Store(_scanResult);
        }

        if (errors > 0)
            Notifications.ShowWarning(Loc["DeletePartialError", errors]);
        else
            Notifications.ShowSuccess(Loc["DeletedCount", paths.Count]);

        StateHasChanged();
    }

    private static string FormatSize(long bytes)
    {
        return bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
            < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
            _ => $"{bytes / (1024.0 * 1024 * 1024):F2} GB"
        };
    }

    private int ProgressPercent
    {
        get
        {
            if (_scanProgress is null || _scanProgress.TotalDirectories <= 0)
                return 0;
            var pct = (int)(100.0 * _scanProgress.ProcessedDirectories / _scanProgress.TotalDirectories);
            return Math.Clamp(pct, 0, 100);
        }
    }

    private int TotalScanEntries => _scanResult?.Entries.Count(e => e.Type == ScanEntryType.File) ?? 0;

    private void OpenExportDialog()
    {
        if (_filteredEntries.Count == 0) return;

        var dirName = Path.GetFileName(_selectedPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        if (string.IsNullOrEmpty(dirName)) dirName = _selectedPath.Replace(":", "").Trim('\\', '/');
        var date = DateTime.Now.ToString("yyyyMMdd");
        var ext = _exportFormat == "Csv" ? "csv" : "json";
        _exportFileName = $"{dirName}_{date}.{ext}";
        _showExportDialog = true;
        StateHasChanged();
    }

    private void CancelExport()
    {
        _showExportDialog = false;
    }

    private void UpdateExportExtension()
    {
        if (string.IsNullOrWhiteSpace(_exportFileName)) return;
        var nameWithoutExt = Path.GetFileNameWithoutExtension(_exportFileName);
        var ext = _exportFormat == "Csv" ? "csv" : "json";
        _exportFileName = $"{nameWithoutExt}.{ext}";
    }

    private async Task ConfirmExportAsync()
    {
        if (_filteredEntries.Count == 0 || _isExporting) return;

        _isExporting = true;
        StateHasChanged();
        try
        {
            var format = _exportFormat == "Csv" ? ExportFormat.Csv : ExportFormat.Json;
            var result = await ExportService.ExportAsync(_filteredEntries, format, _exportFileName);
            _showExportDialog = false;
            if (result.IsSuccess)
                Notifications.ShowSuccess(string.Format(Loc["ExportSuccess"], result.Value!.EntryCount, result.Value.FilePath));
            else
                Notifications.ShowError(string.Format(Loc["ExportFailed"], result.Error));
        }
        finally
        {
            _isExporting = false;
            StateHasChanged();
        }
    }

    // Extension chip helpers
    private sealed record ExtensionInfo(string Extension, int Count);

    private void ComputeTopExtensions()
    {
        if (_scanResult is null) { _topExtensions = []; return; }
        _topExtensions = _scanResult.Entries
            .Where(e => e.Type == ScanEntryType.File && !string.IsNullOrEmpty(e.Extension))
            .GroupBy(e => e.Extension.ToLowerInvariant())
            .OrderByDescending(g => g.Count())
            .Take(20)
            .Select(g => new ExtensionInfo(g.Key, g.Count()))
            .ToList();
        _activeExtensions.Clear();
    }

    private void ToggleExtension(string ext)
    {
        if (!_activeExtensions.Remove(ext))
            _activeExtensions.Add(ext);
        _extensionFilter = string.Join(", ", _activeExtensions);
        ApplyFilters();
    }

    // Breadcrumb helpers
    private sealed record BreadcrumbSegment(string Name, string Path);

    private IEnumerable<BreadcrumbSegment> GetBreadcrumbs()
    {
        if (string.IsNullOrEmpty(_selectedSubDir)) yield break;
        var parts = _selectedSubDir.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var path = "";
        foreach (var part in parts)
        {
            path = string.IsNullOrEmpty(path) ? part : Path.Combine(path, part);
            yield return new BreadcrumbSegment(part, path);
        }
    }

    public void Dispose()
    {
        ConfigService.ConfigChanged -= OnConfigChanged;
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
