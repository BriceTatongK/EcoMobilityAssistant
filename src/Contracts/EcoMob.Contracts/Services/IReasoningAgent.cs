using EcoMob.Contracts.Models;

namespace EcoMob.Contracts.Services
{
    public interface IReasoningAgent
    {
        Task<string> ProcessAsync(
            IntentContext intent,
            string userPrompt,
            CancellationToken ct = default);
    }
}
