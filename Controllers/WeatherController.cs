#region Imports
using Microsoft.AspNetCore.Mvc;
using CLIMATE_REST_API.Services;
using CLIMATE_REST_API.Models;
using Microsoft.AspNetCore.Cors;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using System;
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
        [Route("GetAllSesnors")]
        public async Task<List<SensorDataModel>> GetWeather()
        {
            return await _mongodbServices.GetWeatherAsync();
        }
        #endregion

        #region Http Get Max Precipitation
        [EnableCors]
        [HttpGet]
        [Route("GetMaxPrecipitation")]
        public async Task<IActionResult> GetMaxPrecipitation(string device)
        {
            try
            {
                var result = await _mongodbServices.GetMaxPrecipitaionAsync(device);

                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return Problem(statusCode: 500);
                }
            }
            catch
            {
                return BadRequest("No entry found");
            }
        }
        #endregion

        #region Http Get Fields Based On Time & Date Aysnc
        [EnableCors("{}")]
        [HttpGet]
        [Route("GetFieldsBasedOnTimeAndDate")]
        public async Task<IActionResult> GetFieldsBasedOnTimeAndDate(string device, DateTime date_time)
        {
            try
            {
                var result = await _mongodbServices.GetFieldsBasedOnTimeAndDateAsync(device, date_time);

                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return Problem(statusCode: 500);
                }
            }
            catch
            {
                return BadRequest("No entry found");
            }
        }
        #endregion

        #region Http Get Max Tempreture Aysnc
        [EnableCors]
        [HttpGet]
        [Route("GetMaxTemp")]
        public async Task<IActionResult> GetMaxTemp(DateTime date_time_start, DateTime date_time_end)
        {
            try
            {
                var result = await _mongodbServices.GetMaxTempAsync(date_time_start, date_time_end);

                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return Problem(statusCode: 500);
                }
            }
            catch
            {
                return BadRequest("No entry found");
            }
        }
        #endregion

        #region Http Post Weather
        [EnableCors]
        [HttpPost]
        public async Task<IActionResult> PostWeatherAsync([FromBody]SensorDataModel weatherModel)
        {
            try
            {
                await _mongodbServices.CreateWeatherAsync(weatherModel);
                return CreatedAtAction(nameof(GetWeather), new { id = weatherModel.Id }, weatherModel);
            }
            catch
            {
                return BadRequest("Incorect details");
            }
        }
        #endregion

        #region Http Post Manny Weather
        [EnableCors]
        [HttpPost]
        [Route("PostMannyWeather")]
        public ActionResult PostMannyWeatherAsync([FromBody] List<SensorDataModel> weatherModel)
        {
            try
            {
                _mongodbServices.CreateMannyWeatherAsync(weatherModel);
                return CreatedAtAction(nameof(GetWeather), new { id = weatherModel }, weatherModel);
            }
            catch
            {
                return BadRequest("Incorect details");
            }
        }
        #endregion

        #region Http Post Weather
        [EnableCors]
        [HttpPost]
        [Route("PostWeatherToWebsite")]
        public async Task<IActionResult> PostWeatherToWebisteAsync(SensorDataModel weatherModel)
        {
            await _mongodbServices.CreateWeatherAsync(weatherModel);
            return CreatedAtAction(nameof(GetWeather), new { id = weatherModel.Id }, weatherModel);
        }
        #endregion

        #region Http Put Weather (Precipitation mm/h)
        [EnableCors]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePrecipitationAsync(string id, double precipitation_mm_h)
        {
            try
            {
                if (precipitation_mm_h != 0)
                {
                    await _mongodbServices.UpdatePrecipitaionAsync(id, precipitation_mm_h);
                    return CreatedAtAction(nameof(GetWeather), "Precipitation updated");
                }

                return CreatedAtAction(nameof(GetWeather), "You must enter a value into the [precipitation] field");
            }
            catch
            {
                return BadRequest("Incorect details");
            }
        }
        #endregion
    }
    #endregion
}
