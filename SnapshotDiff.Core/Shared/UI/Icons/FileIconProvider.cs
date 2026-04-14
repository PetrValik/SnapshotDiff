namespace SnapshotDiff.Shared.UI.Icons;

/// <summary>
/// Maps file extensions to Bootstrap Icons CSS classes by category
/// </summary>
public sealed class FileIconProvider : IFileIconProvider
{
    public FileIconProvider() { }

    public string DirectoryIcon => "bi bi-folder";

    // Extension → Bootstrap Icon
    private static readonly Dictionary<string, string> Icons = new(StringComparer.OrdinalIgnoreCase)
    {
        // Code
        [".cs"] = "bi bi-file-earmark-code",
        [".csx"] = "bi bi-file-earmark-code",
        [".js"] = "bi bi-file-earmark-code",
        [".jsx"] = "bi bi-file-earmark-code",
        [".mjs"] = "bi bi-file-earmark-code",
        [".ts"] = "bi bi-file-earmark-code",
        [".tsx"] = "bi bi-file-earmark-code",
        [".py"] = "bi bi-file-earmark-code",
        [".rb"] = "bi bi-file-earmark-code",
        [".java"] = "bi bi-file-earmark-code",
        [".kt"] = "bi bi-file-earmark-code",
        [".go"] = "bi bi-file-earmark-code",
        [".rs"] = "bi bi-file-earmark-code",
        [".c"] = "bi bi-file-earmark-code",
        [".cpp"] = "bi bi-file-earmark-code",
        [".h"] = "bi bi-file-earmark-code",
        [".php"] = "bi bi-file-earmark-code",
        [".swift"] = "bi bi-file-earmark-code",

        // Web
        [".html"] = "bi bi-filetype-html",
        [".htm"] = "bi bi-filetype-html",
        [".razor"] = "bi bi-filetype-html",
        [".css"] = "bi bi-filetype-css",
        [".scss"] = "bi bi-filetype-scss",
        [".sass"] = "bi bi-filetype-scss",
        [".less"] = "bi bi-filetype-css",
        [".vue"] = "bi bi-filetype-vue",
        [".svelte"] = "bi bi-file-earmark-code",

        // Data / Config
        [".json"] = "bi bi-filetype-json",
        [".xml"] = "bi bi-filetype-xml",
        [".yaml"] = "bi bi-filetype-yml",
        [".yml"] = "bi bi-filetype-yml",
        [".toml"] = "bi bi-file-earmark-text",
        [".ini"] = "bi bi-file-earmark-text",
        [".env"] = "bi bi-file-earmark-text",
        [".csv"] = "bi bi-filetype-csv",
        [".tsv"] = "bi bi-file-earmark-spreadsheet",
        [".sql"] = "bi bi-filetype-sql",
        [".db"] = "bi bi-database",
        [".sqlite"] = "bi bi-database",
        [".resx"] = "bi bi-file-earmark-text",

        // Documents
        [".md"] = "bi bi-markdown",
        [".txt"] = "bi bi-file-earmark-text",
        [".rtf"] = "bi bi-file-earmark-text",
        [".pdf"] = "bi bi-file-earmark-pdf",
        [".doc"] = "bi bi-file-earmark-word",
        [".docx"] = "bi bi-file-earmark-word",
        [".xls"] = "bi bi-file-earmark-excel",
        [".xlsx"] = "bi bi-file-earmark-excel",
        [".ppt"] = "bi bi-file-earmark-ppt",
        [".pptx"] = "bi bi-file-earmark-ppt",

        // Images
        [".png"] = "bi bi-file-earmark-image",
        [".jpg"] = "bi bi-file-earmark-image",
        [".jpeg"] = "bi bi-file-earmark-image",
        [".gif"] = "bi bi-file-earmark-image",
        [".svg"] = "bi bi-file-earmark-image",
        [".ico"] = "bi bi-file-earmark-image",
        [".webp"] = "bi bi-file-earmark-image",
        [".bmp"] = "bi bi-file-earmark-image",

        // Media
        [".mp3"] = "bi bi-file-earmark-music",
        [".wav"] = "bi bi-file-earmark-music",
        [".flac"] = "bi bi-file-earmark-music",
        [".ogg"] = "bi bi-file-earmark-music",
        [".mp4"] = "bi bi-file-earmark-play",
        [".avi"] = "bi bi-file-earmark-play",
        [".mkv"] = "bi bi-file-earmark-play",
        [".mov"] = "bi bi-file-earmark-play",

        // Archives
        [".zip"] = "bi bi-file-earmark-zip",
        [".rar"] = "bi bi-file-earmark-zip",
        [".tar"] = "bi bi-file-earmark-zip",
        [".gz"] = "bi bi-file-earmark-zip",
        [".7z"] = "bi bi-file-earmark-zip",

        // Build / Package
        [".sln"] = "bi bi-gear",
        [".csproj"] = "bi bi-gear",
        [".fsproj"] = "bi bi-gear",
        [".dll"] = "bi bi-file-binary",
        [".exe"] = "bi bi-file-binary",
        [".nupkg"] = "bi bi-box-seam",

        // Lock files
        [".lock"] = "bi bi-lock",
    };

