namespace CLIMATE_REST_API.Models
{
    #region MongoDB Settings Model
    public class MongoDBSettings
    {
        public string ConnectionURI 
        { get; set; } =
        null!;

        public string Database 
        { get; set; } =
        null!;

        public string Collection1 
        { get; set; } = 
        null!;

        public string Collection2 
        { get; set; } = 
        null!;

        public string Collection3 
        { get; set; } = 
        null!;
    }
    #endregion
}
