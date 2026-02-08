using EcoMob.Contracts.Models;
using EcoMob.Contracts.Services;
using EcoMob.Infra.Logging;
using EcoMob.Infra.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EcoMob.Infra.Services
{
    public class ContextValidatorAgent([FromKeyedServices(AgentRole.Validator)] IChatClient chatClient, ILogger<ContextValidatorAgent> logger) : IValidatorAgent
    {
        private readonly IChatClient _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
        private readonly ILogger<ContextValidatorAgent> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// validates the context
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<ContextValidationResult> ValidateAsync(string prompt, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                using var _ = _logger.BeginAgentScope(nameof(ContextValidatorAgent));
                _logger.LogWarning("ValidateAsync called with empty prompt.");
                return new ContextValidationResult(false, "empty prompt");
            }

            var systemPrompt = """
            ROLE:
            You are a security and relevance filter for an Eco-Mobility application.

            LANGUAGE DETECTION:
            - Detect the language of the user's input.
            - You MUST write the "reason" field in the SAME language as the user.

            TASK:
            Determine whether the user's input is relevant to eco-mobility topics, including:
            - Transportation and mobility
            - Traffic and road conditions
            - Parking and bookings
            - Public transport
            - Electric vehicles and charging stations
            - Sustainable or low-emission mobility

            DECISION RULES:
            - Return isValid = true ONLY if the request clearly relates to eco-mobility.
            - Return isValid = false if the request is unrelated, harmful, ambiguous, or off-topic.
            - When in doubt, prefer false.

            OUTPUT FORMAT (MANDATORY):
            - Respond EXCLUSIVELY with a single JSON object.
            - The JSON MUST follow this exact structure:
              {"isValid": true|false, "reason": "short explanation"}
            - Do NOT include markdown, comments, or extra text.
            - The response MUST start with "{" and end with "}".

            CONSTRAINTS:
            - The "reason" must be concise, factual, and human-readable.
            - Do NOT invent intent or assume context.
            - Do NOT include line breaks or additional fields.
            """;

            // llm prompt
            var messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, systemPrompt),
                new ChatMessage(ChatRole.User, prompt)
            };

            try
            {
                using var _ = _logger.BeginAgentScope(nameof(ContextValidatorAgent));
                _logger.LogDebug("ValidateAsync: sending prompt for validation (length={Length}).", prompt.Length);

                // llm prompting
                var response = await _chatClient.GetResponseAsync<ContextValidationResult>(messages, cancellationToken: ct);

                var result = response?.Result ?? new ContextValidationResult(false, "validation error.");
                _logger.LogDebug("ValidateAsync: validation result: isValid={IsValid}.", result.IsValid);
                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ValidateAsync cancelled by caller.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during context validation.");
                throw new Exception($"Error during context validation: {ex.Message}");
            }
        }
    }
}
