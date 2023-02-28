namespace CLIMATE_REST_API.Models
{
    public class UserPatchModel
    {
        public UserFilter Filter 
        { get; set; }

        public string PropertyName 
        { get; set; }

        public string PropertyValue 
        { get; set; }
    }
}
