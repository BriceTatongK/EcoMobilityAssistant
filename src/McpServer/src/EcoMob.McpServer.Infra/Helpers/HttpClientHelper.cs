using Newtonsoft.Json;
namespace EcoMob.McpServer.Infra.Helpers
{
    public static class HttpClientNewtonsoftExtensions
    {
        public static async Task<T?> GetFromJsonNewtonsoftAsync<T>(
            this HttpClient http,
            string requestUri,
            JsonSerializerSettings settings,
            CancellationToken ct = default)
        {
            var json = await http.GetStringAsync(requestUri, ct);
            return JsonConvert.DeserializeObject<T>(json, settings);
        }
    }
}
