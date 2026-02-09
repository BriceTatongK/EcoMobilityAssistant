using EcoMob.McpServer.Contracts.Models;
using EcoMob.McpServer.Entry.Helpers;
using McpDotNet.Protocol.Types;
using System.ComponentModel;
using McpDotNet.Server;

namespace EcoMob.McpServer.Entry.McpTools
{
    /// <summary>
    /// 
    /// </summary>
    [McpToolType]
    public static class MobilityTools
    {
        [McpTool("/status")]
        public static async Task Status(HttpContent context)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="station"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        [McpTool("get_stations"), Description("Gets stations informations based on station type and language.")]
        public static async Task<CallToolResponse> GetStationsAsync(
        StationType station,
        DataLanguage language)
        {
            return await MobilityToolFacade.GetStationsAsync(station, language);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="station"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        [McpTool("get_latest_measurements"), Description("Gets latest measurements for a specific station type and data type.")]
        public static async Task<CallToolResponse> GetLatestMeasurementsAsync(
            StationType station,
            DataType dataType)
        {
            return await MobilityToolFacade.GetLatestMeasurementsAsync(station, dataType);
        }
    }
}
