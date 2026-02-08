using EcoMob.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EcoMob.Core.Extensions
{
    public static class CoreLayerConfig
    {
        public static IServiceCollection AddCoreLayer(this IServiceCollection services)
        {
            services.AddSingleton<EcoMobilityService>();
            return services;
        }
    }
}