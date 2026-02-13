using EcoMob.Contracts.Models;
using EcoMob.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace EcoMob.Core.Services
{
    public class EcoMobilityService(
        IIntentAgent intentAgent,
        IReasoningAgent reasoningAgent,
        ILogger<EcoMobilityService> logger)
    {
        private readonly IIntentAgent _intentAgnet = intentAgent;
        private readonly ILogger<EcoMobilityService> _logger = logger;
        private readonly IReasoningAgent _reasoningAgent = reasoningAgent;

        /// <summary>
        /// core logic to handle the user requests
        /// </summary>
        /// <param name="userInput"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> HandleUserRequestAsync(string userInput, CancellationToken ct = default)
        {
            try
            {
                // 1. Classify intent
                IntentContext intent = await _intentAgnet.ClassifyAsync(userInput, ct);

                _logger.LogInformation("Classified intent: {IntentType} - Reasoning: {ClassificationReasoning}",
                    intent.Type, intent.ClassificationReasoning);

                if (intent.Type is Contracts.Enums.IntentType.UNKNOWN)
                {
                    return "Request not supported yet !";
                }

                // 2. Process request based on intent
                var response = await _reasoningAgent.ProcessAsync(intent, userInput, ct);

                return response ?? "Something wrong happened, retry later !";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while handling user request");
                throw new Exception($"Error while handling user request: {ex.Message}");
            }
        }
    }
}
