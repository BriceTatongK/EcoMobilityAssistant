using EcoMob.McpServer.Contracts.Models;
namespace EcoMob.McpServer.Contracts.Services
{
    public interface IEcoMobilityTimeseriesService
    {
        public Task<IReadOnlyList<BaseMobilityStation>> GetStationsAsync(StationType station,
            DataLanguage language = DataLanguage.en,
            int pageSize = 500);

        public Task<IReadOnlyList<BaseMobilityStation>> GetHistoricalDataAsync(StationType stationType, DataType dataType,
            DateTime from, DateTime to, IReadOnlyDictionary<string, string> parameters);

        public Task<IReadOnlyList<BaseMobilityStation>> GetLatestMeasurementsAsync(StationType stationType, DataType dataType);
    }
}
