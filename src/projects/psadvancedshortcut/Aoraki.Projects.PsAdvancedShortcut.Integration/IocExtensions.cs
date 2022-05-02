using Aoraki.Contracts.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace Aoraki.Projects.PsAdvancedShortcut.Integration;

public static class IocExtensions
{
    public static IServiceCollection SetupPsAdvancedShortcutProject(this IServiceCollection services)
    {
        services.AddSingleton<IProjectDefinition, PsAdvancedShortcutProjectDefinition>();
        return services;
    }
}