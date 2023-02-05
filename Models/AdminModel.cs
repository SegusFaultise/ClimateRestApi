using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.ComponentModel;

namespace CLIMATE_REST_API.Models
{
    public class AdminModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [DefaultValue("")]
        public string? Id { get; set; }

        [BsonElement("user_name")]
        public string UserEmail { get; set; } = null!;

        [BsonElement("admin_password")]
        public string? UserPassword { get; set; } = null!;

        [BsonElement("role")]
        public string? Role { get; set; } = null!;
    }
}
