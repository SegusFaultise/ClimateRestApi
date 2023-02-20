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
    public class SensorController : Controller
    {
        #region region MongoDBServices Variable
        private readonly MongoDBServices _mongodbServices;
        #endregion

        #region Setting _mongodbServices To mongodbServices
        public SensorController(MongoDBServices mongodbServices)
        {
            _mongodbServices = mongodbServices;
        }
        #endregion

        #region Http Get All Sensors
        /// <summary>
        /// Gets all sensor readings [Limited to 10 readings due to swagger not being able to load in more then 5000 records]
        /// </summary>
        /// <returns></returns>
        [EnableCors]
        [HttpGet]
        [Route("GetAllSesnors")]
        public async Task<List<SensorDataModel>> GetAllSensors()
        {
            return await _mongodbServices.GetWeatherAsync();
        }
        #endregion

        #region Http Gets A Sensor Reading By Id
        /// <summary>
        /// Gets a sensor reading by id
        /// </summary>
        /// <returns></returns>
        [EnableCors]
        [HttpGet]
        [Route("GetSesnorById")]
        public async Task<SensorDataModel> GetSensorById(string id)
        {
           return await _mongodbServices.GetSensorByIdAsync(id);
        }
        #endregion

        #region Http Get Max Precipitation
        /// <summary>
        /// Gets the max precipitation of a single sensor
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Gets fields based on time and date
        /// </summary>
        /// <returns></returns>
        [EnableCors()]
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
        /// <summary>
        /// Gets max tempreture based on a time range
        /// </summary>
        /// <returns></returns>
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

        #region Http Post A Single Sesnsor
        /// <summary>
        /// Post new sensor reading
        /// </summary>
        /// <returns></returns>
        [EnableCors]
        [HttpPost]
        [Route("PostSingleSensor")]
        public async Task<IActionResult> PostSingleSensorAsync([FromBody] SensorDataModel weatherModel)
        {
            try
            {
                await _mongodbServices.CreateWeatherAsync(weatherModel);
                return CreatedAtAction(nameof(GetAllSensors), new { id = weatherModel.Id }, weatherModel);
            }
            catch
            {
                return BadRequest("Incorect details");
            }
        }
        #endregion

        #region Http Post Manny Weather
        /// <summary>
        /// Post manny sensor readings
        /// </summary>
        /// <returns></returns>
        [EnableCors]
        [HttpPost]
        [Route("PostMannyWeather")]
        public ActionResult PostMannyWeatherAsync([FromBody] List<SensorDataModel> weatherModel)
        {
            try
            {
                _mongodbServices.CreateMannyWeatherAsync(weatherModel);
                return CreatedAtAction(nameof(GetAllSensors), new { id = weatherModel }, weatherModel);
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
        public async Task<IActionResult> PostWeatherToWebisteAsync(SensorDataModel sensor_model)
        {
            await _mongodbServices.CreateWeatherAsync(sensor_model);
            return CreatedAtAction(nameof(GetAllSensors), new { id = sensor_model.Id }, sensor_model);
        }
        #endregion

        #region Http Put Weather (Precipitation mm/h)
        /// <summary>
        /// Updates precipitation based on the id
        /// </summary>
        /// <returns></returns>
        [EnableCors]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePrecipitationAsync(string id, double precipitation_mm_h)
        { 
            try
            {
                if (precipitation_mm_h != 0)
                {
                    await _mongodbServices.UpdatePrecipitaionAsync(id, precipitation_mm_h);
                    return Ok("Precipitation updated");
                }

                return BadRequest("You must enter a value into the [precipitation] field");
            }
            catch
            {
                return Problem(statusCode: 500);
            }
        }
        #endregion
    }
    #endregion
}
