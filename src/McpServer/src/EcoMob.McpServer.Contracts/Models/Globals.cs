namespace EcoMob.McpServer.Contracts.Models
{
    public static class Globals
    {
        public const string STATIONS_CACHE_PREFIX = "stations";
        public const string LATEST_CACHE_SUFFIX = "latest";
        public const string HISTORICAL_CACHE_SUFFIX = "historical";


        public const int STATIONS_CACHE_EXPIRATION_MINUTES = 60;
        public const int LATEST_CACHE_EXPIRATION_MINUTES = 5;
        public const int HISTORICAL_CACHE_EXPIRATION_MINUTES = 1440; // 24h
    }
}
