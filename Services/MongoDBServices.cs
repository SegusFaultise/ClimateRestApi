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
        public async Task<List<SensorDataModel>> GetWeatherAsync()
        {
            return await _weatherCollection.Find(new BsonDocument()).Limit(10).ToListAsync();
        }

        public async Task<SensorDataModel> GetDeviceNameAsync(string device)
        {
            var filter = Builders<SensorDataModel>.Filter.Eq("Device Name", device);
            return await _weatherCollection.Find(filter).FirstAsync();
        }

        #region Get Maximum Precipitation Aysnc
        public async Task<IResult> GetMaximumPrecipitaionAsync(string device)
        {
            var builders = Builders<SensorDataModel>.Filter;
            SensorDataModel sensor_data_model = new SensorDataModel();

            var filter1 = Builders<SensorDataModel>.Filter.Eq("Device Name", device);
            //var filter2 = Builders<SensorDataModel>.Filter.Where(u => u.Precipitation_mm_h.Value);
            //var FilterAnd = builders.And(filter1, filter2);

            var projectStage = Builders<SensorDataModel>.Projection.Expression(u =>
                new
                {
                    Device = u.Device,
                    Precipitation_mm_h = u.Precipitation_mm_h,
                    TimeProject = u.Time
                });

            await  _weatherCollection.Aggregate().Match(filter1).SortByDescending(u => u.Precipitation_mm_h).Project(projectStage).FirstAsync();
            return Results.Ok();

            //return await _weatherCollection.Project(projectStage).FirstAsync();

        }
        #endregion

        public async Task CreateWeatherAsync(SensorDataModel weather)
        {
            await _weatherCollection.InsertOneAsync(weather);
            return;
        }

        //public async Task<List<SensorDataModel>> CreateMultiWeatherAsync(SensorDataModel weather)
        //{
        //    await _weatherCollection.BulkWriteAsync().;
        //    return;
        //}
        //#endregion
        #endregion

        #region User Aysnc Methods
        public async Task<List<UserModel>> GetUserAsync()
        {
            return await _userCollection.Find(new BsonDocument()).Limit(10).ToListAsync();
        }
        #endregion
    }
}
