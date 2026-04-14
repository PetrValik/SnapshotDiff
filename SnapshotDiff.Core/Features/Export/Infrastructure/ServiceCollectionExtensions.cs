using Microsoft.Extensions.DependencyInjection;
using SnapshotDiff.Features.Export.Application;

namespace SnapshotDiff.Features.Export.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExport(this IServiceCollection services)
    {
        services.AddScoped<IExportService, ExportService>();
        return services;
    }
}
