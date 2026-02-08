using EcoMob.Contracts.Services;
using EcoMob.Infra.Models;
using EcoMob.Infra.Services;
using EcoMob.Infra.Settings;
using McpDotNet.Client;
using McpDotNet.Configuration;
using McpDotNet.Protocol.Transport;
using McpDotNet.Protocol.Types;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OllamaSharp;
using OpenAI;

namespace EcoMob.Infra.Extensions
{
    public static class InfraLayerConfig
    {
        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            // 1. Configurazione robusta delle Options
            var mcpSettings = configuration.GetSection(nameof(McpSettings));
            services.AddOptions<McpSettings>()
                .Bind(mcpSettings)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // 2. Registrazione dell'infrastruttura MCP (client, transport, etc.)
            RegisterMcpInfrastructure(services);

            // 4. Registrazione del ChatClient AI (IChatClient)
            // Nota: registriamo IChatClient come singleton così che ClassifierModel possa riceverlo e gestirne il ciclo di vita.
            RegisterAgentsChatClients(services);

            // registration of Agents
            services.AddTransient<IReasoningAgent, ReasoningAgent>();
            services.AddTransient<IIntentAgent, IntentClassifierAgent>();
            services.AddTransient<IValidatorAgent, ContextValidatorAgent>();

            return services;
        }

