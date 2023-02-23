#region Imports
using Microsoft.AspNetCore.Mvc;
using CLIMATE_REST_API.Services;
using CLIMATE_REST_API.Models;
using Microsoft.AspNetCore.Cors;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Linq;
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

        #region Authenticate Users
        /// <summary>
        /// Authenticates the user to allow certian actions
        /// </summary>
        /// <param name="api_token"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        private async Task<bool> AuthenticateUser(string api_token, string role)
        {
            var auth = await _mongodbServices.AuthenticateUserAsync(api_token, role);

            if (auth == null)
            {
                return false;
            }

            await _mongodbServices.UpdateUserLoginTimeAsync(api_token, DateTime.Now);
            return true;
        }
        #endregion

        #region Http Get All Sensors
        /// <summary>
        /// Gets all sensor readings [Limited to 100 readings due to swagger not being able to load in more then 5000 records]
        /// </summary>
        /// <returns></returns>
        [EnableCors]
        [HttpGet]
        [Route("GetAllSensors")]
        public async Task<List<SensorDataModel>> GetAllSensors()
        {
            return await _mongodbServices.GetAllSensorReadingsAsync();
        }
        #endregion

        #region Http Gets A Sensor Reading By Id
        /// <summary>
        /// Gets a sensor reading by id
        /// </summary>
        /// <param name="id"></param>
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
        /// <param name="device"></param>
        /// <returns></returns>
        [EnableCors]
        [HttpGet]
        [Route("GetMaxPrecipitation")]
        public async Task<IActionResult> GetMaxPrecipitation(string device)
        {
            try
            {
                var result = await _mongodbServices.GetMaxPrecipitaionAsync(device);

                if(result == null)
                {
                    return BadRequest("No entry found");
                }

                if (device == null)
                {
                    return BadRequest("Please fill in the device field");
                }

                else
                {
                    return Ok(result);
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
        /// <param name="device"></param>
        /// <param name="date_time"></param>
        /// <returns></returns>
        [EnableCors()]
        [HttpGet]
        [Route("GetFieldsBasedOnTimeAndDate")]
        public async Task<IActionResult> GetFieldsBasedOnTimeAndDate(string device, DateTime date_time)
        {
            try
            {
                var result = await _mongodbServices.GetFieldsBasedOnTimeAndDateAsync(device, date_time);

                if (result == null)
                {
                    return BadRequest("No entry found");
                }

                if (device == null)
                {
                    return BadRequest("Please fill in the device field");
                }

                else
                {
                    return Ok(result);
                }
            }
            catch
            {
                return Problem();
            }
        }
        #endregion

        #region Http Get Max Tempreture Aysnc
        /// <summary>
        /// Gets max tempreture based on a time range
        /// </summary>
        /// <param name="date_time_start"></param>
        /// <param name="date_time_end"></param>
        /// <returns></returns>
        [EnableCors]
        [HttpGet]
        [Route("GetMaxTemp")]
        public async Task<IActionResult> GetMaxTemp(DateTime date_time_start, DateTime date_time_end)
        {
            try
            {
                var result = await _mongodbServices.GetMaxTempAsync(date_time_start, date_time_end);

                if (result == null)
                {
                    return BadRequest("No entry found");
                }

                if (date_time_start > date_time_end || date_time_end < date_time_start)
                {
                    return BadRequest("Do you have a time travel machine?");
                }

                else
                {
                    return Ok(result);
                }
            }
            catch
            {
                return Problem();
            }
        }
        #endregion

        #region Http Post A Single Sesnsor
        /// <summary>
        /// Post new sensor reading
        /// </summary>
        /// <param name="weather_model"></param>
        /// <returns></returns>
        [EnableCors]
        [HttpPost]
        [Route("PostSingleSensor")]
        public async Task<IActionResult> PostSingleSensorAsync([FromBody] SensorDataModel weather_model)
        {
            try
            {
                if (weather_model.Humidity_percentage + 
                    weather_model.Longitude + 
                    weather_model.Latitude + 
                    weather_model.Precipitation_mm_h + 
                    weather_model.WindDirection +
                    weather_model.AtmosphericPressure_kPa +
                    weather_model.Temperature_C + 
                    weather_model.SolarRadiation_Wm2 +
                    weather_model.MaxWindSpeed_ms == 0)
                {
                    return BadRequest("Please fill in the fields");
                }

                if (weather_model.Device == "")
                {
                    return BadRequest("Please fill in the fields");
                }

                else
                {
                    await _mongodbServices.CreateWeatherAsync(weather_model);

                    return CreatedAtAction(nameof(GetAllSensors), 
                        new { id = weather_model.Id },
                        weather_model);
                }
            }
            catch
            {
                return Problem();
            }
        }
        #endregion

        #region Http Post Manny Weather
        /// <summary>
        /// Post manny sensor readings
        /// </summary>
        /// <param name="weather_model"></param>
        /// <returns></returns>
        [EnableCors]
        [HttpPost]
        [Route("PostMannyWeather")]
        public async Task<IActionResult> PostMannyWeatherAsync([FromBody] List<SensorDataModel> weather_model)
        {
            try
            {
                if (Request.Body.Equals(0))
                {
                    return BadRequest("Please fill in the fields");
                }
                else
                {
                    await _mongodbServices.CreateMannyWeatherAsync(weather_model);

                    return CreatedAtAction(nameof(GetAllSensors), 
                        new { id = weather_model },
                        weather_model);
                }
            }
            catch
            {
                return Problem();
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

            return CreatedAtAction(nameof(GetAllSensors), 
                new { id = sensor_model.Id },
                sensor_model);
        }
        #endregion

        #region Http Put Weather (Precipitation mm/h)
        /// <summary>
        /// Updates precipitation based on the id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="precipitation_mm_h"></param>
        /// <param name="api_token"></param>
        /// <returns></returns>
        [EnableCors]
        [HttpPut("{id} UpdatePrecipitation")]
        public async Task<IActionResult> UpdatePrecipitationAsync(string id, double precipitation_mm_h, string api_token)
        { 
            try
            {
                if (AuthenticateUser(api_token, "Admin").Result == false)
                {
                    return Unauthorized("Unauthorized");
                }
                
                if (precipitation_mm_h == 0)
                {
                    return BadRequest("You must enter a value into the [precipitation] field");
                }

                await _mongodbServices.UpdatePrecipitaionAsync(id, precipitation_mm_h);

                return Ok("Precipitation updated");
            }
            catch
            {
                return Problem();
            }
        }
        #endregion
    }
    #endregion
}
