using Microsoft.Extensions.Logging;

namespace EcoMob.McpServer.Entry
{
    internal class App(ILogger<App> logger)
    {
        private readonly ILogger<App> _logger = logger;

        public async Task RunAsync()
        {
            _logger.LogInformation("EcoMob MCP Server is running...");
        }
    }
}
