using System.Text.Json.Serialization;

namespace EcoMob.McpServer.Infra.Models
{
    public class OdhResponse<T>
    {
        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        [JsonPropertyName("data")]
        public List<T> Data { get; set; } = [];
    }
}
