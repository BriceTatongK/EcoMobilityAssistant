namespace EcoMob.Contracts.Models
{
    public record ContextValidationResult(bool IsValid, string? Reason = null);
}
