using Newtonsoft.Json;

namespace EcoMob.McpServer.Infra.Models
{
    public class OdhResponse<T>
    {
        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("data")]
        public List<T> Data { get; set; } = new List<T>();
    }
}
