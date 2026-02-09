using EcoMob.McpServer.Core;
using EcoMob.McpServer.Entry.Extensions;
using EcoMob.McpServer.Entry.Helpers;
using EcoMob.McpServer.Entry.Settings;
using EcoMob.McpServer.Infra.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;


var host = Host.CreateDefaultBuilder(args)

    .ConfigureLogging((context, logging) =>
    {
        // Config Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel
            .Debug()
            .WriteTo
            .File("logs/mcp-server-.txt", rollingInterval: RollingInterval.Minute)
            .CreateLogger();

        logging.ClearProviders();
        logging.AddSerilog();
    })

    .ConfigureServices((context, services) =>
    {
        // 1. Configurazione robusta delle Options
        var mcpSettings = context.Configuration.GetSection(nameof(McpServerSettings));
        services.AddOptions<McpServerSettings>()
            .Bind(mcpSettings)
            .ValidateDataAnnotations()
            .ValidateOnStart();


        // adding layers config
        services.AddInfraLayer(context.Configuration);
        services.AddCoreLayer();

        // adding MCP Server config
        services.AddMcpServerConfig(context.Configuration);
    })
    .Build();

try
{
    MobilityToolFacade._provider = host.Services;
    Log.Information("MCP Server starting ...");

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "MCP Server has stopped unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
