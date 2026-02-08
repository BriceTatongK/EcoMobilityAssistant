using EcoMob.Contracts.Models;

namespace EcoMob.Contracts.Services
{
    public interface IValidatorAgent
    {
        Task<ContextValidationResult> ValidateAsync(string prompt, CancellationToken ct = default);
    }
}
