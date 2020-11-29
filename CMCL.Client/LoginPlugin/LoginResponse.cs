using System.Text.Json.Serialization;

namespace CMCL.Client.LoginPlugin
{
    public class LoginResponse
    {
        public class UserType
        {
            [JsonPropertyName("username")] public string Username { get; set; }
            [JsonPropertyName("id")] public string Id { get; set; }
        }
        
        [JsonPropertyName("user")]
        public UserType User { get; set; }
        
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }
        
        [JsonPropertyName("clientToken")]
        public string ClientToken { get; set; }
        
        [JsonPropertyName("availableProfiles")]
        public GameProfile[] AvailableProfiles { get; set; }
        
        [JsonPropertyName("selectedProfile")]
        public GameProfile SelectedProfile { get; set; }
    }
}