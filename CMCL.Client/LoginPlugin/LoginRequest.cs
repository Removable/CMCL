using System.Text.Json.Serialization;

namespace CMCL.Client.LoginPlugin
{
    public class LoginRequest
    {
        public LoginRequest(string username, string password)
        {
            RequestUser = true;
            Agent = new AgentType {Name = "Minecraft", Version = 1};
            Username = username;
            ClientToken = MojangLogin.ClientToken;
            Password = password;
        }

        [JsonPropertyName("agent")] public AgentType Agent { get; set; }

        [JsonPropertyName("clientToken")] public string ClientToken { get; set; }

        [JsonPropertyName("password")] public string Password { get; set; }

        [JsonPropertyName("requestUser")] public bool RequestUser { get; set; }

        [JsonPropertyName("username")] public string Username { get; set; }


        public class AgentType
        {
            [JsonPropertyName("name")] public string Name { get; set; }

            [JsonPropertyName("version")] public int Version { get; set; }
        }
    }
}