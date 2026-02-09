using System.ComponentModel;
using McpDotNet.Protocol.Types;
using EcoMob.McpServer.Contracts.Models;
using EcoMob.McpServer.Contracts.Services;
using Microsoft.Extensions.DependencyInjection;

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
            [Description("Different station types to filter the stations.")] StationType station,
            [Description($"The language for the data.")] DataLanguage language)

        {
            //ILogger _logger = _provider.GetRequiredService<ILogger>();
            IEcoMobilityTimeseriesService _timeseriesService = _provider.GetRequiredService<IEcoMobilityTimeseriesService>();

            try
            {
                // The library calls this when the AI sends a CallToolRequest
                var stations = await _timeseriesService.GetStationsAsync(station, language);

                // Process the stations as needed
                return McpResponse.Ok(stations);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error fetching stations for type {StationType} and language {Language}", station, language);

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
            [Description("Different station types to filter the measurements.")] StationType station,
            [Description($"The data type for the measurements.")] DataType dataType)
        {
            //ILogger _logger = _provider.GetRequiredService<ILogger>();
            IEcoMobilityTimeseriesService _timeseriesService = _provider.GetRequiredService<IEcoMobilityTimeseriesService>();

            try
            {
                // The library calls this when the AI sends a CallToolRequest
                var latestMeasurements = await _timeseriesService.GetLatestMeasurementsAsync(station, dataType);

                // Process the latest measurements as needed
                return McpResponse.Ok(latestMeasurements);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error fetching latest measurements for type {StationType} and data type {DataType}", station, dataType);

                // Handle exceptions appropriately
                return McpResponse.Error(ex.Message);
            }
        }
    }
}