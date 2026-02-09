using EcoMob.McpSseServer.WebApp.McpTools;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add MCP services
builder.Services
        .AddMcpServer()
        .WithHttpTransport()
        .WithTools<MobilityTools>();

var app = builder.Build();

// Enable routing
app.UseRouting();

// Map an SSE endpoint
app.MapMcp();

app.Run();
