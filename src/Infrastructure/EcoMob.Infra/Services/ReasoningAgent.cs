using EcoMob.Contracts.Enums;
using EcoMob.Contracts.Models;
using EcoMob.Contracts.Services;
using EcoMob.Infra.Logging;
using EcoMob.Infra.Models;
using McpDotNet.Client;
using McpDotNet.Protocol.Types;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EcoMob.Infra.Services
{
    public class ReasoningAgent : IReasoningAgent, IDisposable
    {
        private readonly IMcpClient _mcpClient;
        private readonly IChatClient _chatClient;
        private readonly ILogger<ReasoningAgent> _logger;

        public ReasoningAgent(IMcpClient mcpClient,
            [FromKeyedServices(AgentRole.Reasoning)] IChatClient chatClient,
            ILogger<ReasoningAgent> logger)
        {
            _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
            _mcpClient = mcpClient ?? throw new ArgumentNullException(nameof(mcpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processes the user prompt, possibly invoking MCP tools and returning the model reply.
        /// </summary>
        public async Task<string> ProcessAsync(IntentContext intent,
            string userPrompt,
            CancellationToken ct = default)
        {
            if (intent.Type == IntentType.UNKNOWN || string.IsNullOrWhiteSpace(userPrompt))
            {
                using var _ = _logger.BeginAgentScope(nameof(ReasoningAgent));
                _logger.LogWarning("ProcessAsync called with empty prompt or an unknown intent.");
                return string.Empty;
            }

            try
            {
                using var _ = _logger.BeginAgentScope(nameof(ReasoningAgent));
                _logger.LogDebug("ProcessAsync: starting MCP handshake.");
                await _mcpClient.PingAsync(ct);

                _logger.LogDebug("ProcessAsync: enumerating MCP tools.");
                var mcpTools = new List<Tool>();
                await foreach (var tool in _mcpClient.ListToolsAsync(ct))
                {
                    mcpTools.Add(tool);
                }

                _logger.LogDebug("ProcessAsync: found {Count} MCP tools.", mcpTools.Count);

                var aiFunctions = mcpTools.Select(t => AIFunctionFactory.Create(
                    async (Dictionary<string, object?> args) =>
                    {
                        var result = await _mcpClient.CallToolAsync(t.Name, args!, ct);

                        return string.Join("\n", result.Content
                            .Where(c => c.Type == "text" && c.Text != null)
                            .Select(c => c.Text));
                    },

                    t.Name,
                    t.Description)).Cast<AITool>().ToList();

                _logger.LogDebug("ProcessAsync: invoking chat model with tools (count={Count}).", aiFunctions.Count);

                string intentDetails = intent.Type.ToFormattedString();
                string specificGuardrails = string.Join("\n- ", intent.Guardrails);

                string systemPrompt = $"""
                    ROLE:
                    You are the Reasoning Agent for EcoMobility Assistant.
                    Your current goal is: {intentDetails}

                    LANGUAGE DETECTION:
                    - Detect the language of the user's prompt.
                    - You MUST respond in the same language as the user.
                    - This applies to all synthesis and human-readable results.

                    REASONING CONTEXT:
                    - Why this intent was chosen: {intent.ClassificationReasoning}
                    - Extracted Entities: {JsonSerializer.Serialize(intent.ExtractedEntities)}

                    SPECIFIC GUARDRAILS FOR THIS TASK:
                    - {specificGuardrails}
                    - You MUST rely exclusively on the tools provided to fulfill the request.
                    - Do NOT use prior knowledge or make assumptions outside tool data.

                    FINAL OBJECTIVE:
                    Based on the goal and extracted entities above, select the most relevant tool, use it, and generate a concise, human-readable answer for the user.

                    GLOBAL RULES:
                    - Responses must be short, precise, and actionable.
                    - You MUST use at least one tool to answer the user.
                    - If no tool is available, respond exactly with: ""I have no tools to help with that.""
                    - If multiple tools are available, use the most relevant one.
                    - Do NOT fabricate names, distances, availability, or statuses.
                    - You CANNOT ask the user for additional information.
                    - You MUST always return an answer to the user.
                    - You MAY use icons to improve readability.

                    DATA ANALYSIS & RESPONSE GUIDELINES:
                    1. Synthesize, Don’t Dump  
                       - Never expose raw tool outputs (JSON, arrays, IDs).
                       - Extract only key information: name, availability/status, distance, price, eco-features.

                    2. Contextualize  
                       - Prioritize results that best match the ExtractedEntities.
                       - Rank results by proximity, availability, and eco-friendliness when applicable.

                    3. Graceful Error Handling  
                       - If the tool returns no results or an error:
                         - Explain briefly and clearly.
                         - Suggest the closest viable alternative.
                         - Example: ""Non ho trovato parcheggi a Milano Centro, ma ce ne sono 2 entro 1 km.""

                    4. Tone  
                       - Professional, helpful, and eco-conscious 🌱.

                    5. Format  
                       - Use Markdown.
                       - Bullet points, **bold highlights**, and minimal icons (📍🚗⚡🌿).

                    6. Conciseness  
                       - Keep responses very short and to the point.
                       - Prefer summaries over explanations.

                    7. Human-Readable Output  
                       - Always translate tool data into natural language.
                       - Example:
                         ""Ho trovato 3 parcheggi vicino a te:
                         - Parking A – 500 m · disponibile
                         - Parking B – 1 km · pochi posti
                         - Parking C – 2 km · colonnine EV ⚡""
                    """;


                var messages = new List<ChatMessage>
                {
                    new(ChatRole.System, systemPrompt),
                    new(ChatRole.User, userPrompt)
                };

                var response = await _chatClient.GetResponseAsync(messages, new ChatOptions
                {
                    Temperature = 0.5f,
                    Tools = aiFunctions,
                    ToolMode = ChatToolMode.Auto
                }, ct);

                _logger.LogInformation("ProcessAsync: model responded (length={Length}).", response?.ToString()?.Length ?? 0);
                return string.IsNullOrEmpty(response?.ToString())
                    ? "An error occurred while processing your request. Please try again later." : response.ToString();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ProcessAsync cancelled by caller.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing prompt in ReasoningAgent.");
                return "An error occurred while processing your request. Please try again later.";
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (_chatClient is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                    _logger.LogDebug("ReasoningAgent disposed its chat client.");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing chat client in ReasoningAgent.");
                }
            }
        }
    }
}
