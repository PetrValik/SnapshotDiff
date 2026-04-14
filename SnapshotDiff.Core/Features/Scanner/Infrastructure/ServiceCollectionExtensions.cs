using Microsoft.Extensions.DependencyInjection;
using SnapshotDiff.Features.Scanner.Application.Commands;
using SnapshotDiff.Features.Scanner.Application.Queries;

namespace SnapshotDiff.Features.Scanner.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScanner(this IServiceCollection services)
    {
        services.AddSingleton<IScannerService, ScannerService>();
        services.AddSingleton<IScanStateService, InMemoryScanStateService>();
        services.AddScoped<ScanDirectoryHandler>();
        services.AddScoped<FilterEntriesHandler>();
        return services;
    }
}
