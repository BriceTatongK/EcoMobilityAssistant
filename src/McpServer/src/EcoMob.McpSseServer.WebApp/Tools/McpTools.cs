using EcoMob.McpServer.Contracts.Models;
using EcoMob.McpServer.Entry.Helpers;
using McpDotNet.Protocol.Types;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace EcoMob.McpSseServer.WebApp.McpTools
{

    /// <summary>
    /// Provides tools for interacting with mobility data.
    /// </summary>
    public class MobilityTools
    {
        /// <summary>
        /// Gets stations information based on station type and language.
        /// </summary>
        /// </summary>
        /// <param name="station"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        [McpServerTool(Name = "get_stations"), Description("Gets stations informations based on station type and language.")]
        public static async Task<CallToolResponse> GetStationsAsync(
            [Description("The station type to get stations for.")] StationType station,
            [Description("The language for the station data.")] DataLanguage language)
        {
            return await MobilityToolFacade.GetStationsAsync(station, language);
        }


        /// <summary>
        /// Gets latest measurements for a specific station type and data type.
        /// </summary>
        /// <param name="station"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        [McpServerTool(Name = "get_latest_measurements"), Description("Gets latest measurements for a specific station type and data type.")]
        public static async Task<CallToolResponse> GetLatestMeasurementsAsync(
            [Description("The station type to get latest measurements for.")] StationType station,
            [Description("The data type for the latest measurements.")] DataType dataType)
        {
            return await MobilityToolFacade.GetLatestMeasurementsAsync(station, dataType);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="stationType"></param>
        /// <param name="dataType"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [McpServerTool(Name = "get_historical_data"), Description("Gets historical data for a specific station type, data type and time range.")]
        public static async Task<CallToolResponse> GetHistoricalDataAsync(
            [Description("The station type to get historical data for.")] StationType stationType,
            [Description("The data type for the historical data.")] DataType dataType,
            [Description("The start date and time for the historical data range.")] DateTime from,
            [Description("The end date and time for the historical data range.")] DateTime to,
            [Description("Additional parameters for the historical data request.")] Dictionary<string, string> parameters)
        {
            return await MobilityToolFacade.GetHistoricalDataAsync(stationType, dataType, from, to, parameters);
        }

        /// <summary>
        /// Gets available stations from a list of stations.
        /// </summary>
        /// <param name="stations">The list of stations to check for availability.</param>
        /// <returns>A response containing the available stations.</returns>
        [McpServerTool(Name = "get_available_stations"), Description("Checks available stations.")]
        public static CallToolResponse GetAvailableStations(List<BaseMobilityStation> stations)
        {
            // validates input
            if (stations == null)
            {
                return McpResponse.Error("input not valid");
            }

            // if no stations are available, return an empty list
            if (stations.Count == 0)
            {
                return McpResponse.Ok(new List<BaseMobilityStation>());
            }

            // returns only the available stations:
            // logic : a station is considered available if its IsAvailable property is not null and is true
            var availableStations = stations
                .Where(s => s.IsAvailable != null && s.IsAvailable == true)
                .ToList()
                .AsReadOnly();

            return McpResponse.Ok(availableStations);
        }


        /// <summary>
        /// Gets active stations from a list of stations.
        /// </summary>
        /// <param name="stations"></param>
        /// <returns></returns>
        [McpServerTool(Name = "get_active_stations"), Description("Gets active stations.")]
        public static CallToolResponse GetActiveStations(List<BaseMobilityStation> stations)
        {
            // validates input
            if (stations == null)
            {
                return McpResponse.Error("input not valid");
            }

            // if no stations are available, return an empty list
            if (stations.Count == 0)
            {
                return McpResponse.Ok(new List<BaseMobilityStation>());
            }

            // returns only the active stations:
            // logic : a station is considered active if its IsActive property is not null and is true
            var activeStations = stations
                .Where(s => s.IsActive != null && s.IsActive == true)
                .ToList()
                .AsReadOnly();

            return McpResponse.Ok(activeStations);
        }
    }
}
