using System.Text.Json;
using EcoMob.Infra.Models;
using EcoMob.Infra.Helpers;
using EcoMob.Infra.Logging;
using EcoMob.Contracts.Enums;
using EcoMob.Contracts.Models;
using EcoMob.Contracts.Services;
using ModelContextProtocol.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;

namespace EcoMob.Infra.Services
{
    // Helper class pour la sélection du tool
    internal class ToolSelection
    {
        public string tool { get; set; } = string.Empty;
        public Dictionary<string, object>? arguments { get; set; }
    }

    public class ReasoningAgent : IReasoningAgent, IDisposable
    {
        private readonly McpClient _mcpClient;
        private readonly IChatClient _chatClient;
        private readonly ILogger<ReasoningAgent> _logger;

        public ReasoningAgent(McpClient mcpClient,
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
                return string.Empty;

            using var _ = _logger.BeginAgentScope(nameof(ReasoningAgent));
            _logger.LogDebug("ProcessAsync: starting MCP handshake.");
            await _mcpClient.PingAsync(cancellationToken: ct);

            try
            {
                _logger.LogDebug("ProcessAsync: enumerating MCP tools.");
                var mcpTools = await _mcpClient.ListToolsAsync(cancellationToken: ct);

                _logger.LogDebug("ProcessAsync: found {Count} MCP tools.", mcpTools.Count);

                // Si aucun tool disponible
                if (!mcpTools.Any())
                {
                    return "I have no tools to help with that.";
                }

                // 1️⃣ Demande au modèle de choisir le tool et ses arguments
                string intentDetails = intent.Type.ToFormattedString();
                string specificGuardrails = string.Join("\n- ", intent.Guardrails);

                var toolsText = string.Join(
                    Environment.NewLine,
                    mcpTools.Select(ToolHelpers.FormatTool)
                );

                string format = @"{
                  ""tool"": ""tool_name"",
                  ""arguments"": {
                    ""param1"": ""value"",
                    ""param2"": ""value""
                  }
                }";

                var toolSelectionPrompt = $"""
                ROLE:
                You are the Reasoning Agent for EcoMobility Assistant.

                GOAL:
                {intentDetails}

                GUARDRAILS:
                - {specificGuardrails}

                USER REQUEST:
                {userPrompt}

                AVAILABLE TOOLS:
                {toolsText}

                INSTRUCTIONS:
                1. Select the single most appropriate tool.
                2. Carefully read its Input Schema.
                3. Provide ALL required parameters.
                4. Use only allowed enum values.
                5. Do not invent parameters.
                6. If a required value is missing from the user request, infer it cautiously.
                7. Output valid JSON only in this exact format:

                {format}

                Do NOT answer the user.
                Only output JSON.
                """;

                var selectionResponse = await _chatClient.GetResponseAsync(
                    new List<ChatMessage> { new(ChatRole.System, toolSelectionPrompt) },
                    new ChatOptions { Temperature = 0.2f },
                    ct
                );

                // Parse le JSON choisi par le modèle
                ToolSelection? selection = JsonSerializer.Deserialize<ToolSelection>(selectionResponse.ToString()!);

                if (selection == null || string.IsNullOrEmpty(selection.tool))
                {
                    return "The model did not select a valid tool.";
                }

                // 2️⃣ Appelle le tool MCP choisi
                // Sérialisation des arguments pour gérer enum, types natifs
                var toolArgs = new Dictionary<string, object?>();
                if (selection.arguments != null)
                {
                    foreach (var kvp in selection.arguments)
                    {
                        var value = kvp.Value;
                        // Enum → string
                        if (value != null && value.GetType().IsEnum)
                            value = value.ToString();
                        toolArgs[kvp.Key] = value;
                    }
                }

                _logger.LogDebug("Calling MCP tool {Tool} with arguments: {Args}", selection.tool, JsonSerializer.Serialize(toolArgs));

                var toolResult = await _mcpClient.CallToolAsync(selection.tool, toolArgs!, cancellationToken: ct);

                // Convertit le résultat MCP en texte lisible
                var toolText = string.Join("\n", toolResult.StructuredContent?.ToString());

                // 3️⃣ Prompt final pour synthèse humaine
                string finalPrompt = $"""
                    The user asked: {userPrompt}

                    Tool '{selection.tool}' returned the following data:
                    {toolText}

                    Based on this, generate a concise, human-readable answer.
                    - Follow the guardrails.
                    - Use markdown, bullet points, minimal icons.
                    - Keep it short and actionable.
                    """;

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
                         - Example: "“I couldn’t find any parking in Milan city center, but there are two options within 1 km.”"

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
                         “I found 3 parking options near you:
                         . Parking A – 500 m · available
                         . Parking B – 1 km · limited availability
                         . Parking C – 2 km · EV charging stations ⚡”
                    """;

                var finalResponse = await _chatClient.GetResponseAsync(
                    [
                        new(ChatRole.System, systemPrompt), // ton prompt système original
                        new(ChatRole.User, finalPrompt)
                    ],
                    new ChatOptions { Temperature = 0.5f },
                    ct
                );

                _logger.LogInformation("ProcessAsync: model responded (length={Length}).", finalResponse?.ToString()?.Length ?? 0);

                return string.IsNullOrEmpty(finalResponse?.ToString())
                    ? "An error occurred while processing your request. Please try again later."
                    : finalResponse.ToString();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ProcessAsync cancelled by caller.");
                throw;
            }
            catch (JsonException jex)
            {
                _logger.LogError(jex, "Error while trying to deserialise");
                throw new Exception($"Error while handling the request: {jex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing prompt in ReasoningAgent.");
                throw new Exception($"Error while handling the request: {ex.Message}");
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
