using Microsoft.Extensions.DependencyInjection;
using SnapshotDiff.Features.Config.Infrastructure;
using SnapshotDiff.Infrastructure.Storage;

namespace SnapshotDiff.Features.Config;

public static class ConfigServiceCollectionExtensions
{
    public static IServiceCollection AddConfig(this IServiceCollection services)
    {
        if (!services.Any(s => s.ServiceType == typeof(IStoragePathProvider)))
            services.AddSingleton<IStoragePathProvider, FallbackStoragePathProvider>();

        services.AddSingleton<IConfigService, ConfigService>();
        return services;
    }
}
