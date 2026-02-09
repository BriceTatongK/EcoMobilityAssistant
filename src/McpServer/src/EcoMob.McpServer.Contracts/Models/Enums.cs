using System.ComponentModel;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace EcoMob.McpServer.Contracts.Models
{
    //public enum ContentType
    //{
    //    EBike,
    //    ParkingStation,
    //    EChargingStation
    //}

    /// <summary>
    /// 
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StationType
    {
        [Display(Name = "*")]
        [Description("all station types")]
        All,

        [Display(Name = "Bicycle")]
        [Description("bicycle stations")]
        Bicycle,

        [Display(Name = "BikeParking")]
        [Description("bike parking stations")]
        BikeParking,

        [Display(Name = "BIKE_CHARGER")]
        [Description("bike charger stations")]
        BikeCharger,

        [Display(Name = "ParkingStation")]
        [Description("parking stations")]
        ParkingStation,

        [Display(Name = "EChargingStation")]
        [Description("e charging stations")]
        EChargingStation,

        [Display(Name = "BikesharingStation")]
        [Description("bike sharing stations")]
        BikesharingStation
    }

    /// <summary>
    /// 
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DataType
    {
        [Display(Name = "*")]
        [Description("all the data types")]
        All,

        [Display(Name = "metadata")]
        [Description("station metadata")]
        Metadata,

        [Display(Name = "occupied")]
        [Description("real-time occupancy data")]
        Occupied
    }




    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DataLanguage
    {
        [Description("english")]
        en,
        [Description("german")]
        de,
        [Description("french")]
        fr,
        [Description("italian")]
        it
    }
}
