using System.Runtime.InteropServices;
using SnapshotDiff.Features.ExclusionRules.Domain;

namespace SnapshotDiff.Features.ExclusionRules.Infrastructure;

/// <summary>
/// Provides built-in system exclusion rules based on the current OS.
/// </summary>
internal sealed class DefaultExclusionProvider : IDefaultExclusionProvider
{
    private readonly IReadOnlyList<ExclusionRule> _rules = BuildRules();

    public IReadOnlyList<ExclusionRule> GetSystemRules() => _rules;

    private static IReadOnlyList<ExclusionRule> BuildRules()
    {
        var rules = new List<ExclusionRule>();

        // ── Universal rules ────────────────────────────────────────────────────
        AddUniversalRules(rules);

        // ── Platform-specific ─────────────────────────────────────────────────
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            AddWindowsRules(rules);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            AddMacRules(rules);
        else
            AddLinuxRules(rules);

        return rules.AsReadOnly();
    }

    private static void AddUniversalRules(List<ExclusionRule> rules)
    {
        rules.AddRange(new[]
        {
            Sys("universal-git",       ".git",         "Git repository metadata",           isDir: true),
            Sys("universal-gitignore", ".gitignore",   "Git ignore file"),
            Sys("universal-ds-store",  ".DS_Store",    "macOS folder metadata"),
            Sys("universal-thumbs",    "Thumbs.db",    "Windows thumbnail cache"),
        });
    }

    private static void AddWindowsRules(List<ExclusionRule> rules)
    {
        rules.AddRange(new[]
        {
            Sys("win-recycle",        "$RECYCLE.BIN",              "Windows Recycle Bin",                isDir: true),
            Sys("win-sysvolinfo",     "System Volume Information", "Windows system metadata directory",  isDir: true),
            Sys("win-pagefile",       "pagefile.sys",              "Windows page file"),
            Sys("win-hiberfil",       "hiberfil.sys",              "Windows hibernation file"),
            Sys("win-swapfile",       "swapfile.sys",              "Windows swap file"),
            Sys("win-dumpstack",      "DumpStack.log",             "Windows crash dump log"),
            Sys("win-dumpstacktmp",   "DumpStack.log.tmp",         "Windows crash dump log (temp)"),
            Sys("win-ntuser",         "NTUSER.DAT",                "Windows user registry hive"),
            Sys("win-ntuserlog",      "NTUSER.DAT.LOG*",           "Windows user registry log"),
            Sys("win-ntuser-ini",     "ntuser.ini",                "Windows user registry init"),
            Sys("win-desktop-ini",    "desktop.ini",               "Windows folder customisation file"),
            Sys("win-bootmgr",        "bootmgr",                   "Windows boot manager"),
            Sys("win-bootsect",       "BOOTSECT.BAK",              "Windows boot sector backup"),
            Sys("win-recovery",       "Recovery",                  "Windows recovery partition dir",     isDir: true),
            Sys("win-winsxs",         "WinSxS",                    "Windows side-by-side component store", isDir: true),
        });
    }

    private static void AddLinuxRules(List<ExclusionRule> rules)
    {
        rules.AddRange(new[]
        {
            Sys("lnx-proc",       "/proc",        "Linux virtual process filesystem",  isDir: true),
            Sys("lnx-sys",        "/sys",         "Linux kernel virtual filesystem",   isDir: true),
            Sys("lnx-dev",        "/dev",         "Linux device files",                isDir: true),
            Sys("lnx-run",        "/run",         "Linux runtime data",                isDir: true),
            Sys("lnx-lost",       "lost+found",   "Linux filesystem recovery dir",     isDir: true),
            Sys("lnx-trash",      ".Trash-*",     "Linux per-user trash directories",  isDir: true),
        });
    }

    private static void AddMacRules(List<ExclusionRule> rules)
    {
        rules.AddRange(new[]
        {
            Sys("mac-spotlight",  ".Spotlight-V100",           "Spotlight search index",     isDir: true),
            Sys("mac-trashes",    ".Trashes",                  "macOS Trash directory",       isDir: true),
            Sys("mac-fseventsd",  ".fseventsd",                "macOS file system events",   isDir: true),
            Sys("mac-docrev",     ".DocumentRevisions-V100",   "macOS document revisions",   isDir: true),
            Sys("mac-tempitems",  ".TemporaryItems",           "macOS temporary items",       isDir: true),
            Sys("mac-volicon",    ".VolumeIcon.icns",          "macOS volume icon"),
            Sys("mac-apdisk",     ".apdisk",                   "macOS AFP disk metadata"),
        });
    }

    private static ExclusionRule Sys(string id, string pattern, string desc, bool isDir = false) =>
        new()
        {
            Id = id,
            Pattern = pattern,
            Type = ExclusionRuleType.System,
            Scope = ExclusionScope.Global,
            Description = desc,
            IsEnabled = true,
            IsDirectoryOnly = isDir
        };
}
