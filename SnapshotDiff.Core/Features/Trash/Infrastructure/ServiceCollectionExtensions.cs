using Microsoft.Extensions.DependencyInjection;

namespace SnapshotDiff.Features.Trash.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTrash(this IServiceCollection services)
    {
        services.AddSingleton<ITrashRepository, SqliteTrashRepository>();
        services.AddSingleton<ITrashService, TrashService>();
        services.AddSingleton<Application.Commands.MoveToTrashHandler>();
        services.AddSingleton<Application.Commands.RestoreFromTrashHandler>();
        services.AddSingleton<Application.Commands.DeletePermanentlyHandler>();
        services.AddSingleton<Application.Commands.EmptyTrashHandler>();
        services.AddSingleton<Application.Queries.GetTrashItemsHandler>();
        services.AddHostedService<TrashPurgeService>();
        return services;
    }
}
