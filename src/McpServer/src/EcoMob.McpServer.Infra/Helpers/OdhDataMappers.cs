using EcoMob.McpServer.Contracts.Models;
using EcoMob.McpServer.Infra.Models;

namespace EcoMob.McpServer.Infra.Helpers
{
    public static class MobilityMapper
    {
        public static BaseMobilityStation ToDomain(BaseMobilityDto dto)
        {
            if (dto == null)
            {
                return null!;
            }

            return dto switch
            {
                BicycleStationDto b => new BicycleStation(
                    b.StationCode,
                    b.StationName,
                    b.IsActive,
                    b.IsAvailable,
                    new Coordinate(b.Coordinate.X, b.Coordinate.Y, b.Coordinate.Srid),
                    b.Origin,
                    b.LastUpdate,
                    b.CurrentValue,
                    b.MeasurementType,
                    b.ParentCode,
                    new Contracts.Models.BicycleMeta(b.Metadata.Electric, b.Metadata.Type)
                ),

                ChargingStationDto c => new ChargingStation(
                    c.StationCode,
                    c.StationName,
                    c.IsActive,
                    c.IsAvailable,
                    new Coordinate(c.Coordinate.X, c.Coordinate.Y, c.Coordinate.Srid),
                    c.Origin,
                    c.LastUpdate,
                    c.CurrentValue,
                    c.MeasurementType,
                    c.ParentCode,
                    new ChargingMeta(
                        c.Metadata.State,
                        c.Metadata.Capacity,
                        c.Metadata.TotalBays,
                        c.Metadata.Address)
                ),

                ParkingStationDto p => new ParkingFacility(
                    p.StationCode,
                    p.StationName,
                    p.IsActive,
                    p.IsAvailable,
                    new Coordinate(p.Coordinate.X, p.Coordinate.Y, p.Coordinate.Srid),
                    p.Origin,
                    p.LastUpdate,
                    p.CurrentValue,
                    p.MeasurementType,
                    p.ParentCode,
                    new Contracts.Models.ParkingMeta(
                        p.Metadata.TotalPlaces,
                        p.Metadata.Capacity,
                        new Multilingual(p.Metadata.Names.De, p.Metadata.Names.En, p.Metadata.Names.It),
                        new NetexParking(
                            p.Metadata.NetexParking.Type,
                            p.Metadata.NetexParking.Layout,
                            p.Metadata.NetexParking.Charging,
                            p.Metadata.NetexParking.Reservation,
                            p.Metadata.NetexParking.Surveillance,
                            p.Metadata.NetexParking.VehicleTypes,
                            p.Metadata.NetexParking.HazardProhibited),
                        p.Metadata.Company,
                        p.Metadata.BookAhead
                        )
                ),

                _ => throw new NotSupportedException($"SType '{dto.GetType().Name}' is not supported.")
            };
        }
    }
}
