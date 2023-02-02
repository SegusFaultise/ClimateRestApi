#region Imports
using CLIMATE_REST_API.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Options;
using CLIMATE_REST_API.Models;
#endregion

namespace CLIMATE_REST_API.Services
{
    public class MongoDBServices
    {
        #region Setting The IMongoCollection To Weather
        private readonly IMongoCollection<SensorDataModel> _weatherCollection;
        #endregion

        #region ConnectionURI, Database & Collection - Variables
        public MongoDBServices(IOptions<MongoDBSettings> mongodbSettings) 
        {
            MongoClient client = new MongoClient(mongodbSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(mongodbSettings.Value.Database);
            _weatherCollection = database.GetCollection<SensorDataModel>(mongodbSettings.Value.Collection);
        }
        #endregion

        #region Get Weather Aysnc
        public async Task<List<SensorDataModel>> GetWeatherAsync()
        {
            return await _weatherCollection.Find(new BsonDocument()).Limit(10).ToListAsync();
        }
        #endregion

        #region Get Weather Aysnc
        public async Task<List<SensorDataModel>> GetDeviceNameAsync()
        {
            SensorDataModel sensorDataModel = new SensorDataModel();
            return await _weatherCollection.Find(new BsonDocument()).ToListAsync();
        }
        #endregion


        #region Create Weather Async
        public async Task CreateWeatherAsync(SensorDataModel weather)
        {
            await _weatherCollection.InsertOneAsync(weather);
            return;
        }
        #endregion
    }
}
