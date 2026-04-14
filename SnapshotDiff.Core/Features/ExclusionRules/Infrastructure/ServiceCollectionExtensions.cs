using Microsoft.Extensions.DependencyInjection;
using SnapshotDiff.Features.ExclusionRules.Application.Commands;
using SnapshotDiff.Features.ExclusionRules.Application.Queries;

namespace SnapshotDiff.Features.ExclusionRules.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddExclusionRules(this IServiceCollection services)
    {
        services.AddSingleton<IDefaultExclusionProvider, DefaultExclusionProvider>();
        services.AddSingleton<IExclusionService, ExclusionService>();

        services.AddSingleton<GetExclusionRulesHandler>();
        services.AddSingleton<AddGlobalRuleHandler>();
        services.AddSingleton<RemoveGlobalRuleHandler>();
        services.AddSingleton<ToggleGlobalRuleHandler>();
        services.AddSingleton<AddPerDirectoryPatternHandler>();
        services.AddSingleton<RemovePerDirectoryPatternHandler>();

        return services;
    }
}
