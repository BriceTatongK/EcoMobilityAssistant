using EcoMob.McpServer.Contracts.DataProviders;
using EcoMob.McpServer.Contracts.Services;
using EcoMob.McpServer.Contracts.Models;
using EcoMob.McpServer.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace EcoMob.McpServer.Core.Services
{
    public class EcoMobilityTimeseriesService(
        IEcoMobilityTimeseriesClient timeseriesClient,
        ILogger<EcoMobilityTimeseriesService> logger,
        ICacheProvider cache) : IEcoMobilityTimeseriesService
    {
        private readonly ICacheProvider _cache = cache;
        private readonly ILogger<EcoMobilityTimeseriesService> _logger = logger;
        private readonly IEcoMobilityTimeseriesClient _timeseriesClient = timeseriesClient;

        public async Task<IReadOnlyList<BaseMobilityStation>> GetHistoricalDataAsync(StationType stationType, DataType dataType, DateTime from, DateTime to, IReadOnlyDictionary<string, string> parameters)
        {
            // prepare cache key and check cache
            var cacheKeyParams = new Dictionary<string, string>
                {
                    { "type", stationType.ToString() },
                    { "data", dataType.ToString() },
                    { "from", from.ToString("o") },
                    { "to", to.ToString("o") }
                };

            if (_cache.TryGet<IReadOnlyList<BaseMobilityStation>>(CacheHelper.GenerateCacheKey(Globals.HISTORICAL_CACHE_SUFFIX, cacheKeyParams), out var cachedData))
            {
                _logger.LogInformation("Cache hit for {Type} ({Data}) from {From} to {To}", stationType, dataType, from, to);
                return cachedData!;
            }

            // historical data is not cached, we directly call the client
            try
            {
                _logger.LogDebug("Start fetching historical data for {Type} from {From} to {To}...", stationType, from, to);
                // Chiamata al Client Infrastrutturale
                var response = await _timeseriesClient.GetHistoricalDataAsync(stationType, dataType, from, to, parameters);
                var finalResult = response
                                          .ToList()
                                            .AsReadOnly();
                // check result
                _logger.LogInformation("Result count: {0}", finalResult.Count);
                return finalResult;
            }
            catch (MobilityApiClientFailedException ex)
            {
                _logger.LogError(ex, "API Failure for {Type}", stationType);
                throw new MobilityServiceFailedException("Failed to retrieve mobility data from source.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error {Type}", stationType);
                throw new MobilityServiceFailedException("Internal processing error.", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stationType"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        /// <exception cref="MobilityServiceFailedException"></exception>
        public async Task<IReadOnlyList<BaseMobilityStation>> GetLatestMeasurementsAsync(StationType stationType, DataType dataType)
        {
            // 2. Controllo Cache
            var cacheKeyParams = new Dictionary<string, string>
            {
                { "type", stationType.ToString() },
                { "data", dataType.ToString() }
            };
            string cacheKey = CacheHelper.GenerateCacheKey(Globals.LATEST_CACHE_SUFFIX, cacheKeyParams);

            if (_cache.TryGet<IReadOnlyList<BaseMobilityStation>>(cacheKey, out var cachedData))
            {
                _logger.LogInformation("Cache hit for {Type} ({Data})", stationType, dataType);
                return cachedData!;
            }

            try
            {
                _logger.LogDebug("Start fetching real time data for {Type}...", stationType);

                // Chiamata al Client Infrastrutturale
                var response = await _timeseriesClient.GetLatestMeasurementsAsync(stationType, dataType);
                var finalResult = response
                                          .ToList()
                                            .AsReadOnly();

                // 4. Salvataggio in Cache (se abbiamo dati)
                if (finalResult.Count > 0)
                {
                    _cache.Save(cacheKey, finalResult, TimeSpan.FromMinutes(Globals.LATEST_CACHE_EXPIRATION_MINUTES));
                }

                // check result
                _logger.LogInformation("Result count: {0}", finalResult.Count);
                return finalResult;

            }
            catch (MobilityApiClientFailedException ex)
            {
                _logger.LogError(ex, "API Failure for {Type}", stationType);
                throw new MobilityServiceFailedException("Failed to retrieve mobility data from source.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error {Type}", stationType);
                throw new MobilityServiceFailedException("Internal processing error.", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="station"></param>
        /// <param name="language"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        /// <exception cref="MobilityServiceFailedException"></exception>
        public async Task<IReadOnlyList<BaseMobilityStation>> GetStationsAsync(
            StationType station,
            DataLanguage language = DataLanguage.en,
            int pageSize = 500)
        {
            // 2. Controllo Cache
            var cacheKeyParams = new Dictionary<string, string>
            {
                { "type", station.ToString() },
                { "lang", language.ToString() }
            };
            string cacheKey = CacheHelper.GenerateCacheKey(Globals.STATIONS_CACHE_PREFIX, cacheKeyParams);

            if (_cache.TryGet<IReadOnlyList<BaseMobilityStation>>(cacheKey, out var cachedData))
            {
                _logger.LogInformation("Cache hit for {Type} ({Lang})", station, language);
                return cachedData!;
            }

            try
            {
                _logger.LogDebug("Start fetching data for {Type}...", station);

                var queryParams = new Dictionary<string, string>
                    {
                        { "language", language.ToString() },
                        { "limit", pageSize.ToString() }
                    };

                // Chiamata al Client Infrastrutturale
                var response = await _timeseriesClient.GetStationsAsync(station, queryParams);

                var finalResult = response.Items
                                          .ToList()
                                        .AsReadOnly();

                //. cache save
                if (finalResult.Count > 0)
                {
                    _cache.Save(cacheKey, finalResult, TimeSpan.FromMinutes(Globals.STATIONS_CACHE_EXPIRATION_MINUTES));
                }

                // check result
                _logger.LogInformation("Result count: {0}", finalResult.Count);
                return finalResult;

            }
            catch (MobilityApiClientFailedException ex)
            {
                _logger.LogError(ex, "API Failure during pagination for {Type}", station);
                throw new MobilityServiceFailedException("Failed to retrieve mobility data from source.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error {Type}", station);
                throw new MobilityServiceFailedException("Internal processing error.", ex);
            }
        }
    }
}
