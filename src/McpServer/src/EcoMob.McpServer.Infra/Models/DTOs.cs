using System.Text.Json.Serialization;

namespace EcoMob.McpServer.Infra.Models
{
    public class CoordinateDto
    {
        public double? X { get; set; }
        public double? Y { get; set; }
        public int? Srid { get; set; }
    }

    public class MultilingualDto
    {
        public string? De { get; set; }
        public string? En { get; set; }
        public string? It { get; set; }
    }

    public abstract class BaseMobilityDto
    {
        [JsonPropertyName("scode")] public string? StationCode { get; set; }
        [JsonPropertyName("sname")] public string? StationName { get; set; }
        [JsonPropertyName("sactive")] public bool? IsActive { get; set; }
        [JsonPropertyName("savailable")] public bool? IsAvailable { get; set; }
        [JsonPropertyName("scoordinate")] public CoordinateDto? Coordinate { get; set; }
        [JsonPropertyName("sorigin")] public string? Origin { get; set; }


        // Measurement / "Latest" Fields
        [JsonPropertyName("_timestamp")] public DateTime? LastUpdate { get; set; }
        [JsonPropertyName("mvalue")] public double? CurrentValue { get; set; }
        [JsonPropertyName("tname")] public string? MeasurementType { get; set; }


        // Parent info (if applicable)
        [JsonPropertyName("pcode")] public string? ParentCode { get; set; }
    }


    // 1. Bicycles & Sharing
    public class BicycleStationDto : BaseMobilityDto
    {
        [JsonPropertyName("smetadata")] public BicycleMeta? Metadata { get; set; }
    }

    public class BicycleMeta
    {
        public bool? Electric { get; set; }
        public string? Type { get; set; }
    }

    // 2. Parking & Car Sharing (Handles items 2, 5, and 7)
    public class ParkingStationDto : BaseMobilityDto
    {
        [JsonPropertyName("smetadata")] public ParkingMeta? Metadata { get; set; }
    }

    public class ParkingMeta
    {
        public int? TotalPlaces { get; set; }
        public int? Capacity { get; set; }
        public MultilingualDto? Names { get; set; }
        public NetexParkingDto? NetexParking { get; set; }

        // Car-sharing specific
        public object? Company { get; set; }
        public bool? BookAhead { get; set; }
    }


    // 3. Charging Stations (Handles items 3 and 6)
    public class ChargingStationDto : BaseMobilityDto
    {
        [JsonPropertyName("smetadata")] public ChargingMetaDto? Metadata { get; set; }
    }

    public class ChargingMetaDto
    {
        public string? State { get; set; }
        public int? Capacity { get; set; }
        public int? TotalBays { get; set; }
        public string? Address { get; set; }
    }

    public class NetexParkingDto
    {
        public string? Type { get; set; }
        public string? Layout { get; set; }
        public bool? Charging { get; set; }
        public bool? Surveillance { get; set; }
        public string? Reservation { get; set; }
        public string? VehicleTypes { get; set; }

        [JsonPropertyName("hazard_prohibited")]
        public bool? HazardProhibited { get; set; }
    }
}
