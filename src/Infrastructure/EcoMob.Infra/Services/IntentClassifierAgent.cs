using EcoMob.Infra.Models;
using EcoMob.Infra.Logging;
using EcoMob.Contracts.Enums;
using EcoMob.Contracts.Models;
using Microsoft.Extensions.AI;
using EcoMob.Contracts.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace EcoMob.Infra.Services
{
    public class IntentClassifierAgent : IIntentAgent, IDisposable

    {
        private readonly ILogger<IntentClassifierAgent> _logger;
        private readonly IChatClient _chatClient;

        public IntentClassifierAgent([FromKeyedServices(AgentRole.Intent)] IChatClient chatClient, ILogger<IntentClassifierAgent> logger)
        {
            _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="userPrompt"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<IntentContext> ClassifyAsync(string userPrompt, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userPrompt))
                return IntentContext.Unknown("Empty input");
            try
            {
                var systemPrompt = $$"""
                ROLE:
                You are an intent classification agent for an Eco Mobility Assistant.

                OBJECTIVE:
                Analyze the user's input and return a JSON object that matches EXACTLY the IntentContext schema.
                Your output will be consumed by downstream agents and MUST be machine-parseable.

                LANGUAGE DETECTION:
                - You MUST write classificationReasoning and guardrails in the SAME language as the user.
                - Detect the user's language from the user's input message only.
                - Respond in the user's language for user-facing explanations.
                - Always output structured JSON fields (type, extractedEntities, guardrails) in English.
                - Do not use Italian unless the user's input message is in Italian.
                

                INTENT CLASSIFICATION (CRITICAL):
                You MUST classify the intent into ONE AND ONLY ONE of the following values:

                "{{string.Join("\n", Enum.GetValues<IntentType>().Select(t => t.ToFormattedString()))}}"

                Constraints:
                - Case-sensitive
                - No spaces
                - No snake_case
                - No explanations
                - If the request does not clearly belong to any category, return "Unknown"

                ENTITY EXTRACTION:
                - Extract ONLY entities explicitly mentioned or clearly implied.
                - Supported entities include (when applicable):
                  - location
                  - radius / distance
                  - time / date
                  - stationType
                  - vehicleType
                  - constraints
                - If an entity is not present, OMIT it (do not use null or empty values).

                REASONING:
                - Provide a short, factual explanation of why this intent was chosen.
                - Do NOT include speculation or tool usage assumptions.

                GUARDRAILS:
                - Provide 1–3 concise behavioral rules the reasoning agent MUST respect.
                - Guardrails must be enforceable and task-specific.
                - Do NOT repeat general system rules.

                OUTPUT FORMAT (MANDATORY):
                - Return ONLY a valid JSON object matching IntentContext.
                - Do NOT include markdown, comments, or explanations.
                - Do NOT include schema metadata (e.g., "type": "object", "properties").
                - The response MUST start with "{" and end with "}".

                FORMAT EXAMPLE (STRUCTURE ONLY):
                {
                  "type": "Unknown",
                  "classificationReasoning": "The user's request does not clearly relate to eco mobility or lacks sufficient context.",
                  "extractedEntities": {},
                  "guardrails": [
                    "Do not make assumptions about user intent",
                    "Ask for clarification before proceeding"
                  ]
                },
                {
                  "type": "EcoTips",
                  "classificationReasoning": "The user is looking for advice on how to reduce their environmental impact while traveling.",
                  "extractedEntities": {
                    "context": "daily commuting",
                    "userProfile": "urban resident"
                  },
                  "guardrails": [
                    "Provide actionable and realistic suggestions",
                    "Avoid generic or non-applicable sustainability advice"
                  ]
                },
                 {
                  "type": "PlanRoute",
                  "classificationReasoning": "The user wants to plan an eco-friendly route between two locations.",
                  "extractedEntities": {
                    "origin": "Bolzano",
                    "destination": "University of Trento",
                    "preferredModes": ["train", "bike"]
                  },
                  "guardrails": [
                    "Prioritize low-emission transport modes",
                    "Avoid routes with unnecessary detours or transfers"
                  ]
                },
                 {
                  "type": "MobilityInfo",
                  "classificationReasoning": "The user is asking for information about eco mobility usage trends.",
                  "extractedEntities": {
                    "location": "South Tyrol",
                    "timePeriod": "last year",
                    "dataType": "public transport usage"
                  },
                  "guardrails": [
                    "Use verified and authoritative data sources",
                    "Clearly distinguish between historical data and estimates"
                  ]
                },
                 {
                  "type": "Comparisons",
                  "classificationReasoning": "The user wants to compare different eco-friendly transportation options.",
                  "extractedEntities": {
                    "origin": "Bolzano",
                    "destination": "Trento",
                    "comparisonCriteria": ["cost", "CO2 emissions", "travel time"]
                  },
                  "guardrails": [
                    "Use up-to-date comparison data",
                    "Highlight the most sustainable option clearly"
                  ]
                },
                  {
                  "type": "LocateChargingStation",
                  "classificationReasoning": "The user is looking for electric vehicle charging stations nearby.",
                  "extractedEntities": {
                    "location": "Bolzano",
                    "radius": "1km",
                    "connectorType": "Type 2"
                  },
                  "guardrails": [
                    "Only include operational charging stations",
                    "Prefer renewable-energy-powered stations when available"
                  ]
                },
                {
                  "type": "FindParking",
                  "classificationReasoning": "The user wants to find available parking near their destination.",
                  "extractedEntities": {
                    "location": "Bolzano city center",
                    "radius": "500m",
                    "vehicleType": "car"
                  },
                  "guardrails": [
                    "Do not suggest private or restricted parking",
                    "Prefer parking options with low environmental impact"
                  ]
                }

                GLOBAL RULES:
                - Always return valid JSON.
                - Never invent entities or constraints.
                - Be concise, deterministic, and consistent.
                - If unrelated to eco mobility, return type "Unknown".

                CLASSIFICATION EXAMPLES:
                User: "Where can I park near Unibz?"
                → type: FindParking

                User: "Plan a route using bike and charging stations"
                → type: PlanRoute

                User: "How many charging stations were active last week?"
                → type: MobilityInfo
                """;

                using var _ = _logger.BeginAgentScope(nameof(IntentClassifierAgent));

                var messages = new List<ChatMessage>
                {
                    new(ChatRole.System, systemPrompt),
                    new(ChatRole.User, userPrompt)
                };

                var response = await _chatClient.GetResponseAsync<IntentContext>(
                    messages,
                    new ChatOptions
                    {
                        Temperature = 0.0f,
                        ToolMode = ChatToolMode.None,
                        //ResponseFormat = ChatResponseFormat.ForJsonSchema<IntentContext>()
                    },
                    cancellationToken: ct);

                Console.WriteLine($"{response?.Text}");
                var result = response?.Result ?? IntentContext.Unknown("Model returned null");

                if (!Enum.IsDefined(result.Type))
                    return IntentContext.Unknown("Invalid intent returned");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Intent classification failed.");
                return IntentContext.Unknown($"Classification failure: {ex.Message}");
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
                    _logger.LogDebug("IntentClassifierAgent disposed its chat client.");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error while disposing chat client in IntentClassifierAgent.");
                }
            }
        }
    }
}
