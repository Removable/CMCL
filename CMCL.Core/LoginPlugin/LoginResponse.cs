using Newtonsoft.Json;

namespace CMCL.Core.LoginPlugin
{
    public class LoginResponse
    {
        [JsonProperty("user")] public UserType User { get; set; }

        [JsonProperty("accessToken")] public string AccessToken { get; set; }

        [JsonProperty("clientToken")] public string ClientToken { get; set; }

        [JsonProperty("availableProfiles")] public GameProfile[] AvailableProfiles { get; set; }

        [JsonProperty("selectedProfile")] public GameProfile SelectedProfile { get; set; }

        public class UserType
        {
            [JsonProperty("username")] public string Username { get; set; }
            [JsonProperty("id")] public string Id { get; set; }
        }
    }
}