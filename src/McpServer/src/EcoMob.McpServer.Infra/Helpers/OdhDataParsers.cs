using EcoMob.McpServer.Infra.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace EcoMob.McpServer.Infra.Helpers
{
    public static class MobilitySerializer
    {
        /// <summary>
        /// Gets the JSON serializer options for mobility data.
        /// </summary>
        /// <returns>The JSON serializer options.</returns>
        public static JsonSerializerSettings GetOptions()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Converters = { new OdhBaseMobilityConverter() },
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                DateParseHandling = DateParseHandling.None
            };
        }


        /// <summary>
        /// Custom JSON converter for BaseMobilityDto to handle different station types.
        /// </summary>
        public class OdhBaseMobilityConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(BaseMobilityDto);
            }

            public override object? ReadJson(
                JsonReader reader,
                Type objectType,
                object? existingValue,
                JsonSerializer serializer)
            {
                // Load JSON into JObject for inspection
                var jo = JObject.Load(reader);

                // Determine discriminator
                var stype = jo["stype"]?.ToString()?.ToLowerInvariant();

                Type? targetType = stype switch
                {
                    "bicycle" or "bicyclestationbay" => typeof(BicycleStationDto),

                    "parkingstation" => typeof(ParkingStationDto),

                    "bikeparking" => typeof(ParkingStationDto),

                    "echargingstation" => typeof(ChargingStationDto),

                    "echargingplug" => typeof(ChargingStationDto),

                    _ => typeof(BaseMobilityDto) // fallback SAFE
                };


                // IMPORTANT: create a serializer clone without this converter
                var innerSerializer = CreateInnerSerializer(serializer);

                return jo.ToObject(targetType, innerSerializer);
            }

            public override void WriteJson(
                JsonWriter writer,
                object? value,
                JsonSerializer serializer)
            {
                serializer.Serialize(writer, value);
            }

            private static JsonSerializer CreateInnerSerializer(JsonSerializer serializer)
            {
                var innerSerializer = new JsonSerializer
                {
                    ContractResolver = serializer.ContractResolver,
                    NullValueHandling = serializer.NullValueHandling,
                    DateParseHandling = serializer.DateParseHandling,
                    FloatParseHandling = serializer.FloatParseHandling,
                    Culture = serializer.Culture
                };

                // Copy converters except THIS one (avoid infinite recursion)
                foreach (var conv in serializer.Converters)
                {
                    if (conv is not OdhBaseMobilityConverter)
                        innerSerializer.Converters.Add(conv);
                }

                return innerSerializer;
            }
        }
    }
}
