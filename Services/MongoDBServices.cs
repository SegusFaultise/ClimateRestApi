#region Imports
using CLIMATE_REST_API.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Options;
using CLIMATE_REST_API.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using CLIMATE_DATA_BRAZIL.Controllers;
using ZstdSharp.Unsafe;
using System.Web.Http.Filters;
using System.Collections;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Microsoft.OpenApi.Writers;
using System.Web.Http.Results;
using System.Text.RegularExpressions;
using System.Data;
#endregion

namespace CLIMATE_REST_API.Services
{
    #region MongoDB Services
    public class MongoDBServices
    {
        #region Setting The IMongoCollection To Weather
        private readonly IMongoCollection<SensorDataModel> _weatherCollection;
        private readonly IMongoCollection<UserModel> _userCollection;
        #endregion

        #region ConnectionURI, Database & Collection - Variables
        public MongoDBServices(IOptions<MongoDBSettings> mongodbSettings)
        {
            MongoClient client = new MongoClient(mongodbSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(mongodbSettings.Value.Database);

            _weatherCollection = database.GetCollection<SensorDataModel>(mongodbSettings.Value.Collection1);
            _userCollection = database.GetCollection<UserModel>(mongodbSettings.Value.Collection2);
        }
        #endregion

        #region Weather Aysnc Methods

        #region Gets All Sensor Readings
        public async Task<List<SensorDataModel>> GetAllSensorReadingsAsync()
        {
            return await _weatherCollection.Find(new BsonDocument()).
                Limit(100).
                ToListAsync();
        }
        #endregion

        #region Http Gets A Sensor Reading By Id
        public async Task<SensorDataModel> GetSensorByIdAsync(string id)
        {
            var id_filter = Builders<SensorDataModel>.Filter.Eq("Id", id);

            return await _weatherCollection.Find(id_filter)
                .FirstAsync();
        }
        #endregion

        #region Gets The Max Precipitation Of A Device And DateTime Range
        public async Task<string> GetMaxPrecipitaionAsync(string device)
        {
            var device_filter = Builders<SensorDataModel>.Filter.Eq("Device Name", device);
            var time_filter = Builders<SensorDataModel>.Filter.Lt("Time", DateTime.Now.AddMonths(-5));

            var project_stage = Builders<SensorDataModel>.Projection.Expression(u =>
                new
                {
                    Device = u.Device,
                    Precipitation_mm_h = u.Precipitation_mm_h,
                    Time = u.Time
                });

            var result = await _weatherCollection.Aggregate().
                Match(device_filter).
                Match(time_filter).
                SortByDescending(u => u.Precipitation_mm_h).
                Project(project_stage).
                FirstAsync();

            return result.ToJson();
        }
        #endregion

        #region Gets Readings Based On A DateTime Range And A Device
        public async Task<string> GetFieldsBasedOnTimeAndDateAsync(string device, DateTime date_time)
        {
            var device_filter = Builders<SensorDataModel>.Filter.Eq("Device Name", device);
            var time_filter = Builders<SensorDataModel>.Filter.Eq("Time", date_time);

            var project_stage = Builders<SensorDataModel>.Projection.Expression(u =>
                new
                {
                    Device = u.Device,
                    Precipitation_mm_h = u.Precipitation_mm_h,
                    Time = u.Time,
                    AtmosphericPressure_kPa = u.AtmosphericPressure_kPa,
                    Temperature_C = u.Temperature_C,
                    SolarRadiation_Wm2 = u.SolarRadiation_Wm2
                });

            var result = await _weatherCollection.Aggregate().
                Match(time_filter).
                Match(device_filter).
                Project(project_stage).
                FirstAsync();

            return result.ToJson();
        }
        #endregion

        #region Gets The Max Tempreture Based On A Start Date And A End Date
        public async Task<string> GetMaxTempAsync(DateTime date_time_start, DateTime date_time_end)
        {
            var filter_1 = Builders<SensorDataModel>.Filter.Gt("Time", date_time_start);
            var filter_2 = Builders<SensorDataModel>.Filter.Lt("Time", date_time_end);
            var filters = Builders<SensorDataModel>.Filter.And(filter_1, filter_2);

            var project_stage = Builders<SensorDataModel>.Projection.Expression(u =>
                new
                {
                    Device = u.Device,
                    Time = u.Time,
                    Temperature_C = u.Temperature_C,
                });

            var result = await _weatherCollection.
                Aggregate().
                Match(filters).
                SortByDescending(u => u.Temperature_C).
                Project(project_stage).
                Limit(10).
                ToListAsync();

            return result.ToJson();
        }
        #endregion

        #region Creates A New Sesnor Reading
        public async Task CreateWeatherAsync(SensorDataModel weather)
        {
            await _weatherCollection.InsertOneAsync(weather);
            return;
        }
        #endregion

        #region Creates Manny Sensor Readings 
        public async Task CreateMannyWeatherAsync(List<SensorDataModel> weather)
        { 

            await _weatherCollection.InsertManyAsync(weather);
            return;
        }
        #endregion

        #region Updates The Precipitation For A Single Device
        public async Task UpdatePrecipitaionAsync(string id, double precipitation_mm_h)
        {
            FilterDefinition<SensorDataModel> filter = Builders<SensorDataModel>.Filter.Eq("Id", id);
            UpdateDefinition<SensorDataModel> update = Builders<SensorDataModel>.Update.Set("Precipitation mm/h", precipitation_mm_h);

            await _weatherCollection.UpdateOneAsync(filter, update);
            return;
        }
        #endregion

        #endregion

        #region User Aysnc Methods

        #region Gets All Users
        public async Task<List<UserModel>> GetUserAsync()
        {
            return await _userCollection.Find(new BsonDocument()).Limit(10).ToListAsync();
        }
        #endregion

        #region Authenticates A User
        public async Task<UserModel?> AuthenticateUserAsync(string api_token, string role)
        {
            var api_token_filter = Builders<UserModel>.Filter.Eq(u => u.ApiToken, api_token);
            var user = await _userCollection.Find(api_token_filter).FirstOrDefaultAsync();

            if (user == null || user.Role != role)
            {
                return null;
            }

            return user;
        }
        #endregion

        #region Creates A New User
        public async Task<bool> CreatedUserAsync(UserModel user_model)
        {
            var filter = Builders<UserModel>.Filter.Eq(u => u.UserEmail, user_model.UserEmail);
            var existing_user = await _userCollection.Find(filter).FirstOrDefaultAsync();

            if (existing_user != null)
            {
                return false;
            }

            user_model.ApiToken = Guid.NewGuid().ToString();
            user_model.CreatedDate= DateTime.Now;

            await _userCollection.InsertOneAsync(user_model);
            return true;
        }
        #endregion

        #region Deletes A User
        public async Task DeleteUserByIdAsync(string api_token, string id)
        {
            FilterDefinition<UserModel> filter = Builders<UserModel>.Filter.Eq("Id", id);

            await _userCollection.DeleteOneAsync(filter);
            return;
        }
        #endregion


        #region Deletes Many Users
        public async Task<UserModel> DeletesManyUsersAsync(string api_token, DateTime date_time_start, DateTime date_time_end)
        {
            var filter_date_time_start = Builders<UserModel>.Filter.Gte(u => u.LoginDate, date_time_start);
            var filter_date_time_end = Builders<UserModel>.Filter.Lte(u => u.LoginDate, date_time_end);
            var filters = Builders<UserModel>.Filter.And(filter_date_time_start, filter_date_time_end);

            var users = _userCollection.Find(filters).FirstOrDefault();

            if (users == null)
            {
                return null;
            }

            await _userCollection.DeleteManyAsync(filters);
            return users;
        }
        #endregion

        #region Updates A User Login Time
        public async Task UpdateUserLoginTimeAsync(string api_token, DateTime login_date_time)
        {
            var api_token_filter = Builders<UserModel>.Filter.Eq(u => u.ApiToken, api_token);
            var update = Builders<UserModel>.Update.Set(u => u.LoginDate, login_date_time);

            await _userCollection.UpdateOneAsync(api_token_filter, update);
            return;
        }
        #endregion

        #region Updates Many Users Roles
        public async Task<UserModel?> PatchUsersRoleAsync(string property, object value, DateTime date_time_start, DateTime date_time_end)
        {
            var filter_date_time_start = Builders<UserModel>.Filter.Gte(u => u.CreatedDate, date_time_start);
            var filter_date_time_end = Builders<UserModel>.Filter.Lte(u => u.CreatedDate, date_time_end);
            var filters = Builders<UserModel>.Filter.And(filter_date_time_start, filter_date_time_end);
            var value_property_update = Builders<UserModel>.Update.Set(property, value);

            var users = _userCollection.Find(filters).FirstOrDefault();

            if (users == null)
            {
                return null;
            }

            await _userCollection.UpdateManyAsync(filters, value_property_update);
            return users;
        }
        #endregion

        #endregion
    }
    #endregion
}
