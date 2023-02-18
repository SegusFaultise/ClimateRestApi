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
#endregion

namespace CLIMATE_REST_API.Services
{
    public class MongoDBServices
    {
        #region Setting The IMongoCollection To Weather
        private readonly IMongoCollection<SensorDataModel> _weatherCollection;
        private readonly IMongoCollection<UserModel> _userCollection;
        private readonly IMongoCollection<AdminModel> _adminCollection;
        #endregion

        #region ConnectionURI, Database & Collection - Variables
        public MongoDBServices(IOptions<MongoDBSettings> mongodbSettings)
        {
            MongoClient client = new MongoClient(mongodbSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(mongodbSettings.Value.Database);

            _weatherCollection = database.GetCollection<SensorDataModel>(mongodbSettings.Value.Collection1);
            _userCollection = database.GetCollection<UserModel>(mongodbSettings.Value.Collection2);
            _adminCollection = database.GetCollection<AdminModel>(mongodbSettings.Value.Collection3);
        }
        #endregion

        #region Weather Aysnc Methods
        public async Task<List<SensorDataModel>> GetWeatherAsync()
        {
            return await _weatherCollection.Find(new BsonDocument()).Limit(10).ToListAsync();
        }

        public async Task<SensorDataModel> GetDeviceNameAsync(string device)
        {
            var filter = Builders<SensorDataModel>.Filter.Eq("Device Name", device);
            return await _weatherCollection.Find(filter).FirstAsync();
        }

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

            var w = await  _weatherCollection.Aggregate().Match(device_filter).Match(time_filter).SortByDescending(u => u.Precipitation_mm_h).Project(project_stage).FirstAsync();
            return w.ToJson();
        }

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
        
        public async Task<string> GetMaxTempAsync(DateTime date_time_start, DateTime date_time_end)
        {
            var time_filter_start = Builders<SensorDataModel>.Filter.Gt("Time", date_time_start);
            var time_filter_end = Builders<SensorDataModel>.Filter.Lt("Time", date_time_end);

            var projectStage = Builders<SensorDataModel>.Projection.Expression(u =>
                new
                {
                    Device = u.Device,
                    Time = u.Time,
                    Temperature_C = u.Temperature_C,
                });

            var result = await _weatherCollection.Aggregate().Match(time_filter_start).SortByDescending(u => u.Temperature_C).Project(projectStage).ToListAsync();
            return result.ToJson();
        }

        public async Task CreateWeatherAsync(SensorDataModel weather)
        {
            await _weatherCollection.InsertOneAsync(weather);
            return;
        }

        public void CreateMannyWeatherAsync(List<SensorDataModel> weather)
        {
            _weatherCollection.InsertMany(weather);
            return;
        }

        public async Task UpdatePrecipitaionAsync(string id, double precipitation_mm_h)
        {
            FilterDefinition<SensorDataModel> filter = Builders<SensorDataModel>.Filter.Eq("Id", id);
            UpdateDefinition<SensorDataModel> update = Builders<SensorDataModel>.Update.Set("Precipitation mm/h", precipitation_mm_h);

            await _weatherCollection.UpdateOneAsync(filter, update);
            return;
        }

        #endregion

        #region User Aysnc Methods
        public async Task<List<UserModel>> GetUserAsync()
        {
            return await _userCollection.Find(new BsonDocument()).Limit(10).ToListAsync();
        }

        public async Task DeleteUserByIdAsync(string id)
        {
            FilterDefinition<UserModel> filter = Builders<UserModel>.Filter.Eq("Id", id);
            await _userCollection.DeleteOneAsync(filter);
            return;
        }
        #endregion

        #region Admin Aysnc Methods
        public async Task<List<AdminModel>> GetAdminAsync()
        {
            return await _adminCollection.Find(new BsonDocument()).Limit(10).ToListAsync();
        }
        #endregion
    }
}
