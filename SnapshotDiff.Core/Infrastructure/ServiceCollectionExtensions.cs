using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SnapshotDiff.Infrastructure.FileIO;
using SnapshotDiff.Infrastructure.Hashing;
using SnapshotDiff.Infrastructure.Localization;
using SnapshotDiff.Infrastructure.Notifications;
using SnapshotDiff.Infrastructure.Permissions;
using SnapshotDiff.Infrastructure.Storage;
using SnapshotDiff.Infrastructure.Theme;
using SnapshotDiff.Shared.UI.Icons;

namespace SnapshotDiff.Infrastructure;

/// <summary>
/// Registrace sdílené infrastruktury (cross-cutting concerns).
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registruje sdílené infrastructure služby (hashing, localization, notifications, file I/O).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Hashing
        services.AddSingleton<IHasher, Sha256Hasher>();

        // Notifications - Singleton aby fungoval napříč scopes
        services.AddSingleton<INotificationService, NotificationService>();

        // Culture state - shared singleton for culture change notifications
        services.AddSingleton<CultureState>();

        // Culture-aware string localizer — syncs thread culture from CultureState
        // before every lookup. Registered before AddLocalization() so TryAdd skips
        // the default StringLocalizer<T>.
        services.AddTransient(typeof(IStringLocalizer<>), typeof(CultureStateStringLocalizer<>));

        // File I/O
        services.AddFileIO();

        // UI Icons
        services.AddSingleton<IFileIconProvider, FileIconProvider>();

        // Theme
        services.AddScoped<IThemeService, ThemeService>();

        // Permissions - desktop default (always granted)
        services.AddSingleton<IPlatformPermissionService, DefaultPermissionService>();

        // Storage path provider fallback (can be overridden by host before calling AddInfrastructure)
        if (!services.Any(s => s.ServiceType == typeof(IStoragePathProvider)))
            services.AddSingleton<IStoragePathProvider, FallbackStoragePathProvider>();

        // Folder picker fallback – unsupported on this platform (e.g. tests)
        // Hosts (MAUI, Linux) register their own implementation before AddInfrastructure.
        if (!services.Any(s => s.ServiceType == typeof(IFolderPickerService)))
            services.AddSingleton<IFolderPickerService, UnsupportedFolderPickerService>();

        return services;
    }
}
