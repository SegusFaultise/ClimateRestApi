#region Imports
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
#endregion

namespace CLIMATE_REST_API.Models
{
    #region Weather Model
    public class SensorDataModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [DefaultValue("")]
        public string? Id { get; set; }

        [BsonElement("Device Name")]
        public string Device { get; set; } = null!;

        [BsonElement("Precipitation mm/h")]
        public double? Precipitation_mm_h { get; set; }

        [BsonElement("Time")]
        public DateTime Time { get; set; }

        [BsonElement("Latitude")]
        public double? Latitude { get; set; }

        [BsonElement("Longitude")]
        public double? Longitude { get; set; }

        [BsonElement("Temperature (°C)")]
        public double? Temperature_C { get; set; }

        [BsonElement("Atmospheric Pressure (kPa)")]
        public double? AtmosphericPressure_kPa { get; set; }

        [BsonElement("Max Wind Speed (m/s)")]
        public double? MaxWindSpeed_ms { get; set; }

        [BsonElement("Solar Radiation (W/m2)")]
        public double? SolarRadiation_Wm2 { get; set; }

        [BsonElement("Vapor Pressure (kPa)")]
        public double? VaporPressure_kPa { get; set; }

        [BsonElement("Humidity (%)")]
        public double? Humidity_percentage { get; set; }

        [BsonElement("Wind Direction (°)")]
        public double? WindDirection { get; set; }
    }
    #endregion
}