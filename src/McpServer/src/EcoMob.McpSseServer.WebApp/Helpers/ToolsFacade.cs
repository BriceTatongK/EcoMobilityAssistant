using McpDotNet.Protocol.Types;
using EcoMob.McpServer.Contracts.Models;
using EcoMob.McpServer.Contracts.Services;

namespace EcoMob.McpServer.Entry.Helpers
{
    public static class MobilityToolFacade
    {
        public static IServiceProvider _provider = default!;

        /// <summary>
        /// Gets a vehicle based on the specified station and language.
        /// </summary>
        /// <param name="station">The station type to get the vehicle from.</param>
        /// <param name="language">The language for the data.</param>
        /// <returns>A CallToolResult containing the vehicle information.</returns>
        public static async Task<CallToolResponse> GetStationsAsync(
            StationType station,
            DataLanguage language)

        {
            //ILogger _logger = _provider.GetRequiredService<ILogger>();
            IEcoMobilityTimeseriesService _timeseriesService = _provider.GetRequiredService<IEcoMobilityTimeseriesService>();
            ILogger _logger = _provider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(MobilityToolFacade));

            try
            {
                // The library calls this when the AI sends a CallToolRequest
                var stations = await _timeseriesService.GetStationsAsync(station, language);

                // Process the stations as needed
                return McpResponse.Ok(stations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stations for type {StationType} and language {Language}", station, language);

                // Handle exceptions appropriately
                return McpResponse.Error(ex.Message);
            }
        }

        /// <summary>
        /// Gets latest measurements for a specific station type and data type.
        /// </summary>
        /// <param name="station">The station type to get the measurements from.</param>
        /// <param name="dataType">The data type for the measurements.</param>
        /// <returns>A CallToolResult containing the latest measurements.</returns>
        public static async Task<CallToolResponse> GetLatestMeasurementsAsync(
            StationType station,
            DataType dataType)
        {
            IEcoMobilityTimeseriesService _timeseriesService = _provider.GetRequiredService<IEcoMobilityTimeseriesService>();
            ILogger _logger = _provider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(MobilityToolFacade));

            try
            {
                // The library calls this when the AI sends a CallToolRequest
                var latestMeasurements = await _timeseriesService.GetLatestMeasurementsAsync(station, dataType);

                // Process the latest measurements as needed
                return McpResponse.Ok(latestMeasurements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching latest measurements for type {StationType} and data type {DataType}", station, dataType);

                // Handle exceptions appropriately
                return McpResponse.Error(ex.Message);
            }
        }

        /// <summary>
        /// Gets historical data for a specific station type, data type, and time range.
        /// </summary>
        /// <param name="stationType"></param>
        /// <param name="dataType"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static async Task<CallToolResponse> GetHistoricalDataAsync(
            StationType stationType,
            DataType dataType,
            DateTime from,
            DateTime to,
            IReadOnlyDictionary<string, string> parameters)
        {
            IEcoMobilityTimeseriesService _timeseriesService = _provider.GetRequiredService<IEcoMobilityTimeseriesService>();
            ILogger _logger = _provider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(MobilityToolFacade));

            try
            {
                // The library calls this when the AI sends a CallToolRequest
                var historicalData = await _timeseriesService.GetHistoricalDataAsync(stationType, dataType, from, to, parameters);
                // Process the historical data as needed
                return McpResponse.Ok(historicalData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching historical data for station type {StationType}, data type {DataType}, from {From} to {To}", stationType, dataType, from, to);
                // Handle exceptions appropriately
                return McpResponse.Error(ex.Message);
            }
        }
    }
}