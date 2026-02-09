using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EcoMob.Contracts.Enums
{
    /// <summary>
    /// Custom JSON converter for IntentType enum to handle unknown values gracefully.
    /// </summary>
    public class SafeIntentTypeConverter : JsonConverter<IntentType>
    {
        public override IntentType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var raw = reader.GetString();

            if (Enum.TryParse<IntentType>(raw, true, out var result))
                return result;

            return IntentType.UNKNOWN;
        }
        public override void Write(Utf8JsonWriter writer, IntentType value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString());
    }

    /// <summary>
    /// Extension methods for the IntentType enum.
    /// </summary>
    public static class IntentTypeExtensions
    {
        public static string ToFormattedString(this IntentType intent)
        {
            var field = intent.GetType().GetField(intent.ToString());

            // display
            var displayAttr = field?.GetCustomAttribute<DisplayAttribute>();
            string name = displayAttr?.Name ?? intent.ToString();

            // description
            var descAttr = field?.GetCustomAttribute<DescriptionAttribute>();
            string description = descAttr?.Description ?? string.Empty;

            return $"{name} → {description}";
        }
    }

    /// <summary>
    /// The supported functionalities are defined here, in order to be LLM supported.
    /// </summary>
    [JsonConverter(typeof(SafeIntentTypeConverter))]
    public enum IntentType
    {
        // [Uncategorized]
        [Display(Name = "UNKNOWN"), Description("if the request is unclear or unrelated to eco mobility")]
        UNKNOWN,

        // [Availability] - Finding real-time resources
        [Display(Name = "FINDPARKING"), Description("user wants parking availability or location")]
        FINDPARKING,

        [Display(Name = "LOCATECHARGINGSTATION"), Description("user wants EV charging points")]
        LOCATECHARGINGSTATION,

        // [Comparison] - Benchmarking options
        [Display(Name = "COMPARISONS"), Description("user compares transport, stations, or options")]
        COMPARISONS,

        // [Itinerary] - Planning routes/trips
        [Display(Name = "MOBILITYINFO"), Description("user asks for current or historical eco mobility data")]
        MOBILITYINFO,

        [Display(Name = "PLANROUTE"), Description("user wants route or trip planning")]
        PLANROUTE, // Added: for multi-modal trip planning

        // [Educational/Support]
        [Display(Name = "ECOTIPS"), Description("user asks for sustainability advice or eco education")]
        ECOTIPS
    }
}
