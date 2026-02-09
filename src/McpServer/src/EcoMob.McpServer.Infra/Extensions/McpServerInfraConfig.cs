using EcoMob.McpServer.Contracts.DataProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EcoMob.McpServer.Infra.OdhClients;
using EcoMob.McpServer.Infra.Settings;
using EcoMob.McpServer.Infra.Cache;
using Microsoft.Extensions.Options;
using Polly;

namespace EcoMob.McpServer.Infra.Extensions
{
    public static class Extensions
    {
        public static IServiceCollection AddInfraLayer(this IServiceCollection services, IConfiguration configuration)
        {
            // register Odh settings
            var mcpSettings = configuration.GetSection(nameof(OdhSettings));
            services
                .AddOptions<OdhSettings>()
                .Bind(mcpSettings)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Registers IMemoryCache as a Singleton by default
            // You can also configure global options (like size limits)
            services.AddMemoryCache(options =>
            {
                // Useful to prevent the AI context from eating all RAM
                options.SizeLimit = 1024 * 2;
            });

            // memory cache
            services.AddSingleton<ICacheProvider, MemoryCacheProvider>();

            // httpClient
            services.AddHttpClient<IEcoMobilityTimeseriesClient, OdhTimeseriesClient>((IServiceProvider sp, HttpClient client) =>
            {
                var setting = sp.GetRequiredService<IOptions<OdhSettings>>().Value;

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new("application/json"));

                client.BaseAddress = new Uri(setting.TimeSeriesHost);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddTransientHttpErrorPolicy(policy =>
            policy.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(500))); // Resilience!


            return services;
        }
    }
}
