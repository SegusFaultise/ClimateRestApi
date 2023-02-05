#region Imports
using Microsoft.AspNetCore.Mvc;
using CLIMATE_REST_API.Services;
using CLIMATE_REST_API.Models;
using Microsoft.AspNetCore.Cors;
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

        #region Http Get A Weather Device
        [EnableCors]
        [HttpGet]
        [Route("GetMaxPrecipitation")]
        public async Task<IResult> GetWeatherDevice(string device)
        {
            await _mongodbServices.GetMaximumPrecipitaionAsync(device);
            return Results.Ok();
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
