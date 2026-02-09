using System.Text;

namespace EcoMob.McpServer.Core.Helpers
{
    public static class CacheHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string GenerateCacheKey(string prefix, IDictionary<string, string> parameters)
        {
            var sortedParams = parameters.OrderBy(kv => kv.Key);
            var keyBuilder = new StringBuilder(prefix);

            foreach (var kv in sortedParams)
            {
                keyBuilder.Append($"|{kv.Key}:{kv.Value}");
            }

            return keyBuilder.ToString();
        }
    }
}
