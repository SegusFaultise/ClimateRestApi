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
        public async Task<List<SensorDataModel>> GetWeatherAsync()
        {
            return await _weatherCollection.Find(new BsonDocument()).Limit(100).ToListAsync();
        }
        #endregion

        #region Http Gets A Sensor Reading By Id
        public async Task<SensorDataModel> GetSensorByIdAsync(string id)
        {
            var filter = Builders<SensorDataModel>.Filter.Eq("Id", id);
            return await _weatherCollection.Find(filter).FirstAsync();
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

            var w = await _weatherCollection.Aggregate().Match(device_filter).Match(time_filter).SortByDescending(u => u.Precipitation_mm_h).Project(project_stage).FirstAsync();
            return w.ToJson();
        }
        #endregion

        #region Gets Readings Based On A DateTime Range And A Device
        public async Task<string> GetFieldsBasedOnTimeAndDateAsync(string device, DateTime date_time)
        {
            var device_filter = Builders<SensorDataModel>.Filter.Eq("Device Name", device);
            var time_filter = Builders<SensorDataModel>.Filter.Eq("Time", date_time);

            var projectStage = Builders<SensorDataModel>.Projection.Expression(u =>
                new
                {
                    Device = u.Device,
                    Precipitation_mm_h = u.Precipitation_mm_h,
                    Time = u.Time,
                    AtmosphericPressure_kPa = u.AtmosphericPressure_kPa,
                    Temperature_C = u.Temperature_C,
                    SolarRadiation_Wm2 = u.SolarRadiation_Wm2
                });

            var w = await _weatherCollection.Aggregate().Match(time_filter).Match(device_filter).Project(projectStage).FirstAsync();
            return w.ToJson();
        }
        #endregion

        #region Gets The Max Tempreture Based On A Start Date And A End Date
        public async Task<string> GetMaxTempAsync(DateTime date_time_start, DateTime date_time_end)
        {
            var filter_1 = Builders<SensorDataModel>.Filter.Gt("Time", date_time_start);
            var filter_2 = Builders<SensorDataModel>.Filter.Lt("Time", date_time_end);
            var filters = Builders<SensorDataModel>.Filter.And(filter_1, filter_2);

            var projectStage = Builders<SensorDataModel>.Projection.Expression(u =>
                new
                {
                    Device = u.Device,
                    Time = u.Time,
                    Temperature_C = u.Temperature_C,
                });

            var result = await _weatherCollection.Aggregate().Match(filters).SortByDescending(u => u.Temperature_C).Project(projectStage).Limit(10).ToListAsync();
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
        public async void CreateMannyWeatherAsync(List<SensorDataModel> weather)
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
            return ;
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
            var filter = Builders<UserModel>.Filter.Eq(u => u.ApiToken, api_token);
            var user = await _userCollection.Find(filter).FirstOrDefaultAsync();

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

        #region Updates A User Login Time
        public async Task UpdateUserLoginTimeAsync(string api_token, DateTime login_date_time)
        {
            var filter = Builders<UserModel>.Filter.Eq(u => u.ApiToken, api_token);
            var update = Builders<UserModel>.Update.Set(u => u.LoginDate, login_date_time);

            await _userCollection.UpdateOneAsync(filter, update);
            return;
        }
        #endregion
    
        public async Task UpdateUserEmailAsync(string api_token, string email, DateTime start_date, DateTime end_date)
        {
            var filter_1 = Builders<UserModel>.Filter.Eq(u => u.ApiToken, api_token);
            var filter_2 = Builders<UserModel>.Filter.Gte(u => u.CreatedDate, start_date);
            var filter_3 = Builders<UserModel>.Filter.Lte(u => u.CreatedDate, end_date);
            var filters = Builders<UserModel>.Filter.And(filter_1, filter_2, filter_3);
            var update = Builders<UserModel>.Update.Set(u => u.UserEmail, email);

            await _userCollection.UpdateManyAsync(filters, update);
            return;
        }

        public async Task UpdateUserRoleAsync(string api_token, string role)
        {
            var filter = Builders<UserModel>.Filter.Eq(u => u.ApiToken, api_token);
            var update = Builders<UserModel>.Update.Set(u => u.Role, role);

            await _userCollection.UpdateManyAsync(filter, update);
            return;
        }

        //public async Task<bool> UpdateManyRoleAsync(UserFilter user_filter, string role)
        //{
        //    var filter = BuildFilter(user_filter);
        //    var update_role = Builders<UserModel>.Update.Set(c => c.Role, role);

        //    await _userCollection.UpdateManyAsync(filter, update_role);
        //    return true;
        //}

        //public async Task<bool> UpdateManyEmailAsync(UserFilter user_filter, string email)
        //{
        //    var filter = BuildFilter(user_filter);
        //    var update_email = Builders<UserModel>.Update.Set(c => c.UserEmail, email);
        //    var result = await _userCollection.UpdateManyAsync(filter, update_email);

        //    return true;
        //}

        //private FilterDefinition<UserModel> BuildFilter(UserFilter noteFilter)
        //{
        //    var builder = Builders<UserModel>.Filter;

        //    var filter = builder.Empty;

        //    if (!String.IsNullOrEmpty(noteFilter?.Role))
        //    {
        //        var regexFilter = Regex.Escape(noteFilter.Role);
        //        filter &= builder.Regex(c => c.Role, BsonRegularExpression.Create(regexFilter));

        //    }

        //    if (!String.IsNullOrEmpty(noteFilter?.UserEmail))
        //    {
        //        // Add a Contains filter for the Body
        //        var regexFilter = Regex.Escape(noteFilter.UserEmail);
        //        filter &= builder.Regex(c => c.CreatedDate, BsonRegularExpression.Create(regexFilter));
        //    }

        //    if (noteFilter != null && noteFilter.CreatedFrom.HasValue)
        //    {
        //        // add a greater than filter for the creation date
        //        filter &= builder.Gte(c => c.CreatedDate, noteFilter.CreatedFrom.Value);
        //    }

        //    if (noteFilter != null && noteFilter.CreatedTo.HasValue)
        //    {
        //        // add a less than filter for the creation date
        //        filter &= builder.Lte(c => c.CreatedDate, noteFilter.CreatedTo.Value);
        //    }

        //    return filter;

        //}
        #endregion
    }
}
