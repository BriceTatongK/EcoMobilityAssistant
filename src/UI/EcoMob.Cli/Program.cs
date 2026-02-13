using Serilog;
using EcoMob.Cli;
using EcoMob.Core.Extensions;
using EcoMob.Infra.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using IHost host = Host.CreateDefaultBuilder(args)

    // config logging to file.
    .ConfigureLogging((context, logging) =>
    {
        // Config Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel
            .Debug()
            .WriteTo
            .File("logs/mcp-client.txt", rollingInterval: RollingInterval.Minute)
            .CreateLogger();

        logging.ClearProviders();
        logging.AddSerilog(Log.Logger);
    })

    .ConfigureServices((hostContext, services) =>
    {
        // IoC inversion of control
        services.AddInfrastructureLayer(hostContext.Configuration);
        services.AddCoreLayer();

        services.AddTransient<App>();
    })
    .Build();

var myApp = host.Services.GetRequiredService<App>();
await myApp.RunAsync();