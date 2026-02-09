namespace EcoMob.McpServer.Contracts.Models
{
    public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }


    public record Coordinate(
    double? X,
    double? Y,
    int? Srid);

    public record Multilingual(
        string? De,
        string? En,
        string? It);

    public record NetexParking(
        string? Type,
        string? Layout,
        bool? Charging,
        string? Reservation,
        bool? Surveillance,
        string? VehicleTypes,
        bool? HazardProhibited);


    // The "Base" Record
    public abstract record BaseMobilityStation(
        string? StationCode,
        string? StationName,
        bool? IsActive,
        bool? IsAvailable,
        Coordinate? Coordinate,
        string? Origin,
        DateTime? LastUpdate,
        double? CurrentValue,
        string? MeasurementType,
        string? ParentCode);

    // 1. Bicycles & Sharing
    public record BicycleMeta(bool? Electric, string? Type);

    public record BicycleStation(
        string? StationCode, string? StationName, bool? IsActive, bool? IsAvailable,
        Coordinate? Coordinate, string? Origin, DateTime? LastUpdate,
        double? CurrentValue, string? MeasurementType, string? ParentCode,
        BicycleMeta? Metadata
    ) : BaseMobilityStation(StationCode, StationName, IsActive, IsAvailable,
        Coordinate, Origin, LastUpdate, CurrentValue, MeasurementType, ParentCode);

    // 2. Parking & Car Sharing
    public record ParkingMeta(
        int? TotalPlaces,
        int? Capacity,
        Multilingual? Names,
        NetexParking? NetexParking,
        object? Company,
        bool? BookAhead);

    public record ParkingFacility(
        string? StationCode, string? StationName, bool? IsActive, bool? IsAvailable,
        Coordinate? Coordinate, string? Origin, DateTime? LastUpdate,
        double? CurrentValue, string? MeasurementType, string? ParentCode,
        ParkingMeta? Metadata
    ) : BaseMobilityStation(StationCode, StationName, IsActive, IsAvailable,
        Coordinate, Origin, LastUpdate, CurrentValue, MeasurementType, ParentCode);

    // 3. Charging Stations
    public record ChargingMeta(
        string? State,
        int? Capacity,
        int? TotalBays,
        string? Address
        );

    public record ChargingStation(
        string? StationCode, string? StationName, bool? IsActive, bool? IsAvailable,
        Coordinate? Coordinate, string? Origin, DateTime? LastUpdate,
        double? CurrentValue, string? MeasurementType, string? ParentCode,
        ChargingMeta? Metadata
    ) : BaseMobilityStation(StationCode, StationName, IsActive, IsAvailable,
        Coordinate, Origin, LastUpdate, CurrentValue, MeasurementType, ParentCode);
}
