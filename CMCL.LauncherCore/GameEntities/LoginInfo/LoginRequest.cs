using CMCL.LauncherCore.Launch;
using Newtonsoft.Json;

namespace CMCL.LauncherCore.GameEntities.LoginInfo
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

        [JsonProperty("agent")] public AgentType Agent { get; set; }

        [JsonProperty("clientToken")] public string ClientToken { get; set; }

        [JsonProperty("password")] public string Password { get; set; }

        [JsonProperty("requestUser")] public bool RequestUser { get; set; }

        [JsonProperty("username")] public string Username { get; set; }


        public class AgentType
        {
            [JsonProperty("name")] public string Name { get; set; }

            [JsonProperty("version")] public int Version { get; set; }
        }
    }
}