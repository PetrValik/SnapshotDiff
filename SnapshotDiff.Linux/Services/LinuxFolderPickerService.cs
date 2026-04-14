using SnapshotDiff.Infrastructure.Storage;

namespace SnapshotDiff.Linux.Services;

/// <summary>
/// Linux folder picker using zenity (GTK) or kdialog (KDE) subprocess.
/// Falls back gracefully if neither tool is available.
/// </summary>
public sealed class LinuxFolderPickerService : IFolderPickerService
{
    private static readonly string? _tool = FindPickerTool();

    public bool IsSupported => _tool is not null;

    public async Task<string?> PickFolderAsync(CancellationToken ct = default)
    {
        if (_tool is null)
            return null;

        var arguments = _tool switch
        {
            "zenity" => "--file-selection --directory --title=Select Folder",
            "kdialog" => "--getexistingdirectory /",
            _ => null
        };

        if (arguments is null)
            return null;

        var psi = new System.Diagnostics.ProcessStartInfo(_tool, arguments)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = System.Diagnostics.Process.Start(psi);
        if (process is null)
            return null;

        var output = await process.StandardOutput.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        var path = output.Trim();
        return string.IsNullOrEmpty(path) ? null : path;
    }

    private static string? FindPickerTool()
    {
        foreach (var tool in new[] { "zenity", "kdialog" })
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo("which", tool)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using var process = System.Diagnostics.Process.Start(psi);
                process?.WaitForExit();
                if (process?.ExitCode == 0)
                    return tool;
            }
            catch
            {
                // Ignore – tool not available
            }
        }
        return null;
    }
}
