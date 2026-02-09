using McpDotNet;
using McpDotNet.Protocol.Types;
using EcoMob.McpServer.Entry.McpTools;
using EcoMob.McpServer.Entry.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcoMob.McpServer.Entry.Extensions
{
    public static class McpServerExtension
    {
        public static IServiceCollection AddMcpServerConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection(nameof(McpServerSettings)).Get<McpServerSettings>();

            var serverBuilder = services.AddMcpServer(options =>
            {
                options.ServerInfo = new Implementation
                {
                    Name = "Mobility-MCP-Server",
                    Version = "1.0.0"
                };
            });

            serverBuilder.WithStdioServerTransport();

            serverBuilder.WithTools([typeof(MobilityTools)]);

            return services;
        }
    }
}
