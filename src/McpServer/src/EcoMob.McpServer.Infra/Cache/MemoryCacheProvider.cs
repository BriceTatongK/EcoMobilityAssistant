using EcoMob.McpServer.Contracts.DataProviders;
using Microsoft.Extensions.Caching.Memory;

namespace EcoMob.McpServer.Infra.Cache
{
    public class MemoryCacheProvider(IMemoryCache memoryCache) : ICacheProvider
    {
        private readonly IMemoryCache _memoryCache = memoryCache;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void Save(string key, object value)
        {
            _memoryCache.Set(key, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="ttl"></param>
        /// <returns></returns>
        public void Save(string key, object value, TimeSpan ttl)
        {
            _memoryCache.Set(key, value, ttl);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet<T>(string key, out T? value)
        {
            if (_memoryCache.TryGetValue(key, out var obj) && obj is T t)
            {
                value = t;
                return true;
            }

            value = default;

            return false;
        }
    }
}
