using Microsoft.Extensions.DependencyInjection;

namespace SnapshotDiff.Infrastructure.FileIO;

public static class FileIOServiceCollectionExtensions
{
    /// <summary>
    /// Adds browser-based file I/O services for web applications
    /// Uses File System Access API when available, with automatic fallback
    /// </summary>
    public static IServiceCollection AddFileIO(this IServiceCollection services)
    {
        services.AddScoped<BrowserFileWriter>();
        services.AddScoped<IFileWriter>(sp => sp.GetRequiredService<BrowserFileWriter>());
        services.AddSingleton<IFileNameGenerator, FileNameGenerator>();

        return services;
    }
}
