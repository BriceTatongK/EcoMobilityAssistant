using EcoMob.Contracts.Models;

namespace EcoMob.Contracts.Services
{
    public interface IIntentAgent
    {
        Task<IntentContext> ClassifyAsync(string userPrompt, CancellationToken ct = default);
    }
}