namespace EcoMob.McpServer.Contracts.DataProviders
{
    public interface ICacheProvider
    {
        public void Save(string key, object value);
        public bool TryGet<T>(string key, out T? value);
        public void Save(string key, object value, TimeSpan ttl);
    }
}
