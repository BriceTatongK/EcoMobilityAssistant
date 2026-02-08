using System.ComponentModel.DataAnnotations;

namespace EcoMob.Infra.Settings
{
    public class McpSettings
    {
        [Required]
        public string TransportType { get; init; } = "stdio";

        public string? SseUrl { get; init; }

        public string? ServerPath { get; init; }

        [Required]
        public string OpenAIKey { get; init; } = string.Empty;

        [Required]
        public bool UseLocalModel { get; init; } = false;

        public string? LocalModelUrl { get; init; }

        [Required]
        public string ModelName { get; init; } = "gpt-4o-mini";

        [Required]
        public int MaxChatMemoryItems { get; init; } = 5;
    }
}