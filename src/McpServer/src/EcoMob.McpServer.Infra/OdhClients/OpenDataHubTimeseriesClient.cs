using EcoMob.McpServer.Contracts.DataProviders;
using Microsoft.AspNetCore.WebUtilities;
using EcoMob.McpServer.Contracts.Models;
using EcoMob.McpServer.Infra.Helpers;
using EcoMob.McpServer.Infra.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EcoMob.McpServer.Infra.OdhClients
{
    public class OdhTimeseriesClient(HttpClient httpClient, ILogger<OdhTimeseriesClient> logger) : IEcoMobilityTimeseriesClient
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<OdhTimeseriesClient> _logger = logger;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stationType"></param>
        /// <param name="dataType"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IReadOnlyList<BaseMobilityStation>> GetHistoricalDataAsync(StationType stationType,
            DataType dataType,
            DateTime from,
            DateTime to,
            IReadOnlyDictionary<string, string> parameters)
        {
            // Add pagination to existing parameters
            var queryParams = new Dictionary<string, string>(parameters)
            {
                { "limit", "200" },
                { "offset", "300" }
            };

            string uri = string.Empty;

            try
            {
                var fromDate = from.ToString("yyyy-MM-ddThh:mm:ss.SSS");
                var toDate = to.ToString("yyyy-MM-ddThh:mm:ss.SSS");

                // build request uri
                uri = QueryHelpers.AddQueryString($"flat/{EnumHelper.GetDisplayName(stationType)}/{EnumHelper.GetDisplayName(dataType)}/{fromDate}/{toDate}", queryParams!);

                // exec request
                var response = await _httpClient.GetFromJsonNewtonsoftAsync<OdhResponse<BaseMobilityDto>>(uri, MobilitySerializer.GetOptions());

                if (response is null || response.Data.Count is 0)
                {
                    return Array.Empty<BaseMobilityStation>();
                }

                // mapp to domain
                return response
                    .Data
                    .Select(dto => MobilityMapper.ToDomain(dto))
                    .ToList()
                    .AsReadOnly();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error occurred while fetching {StationType} from {Uri}", stationType, uri);
                throw new MobilityApiClientFailedException($"Could not reach the mobility provider for {stationType}/{dataType}.", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogCritical(ex, "Data format mismatch for {StationType}. The API contract might have changed.", stationType);
                throw new MobilityApiClientFailedException("The data received from the provider was in an invalid format.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred in GetStationsAsync");
                throw new MobilityApiClientFailedException("An internal error occurred while processing mobility data.", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stationType"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        /// <exception cref="MobilityApiClientFailedException"></exception>
        public async Task<IReadOnlyList<BaseMobilityStation>> GetLatestMeasurementsAsync(StationType stationType,
            DataType dataType)
        {
            // build request uri
            string uri = $"flat/{EnumHelper.GetDisplayName(stationType)}/latest";

            try
            {
                // exec request
                var response = await _httpClient.GetFromJsonNewtonsoftAsync<OdhResponse<BaseMobilityDto>>(uri, MobilitySerializer.GetOptions());

                if (response is null || response.Data.Count is 0)
                {
                    return Array.Empty<BaseMobilityStation>();
                }

                // mapp to domain
                return response
                    .Data
                    .Select(dto => MobilityMapper.ToDomain(dto))
                    .ToList()
                    .AsReadOnly();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error occurred while fetching {StationType} from {Uri}", stationType, uri);
                throw new MobilityApiClientFailedException($"Could not reach the mobility provider for {stationType}/{dataType}.", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogCritical(ex, "Data format mismatch for {StationType}. The API contract might have changed.", stationType);
                throw new MobilityApiClientFailedException("The data received from the provider was in an invalid format.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred in GetStationsAsync");
                throw new MobilityApiClientFailedException("An internal error occurred while processing mobility data.", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stationType"></param>
        /// <param name="parameters"></param>
        /// <param name="limit"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        /// <exception cref="MobilityApiClientFailedException"></exception>
        public async Task<PagedResult<BaseMobilityStation>> GetStationsAsync(StationType stationType,
            IReadOnlyDictionary<string, string> parameters, int page = 2, int limit = 100)
        {
            // Add pagination to existing parameters
            var queryParams = new Dictionary<string, string>(parameters)
            {
                ["limit"] = limit.ToString(),
                ["offset"] = ((page - 1) * limit).ToString()
            };

            string uri = string.Empty;

            try
            {
                // build request uri
                uri = QueryHelpers.AddQueryString($"flat/{EnumHelper.GetDisplayName(stationType)}", queryParams!);

                // exec request
                var response = await _httpClient.GetFromJsonNewtonsoftAsync<OdhResponse<BaseMobilityDto>>(uri, MobilitySerializer.GetOptions());

                _logger.LogInformation("Successfully fetched data for {StationType} from {Uri} DATA: {data}", stationType, uri, response?.ToString());
                // mapp to domain
                var items = response?
                    .Data
                    .Where(dto => dto != null)
                    .Select(dto => MobilityMapper.ToDomain(dto))
                    .ToList() ?? [];

                return new PagedResult<BaseMobilityStation>(
                    PageSize: limit,
                    PageNumber: page,
                    TotalCount: items.Count,
                    Items: items.AsReadOnly());
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error occurred while fetching {StationType} from {Uri}", stationType, uri);
                throw new MobilityApiClientFailedException($"Could not reach the mobility provider for {stationType}.", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogCritical(ex, "Data format mismatch for {StationType}. The API contract might have changed.", stationType);
                throw new MobilityApiClientFailedException("The data received from the provider was in an invalid format.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred in GetStationsAsync");
                throw new MobilityApiClientFailedException("An internal error occurred while processing mobility data.", ex);
            }
        }
    }
}
