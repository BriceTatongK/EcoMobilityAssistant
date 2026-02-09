using EcoMob.McpServer.Contracts.Models;
namespace EcoMob.McpServer.Contracts.DataProviders
{
    public interface IEcoMobilityTimeseriesClient
    {
        public Task<IReadOnlyList<BaseMobilityStation>> GetHistoricalDataAsync(StationType stationType, DataType dataType,
            DateTime from, DateTime to, IReadOnlyDictionary<string, string> parameters);

        public Task<IReadOnlyList<BaseMobilityStation>> GetLatestMeasurementsAsync(StationType stationType, DataType dataType);

        public Task<PagedResult<BaseMobilityStation>> GetStationsAsync(StationType stationType,
          IReadOnlyDictionary<string, string> parameters, int page = 1, int limit = 200);
    }
}