        /// <summary>
        /// register the infrastructure for mcp client
        /// </summary>
        /// <param name="services"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private static void RegisterMcpInfrastructure(IServiceCollection services)
        {
            // choosing the transport.
            services.AddScoped<IClientTransport>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<McpSettings>>().Value;
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("McpTransportFactory");

                if (settings.TransportType.Equals("sse", StringComparison.OrdinalIgnoreCase))
                {
                    var fullUrl = settings.SseUrl ?? throw new InvalidOperationException("SSE URL missing");
                    var httpClient = new HttpClient { BaseAddress = new Uri(fullUrl + "/sse") };
                    return new SseClientTransport(new SseClientTransportOptions(), PrepareServerConfig(settings), httpClient, loggerFactory);
                }

                // Default: stdio
                return new StdioClientTransport(new StdioClientTransportOptions
                {
                    Command = "dotnet",
                    Arguments = $"run --project \"{settings.ServerPath}\" --nologo -v q"
                }, PrepareServerConfig(settings), loggerFactory);
            });

            // Registrazione del Client che consuma il Transport iniettato
            services.AddScoped<IMcpClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<McpSettings>>().Value;
                var transport = sp.GetRequiredService<IClientTransport>();
                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger(typeof(InfraLayerConfig));

                logger.LogDebug("Creating IMcpClient for TransportType={TransportType}.", settings.TransportType);

                try
                {
                    var clientOptions = new McpClientOptions
                    {
                        ClientInfo = new Implementation
                        {
                            Name = "EcoMobilityAssistantClient",
                            Version = "1.0.0"
                        }
                    };

                    var client = McpClientFactory.CreateAsync(PrepareServerConfig(settings), clientOptions, null, loggerFactory)
                                                .GetAwaiter()
                                                .GetResult();

                    logger.LogInformation("IMcpClient created successfully.");

                    return client;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to create IMcpClient.");
                    throw;
                }
            });
        }




        /// <summary>
        /// register chat client based on agent role
        /// </summary>
        /// <param name="services"></param>
        private static void RegisterAgentsChatClients(IServiceCollection services)
        {
            foreach (var role in Enum.GetValues<AgentRole>())
            {
                services.AddKeyedTransient<IChatClient>(role, (sp, key) =>
                {
                    var settings = sp.GetRequiredService<IOptions<McpSettings>>().Value;
                    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(InfraLayerConfig));

                    // 1. Determine which model name to use based on role
                    string modelName = role switch
                    {
                        AgentRole.Reasoning => settings.ModelName, // e.g., gpt-4o or llama3
                        _ => "gpt-4o-mini" // Intent and Validator use cheaper models
                    };

                    // 2. Create the base client (OpenAI or Ollama)
                    IChatClient baseClient = CreateBaseClient(settings, modelName, logger);

                    // 3. Build the pipeline
                    var builder = new ChatClientBuilder(baseClient);

                    // Only Reasoning Agent needs History and Function Invocation (MCP)
                    if (role == AgentRole.Reasoning)
                    {
                        builder.Use(client => new ChatHistoryMiddleware(client, settings.MaxChatMemoryItems))
                               .UseLogging(sp.GetRequiredService<ILoggerFactory>())
                               .UseFunctionInvocation();
                    }

                    return builder.Build();
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="modelName"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static IChatClient CreateBaseClient(McpSettings settings, string modelName, ILogger logger)
        {
            // Toggle this based on a new setting: settings.UseLocalModel
            if (settings.UseLocalModel)
            {
                logger.LogDebug("Creating Local Ollama client (model={Model}).", modelName);
                var model = new OllamaApiClient(new Uri(settings.LocalModelUrl), modelName);
                return model;
            }

            if (string.IsNullOrWhiteSpace(settings.OpenAIKey))
                throw new InvalidOperationException("OpenAI key is missing.");

            logger.LogDebug("Creating OpenAI client (model={Model}).", modelName);
            return new OpenAIClient(settings.OpenAIKey)
                        .GetChatClient(modelName)
                        .AsIChatClient();
        }

        /// <summary>
        /// Creates and configures an instance of McpServerConfig based on the specified settings.
        /// </summary>
        /// <param name="settings">The settings used to determine the server configuration, including transport type and connection details.
        /// Cannot be null.</param>
        /// <returns>A configured McpServerConfig instance representing either a remote server (using SSE) or a local server
        /// (using standard input/output), depending on the provided settings.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the required SSE URL or server path is missing from the settings.</exception>
        private static McpServerConfig PrepareServerConfig(McpSettings settings)
        {
            if (settings.TransportType.Equals("sse", StringComparison.OrdinalIgnoreCase))
            {
                // Configurazione per server remoto tramite HTTP/SSE
                return new McpServerConfig
                {
                    Id = "EcoMobilityServer", // Un identificativo univoco per la sessione
                    Name = "EcoMobility-McpServer",
                    TransportType = settings.TransportType,
                    Location = settings.SseUrl ?? throw new InvalidOperationException("SSE URL missing"),
                };
            }
            else
            {
                // Configurazione per server locale tramite Standard Input/Output
                return new McpServerConfig
                {
                    Id = "EcoMobilityServer",
                    Name = "EcoMobility-McpServer",
                    TransportType = settings.TransportType,
                    Location = settings.ServerPath ?? throw new InvalidOperationException("Server Path missing"),
                    Arguments = new[] { "run", "--project", settings.ServerPath ?? throw new InvalidOperationException("Server Path missing") }
                };
            }
        }


        //public class LanguageDetectionMiddleware : DelegatingChatClient
        //{
        //    public LanguageDetectionMiddleware(IChatClient innerClient) : base(innerClient) { }

        //    public override async Task<ChatResponse> GetResponseAsync(
        //        IList<ChatMessage> messages,
        //        ChatOptions? options = null,
        //        CancellationToken cancellationToken = default)
        //    {
        //        // 1. Find the last user message
        //        var lastUserMessage = messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text;

        //        if (!string.IsNullOrEmpty(lastUserMessage))
        //        {
        //            // 2. Detect language (Simple pseudo-code for the detection logic)
        //            var detector = new LanguageDetector();
        //            detector.AddAllLanguages();
        //            string lang = detector.Detect(lastUserMessage); // returns "it", "en", "fr", etc.

        //            // 3. Inject a hidden instruction for the LLM
        //            messages.Insert(0, new ChatMessage(ChatRole.System,
        //                $"User language detected: {lang}. Please respond exclusively in this language."));
        //        }

        //        return await base.GetResponseAsync(messages, options, cancellationToken);
        //    }
        //}


        /// <summary>
        /// middleware, adding chat memory support to the IChatClient instance
        /// </summary>
        public class ChatHistoryMiddleware : DelegatingChatClient
        {
            private readonly List<ChatMessage> _history = [];
            private readonly int _maxHistoryItems;

            public ChatHistoryMiddleware(IChatClient innerClient, int maxHistoryItems = 10)
                : base(innerClient) { _maxHistoryItems = maxHistoryItems; }

            public override async Task<ChatResponse> GetResponseAsync(
                IEnumerable<ChatMessage> messages,
                ChatOptions? options = null,
                CancellationToken cancellationToken = default)
            {
                // 1. Se la richiesta corrente ha un System Message, lo trattiamo come "Root"
                var systemMessage = messages.FirstOrDefault(m => m.Role == ChatRole.System);
                var userMessages = messages.Where(m => m.Role != ChatRole.System).ToList();

                // 2. Prepara il payload completo: [System Message] + [Storico Chat] + [Input Corrente]
                var payload = new List<ChatMessage>();
                if (systemMessage != null) payload.Add(systemMessage);
                payload.AddRange(_history);
                payload.AddRange(userMessages);

                // 3. Ottieni la risposta
                var response = await base.GetResponseAsync(payload, options, cancellationToken);

                // 4. Salva nello storico (input utente + output assistente)
                _history.AddRange(userMessages);
                _history.AddRange(response.Messages);

                // 5. Limita la cronologia entro i limiti
                if (_history.Count > _maxHistoryItems)
                {
                    _history.RemoveRange(0, _history.Count - _maxHistoryItems);
                }

                return response;
            }
        }
    }
}