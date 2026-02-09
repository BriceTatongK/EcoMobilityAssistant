using EcoMob.McpServer.Core.Services;
using EcoMob.McpServer.Contracts.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EcoMob.McpServer.Core
{
    public static class Extensions
    {
        public static IServiceCollection AddCoreLayer(this IServiceCollection services)
        {
            // Register core services here
            services.AddScoped<IEcoMobilityTimeseriesService, EcoMobilityTimeseriesService>();

            return services;
        }
    }
}
