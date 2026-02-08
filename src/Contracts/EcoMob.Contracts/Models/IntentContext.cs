using EcoMob.Contracts.Enums;
using System.Text.Json.Serialization;

namespace EcoMob.Contracts.Models
{
    public record IntentContext
    {
        // The core classification
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public IntentType Type { get; init; }

        // Why the classifier chose this (helps the reasoning agent avoid mistakes)
        public string ClassificationReasoning { get; init; } = string.Empty;

        // High-priority data extracted from the prompt (e.g., "Milan", "2 PM")
        public Dictionary<string, string> ExtractedEntities { get; init; } = [];

        // Specific rules the reasoning agent MUST follow for this specific intent
        public string[] Guardrails { get; init; } = [];

        public static IntentContext Unknown(string reason) => new()
        {
            ClassificationReasoning = reason,
            Type = IntentType.UNKNOWN,
            ExtractedEntities = [],
            Guardrails = []
        };
    }
}