    // Extension → CSS class for color coding
    private static readonly Dictionary<string, string> Categories = new(StringComparer.OrdinalIgnoreCase)
    {
        // Code - blue
        [".cs"] = "icon-code",
        [".csx"] = "icon-code",
        [".js"] = "icon-code",
        [".jsx"] = "icon-code",
        [".mjs"] = "icon-code",
        [".ts"] = "icon-code",
        [".tsx"] = "icon-code",
        [".py"] = "icon-code",
        [".rb"] = "icon-code",
        [".java"] = "icon-code",
        [".kt"] = "icon-code",
        [".go"] = "icon-code",
        [".rs"] = "icon-code",
        [".c"] = "icon-code",
        [".cpp"] = "icon-code",
        [".h"] = "icon-code",

        // Web - green
        [".html"] = "icon-web",
        [".htm"] = "icon-web",
        [".razor"] = "icon-web",
        [".css"] = "icon-web",
        [".scss"] = "icon-web",

        // Data - yellow
        [".json"] = "icon-data",
        [".xml"] = "icon-data",
        [".yaml"] = "icon-data",
        [".yml"] = "icon-data",
        [".toml"] = "icon-data",
        [".csv"] = "icon-data",
        [".sql"] = "icon-data",
        [".resx"] = "icon-data",

        // Images - pink
        [".png"] = "icon-image",
        [".jpg"] = "icon-image",
        [".jpeg"] = "icon-image",
        [".gif"] = "icon-image",
        [".svg"] = "icon-image",
        [".webp"] = "icon-image",

        // Documents - orange
        [".md"] = "icon-doc",
        [".txt"] = "icon-doc",
        [".pdf"] = "icon-doc",
        [".doc"] = "icon-doc",
        [".docx"] = "icon-doc",
    };

    // Special filenames
    private static readonly Dictionary<string, string> SpecialFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Dockerfile"] = "bi bi-docker",
        ["Makefile"] = "bi bi-hammer",
        ["LICENSE"] = "bi bi-shield-check",
        ["README"] = "bi bi-info-circle",
        [".gitignore"] = "bi bi-git",
        [".gitattributes"] = "bi bi-git",
        [".editorconfig"] = "bi bi-gear",
    };

    public string GetFileIcon(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            return "bi bi-file-earmark";

        if (SpecialFiles.TryGetValue(extension, out var special))
            return special;

        if (Icons.TryGetValue(extension, out var icon))
            return icon;

        return "bi bi-file-earmark";
    }

    public string GetIconClass(string extension)
    {
        if (string.IsNullOrEmpty(extension))
            return "icon-default";

        if (Categories.TryGetValue(extension, out var cls))
            return cls;

        return "icon-default";
    }
}
