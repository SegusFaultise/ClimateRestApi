#region Imports
using Microsoft.AspNetCore.Mvc;
using CLIMATE_REST_API.Services;
using CLIMATE_REST_API.Models;
using Microsoft.AspNetCore.Cors;
using MongoDB.Bson;
#endregion

namespace CLIMATE_DATA_BRAZIL.Controllers
{
    #region Controller & Route -Variables
    [Controller]
    [Route("~/api/[controller]")]
    #endregion

    #region Weather Controller
    public class WeatherController : Controller
    {
        #region region MongoDBServices Variable
        private readonly MongoDBServices _mongodbServices;
        #endregion

        #region Setting _mongodbServices To mongodbServices
        public WeatherController(MongoDBServices mongodbServices)
        {
            _mongodbServices = mongodbServices;
        }
        #endregion

        #region Http Get Weather
        [EnableCors]
        [HttpGet]
        public async Task<List<SensorDataModel>> GetWeather()
        {
            return await _mongodbServices.GetWeatherAsync();
        }
        #endregion

        #region Http Get Max Precipitation
        [EnableCors]
        [HttpGet]
        [Route("GetMaxPrecipitation")]
        public async Task<IResult> GetMaxPrecipitation(string device)
        {
           var result = await _mongodbServices.GetMaxPrecipitaionAsync(device);
            return Results.Text(result);
        }
        #endregion

        #region Http Get Fields Based On Time & Date Aysnc
        [EnableCors]
        [HttpGet]
        [Route("GetFieldsBasedOnTimeAndDate")]
        public async Task<IResult> GetFieldsBasedOnTimeAndDate(string device, DateTime date_time)
        {
            var result = await _mongodbServices.GetFieldsBasedOnTimeAndDateAsync(device, date_time);
            return Results.Text(result);
        }
        #endregion

        #region Http Get Max Tempreture Aysnc
        [EnableCors]
        [HttpGet]
        [Route("GetMaxTemp")]
        public async Task<IResult> GetMaxTemp(DateTime date_time_start, DateTime date_time_end)
        {
            var result = await _mongodbServices.GetMaxTempAsync(date_time_start, date_time_end);
            return Results.Ok(result);
        }
        #endregion

        #region Http Post Weather
        [EnableCors]
        [HttpPost]
        public async Task<IActionResult> PostWeather([FromBody]SensorDataModel weatherModel)
        {
            await _mongodbServices.CreateWeatherAsync(weatherModel);
            return CreatedAtAction(nameof(GetWeather), new { id = weatherModel.Id}, weatherModel);
        }
        #endregion

        //#region Http Post Multi Weather
        //[HttpPost]
        //[Route("PostMultiWeather")]
        //public async Task<IActionResult> PostMultiWeather([FromBody] SensorDataModel weatherModel)
        //{
        //    await _mongodbServices.CreateMultiWeatherAsync(weatherModel);
        //    return CreatedAtAction(nameof(GetWeather), new { id = weatherModel.Id }, weatherModel);
        //}
        //#endregion
    }
    #endregion
}
