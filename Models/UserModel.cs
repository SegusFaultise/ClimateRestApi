using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel;

namespace CLIMATE_REST_API.Models
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(
        BsonType
        .ObjectId)]
        [DefaultValue("")]
        public string? Id 
        { get; set; }

        [BsonElement(
        "user_email")]
        public string UserEmail 
        { get; set; } = 
        null!;

        [BsonElement(
        "user_password")]
        public string? UserPassword 
        { get; set; } = 
        null!;

        [BsonElement(
        "created_date")]
        public DateTime? CreatedDate 
        { get; set; } = 
        null!;

        [BsonElement(
        "role")]
        public string Role 
        { get; set; } = 
        null!;

        [BsonElement(
        "login_date")]
        public DateTime? LoginDate 
        { get; set; } = 
        null!;

        [BsonElement(
        "api_token")]
        public string ApiToken 
        { get; set; } = 
        null!;
    }
}
