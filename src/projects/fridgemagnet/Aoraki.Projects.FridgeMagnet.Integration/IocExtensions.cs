using Aoraki.Contracts.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace Aoraki.Projects.FridgeMagnet.Integration;

public static class IocExtensions
{
    public static IServiceCollection SetupFridgeMagnetProject(this IServiceCollection services)
    {
        services.AddSingleton<IProjectDefinition, FridgeMagnetProjectDefinition>();
        return services;
    }
}