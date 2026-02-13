using EcoMob.McpServer.Tests.Helpers;
using EcoMob.McpServer.Infra.Helpers;
using EcoMob.McpServer.Infra.Models;

namespace EcoMob.McpServer.Tests.Infra
{
    public class OdhClientsTests
    {
        /// <summary>
        /// assert the deserialization of the ODH response with the custom converter, in order to check that
        /// the discriminator is correctly applied and the correct types are deserialized
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Discriminator_Serializer_Should_Instantiate_Correct_Types()
        {
            // Arrange
            var data = await GetOdhTestDataAsync();
            var httpClient = MockHelpers.MockHttpClient(responseContent: data);

            // Act
            var response = await httpClient
                .GetFromJsonNewtonsoftAsync<OdhResponse<BaseMobilityDto>>(
                    "https://mobility.api.opendatahub.com/v2/",
                    MobilitySerializer.GetOptions());

            // Assert
            Assert.NotNull(response);
            Assert.NotNull(response!.Data);
            Assert.NotEmpty(response.Data);

            // Charging Plug
            var charging = response.Data.First(x => x.StationCode == "00001-0");
            Assert.IsType<ChargingStationDto>(charging);

            var chargingTyped = (ChargingStationDto)charging;
            Assert.NotNull(chargingTyped.Metadata);
            Assert.NotNull(chargingTyped.Metadata!.Outlets);
            Assert.Single(chargingTyped.Metadata.Outlets);

            // Parking Station
            var parking = response.Data.First(x => x.StationCode == "105");
            Assert.IsType<ParkingStationDto>(parking);

            var parkingTyped = (ParkingStationDto)parking;
            Assert.NotNull(parkingTyped.Metadata);
            Assert.Equal(90, parkingTyped.Metadata!.Capacity);

            // Bike Parking
            var bikeParking = response.Data.First(x => x.StationCode == "2174");
            Assert.IsType<ParkingStationDto>(bikeParking);

            // Bicycle
            var bicycle = response.Data.First(x => x.StationCode == "bike118");
            Assert.IsType<BicycleStationDto>(bicycle);

            // Global integrity
            Assert.IsType<int>(response.Offset);
            Assert.IsType<int>(response.Limit);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetOdhTestDataAsync()
        {
            return await File.ReadAllTextAsync("TestData/odh-data.json");
        }
    }
}