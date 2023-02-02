#region Imports
using CLIMATE_REST_API.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Options;
using CLIMATE_REST_API.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
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

        //#region Get Maximum Precipitation Aysnc
        //public async Task<SensorDataModel> GetMaximumPrecipitaionAsync(string device, int time)
        //{
        //    var months = time < 5;
        //    var device_filter = Builders<SensorDataModel>.Filter.Eq("Device Name", device);
        //    return await _weatherCollection.Find(filter).FirstAsync();
        //}
        //#endregion

        public async Task CreateWeatherAsync(SensorDataModel weather)
        {
            //var filter = Builders<SensorDataModel>.Filter.Eq("Device Name", device);
            //await _weatherCollection.Find(filter).FirstAsync();
            await _weatherCollection.InsertOneAsync(weather);
            return;
        }
        #endregion

        #region User Aysnc Methods
        public async Task<List<UserModel>> GetUserAsync()
        {
            return await _userCollection.Find(new BsonDocument()).Limit(10).ToListAsync();
        }
        #endregion
    }
}
