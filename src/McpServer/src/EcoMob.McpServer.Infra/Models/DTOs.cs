using Newtonsoft.Json;

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
        [JsonProperty("scode")] public string? StationCode { get; set; }
        [JsonProperty("sname")] public string? StationName { get; set; }
        [JsonProperty("sactive")] public bool? IsActive { get; set; }
        [JsonProperty("savailable")] public bool? IsAvailable { get; set; }
        [JsonProperty("scoordinate")] public CoordinateDto? Coordinate { get; set; }
        [JsonProperty("sorigin")] public string? Origin { get; set; }


        // Measurement / "Latest" Fields
        [JsonProperty("_timestamp")] public DateTime? LastUpdate { get; set; }
        [JsonProperty("mvalue")] public double? CurrentValue { get; set; }
        [JsonProperty("tname")] public string? MeasurementType { get; set; }


        // Parent info (if applicable)
        [JsonProperty("pcode")] public string? ParentCode { get; set; }
    }


    // 1. Bicycles & Sharing
    public class BicycleStationDto : BaseMobilityDto
    {
        [JsonProperty("smetadata")] public BicycleMeta? Metadata { get; set; }
    }

    public class BicycleMeta
    {
        public bool? Electric { get; set; }
        public string? Type { get; set; }
    }

    // 2. Parking & Car Sharing (Handles items 2, 5, and 7)
    public class ParkingStationDto : BaseMobilityDto
    {
        [JsonProperty("smetadata")] public ParkingMeta? Metadata { get; set; }
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
        [JsonProperty("smetadata")] public ChargingMetaDto? Metadata { get; set; }
    }

    public class ChargingMetaDto
    {
        public string? State { get; set; }
        public int? Capacity { get; set; }
        public int? TotalBays { get; set; }
        public string? Address { get; set; }

        public List<OutletDto>? Outlets { get; set; }
    }

    public class OutletDto
    {
        public string? Id { get; set; }
        public int? MaxPower { get; set; }
        public int? MaxCurrent { get; set; }
        public int? MinCurrent { get; set; }
        public bool? HasFixedCable { get; set; }
        public string? OutletTypeCode { get; set; }
    }


    public class NetexParkingDto
    {
        public string? Type { get; set; }
        public string? Layout { get; set; }
        public bool? Charging { get; set; }
        public bool? Surveillance { get; set; }
        public string? Reservation { get; set; }
        public string? VehicleTypes { get; set; }

        [JsonProperty("hazard_prohibited")]
        public bool? HazardProhibited { get; set; }
    }
}
