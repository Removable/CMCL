using Newtonsoft.Json;

namespace CMCL.LauncherCore.GameEntities.LoginInfo
{
    public class GameProfile
    {
        [JsonProperty("name")] public string Name { get; init; }
        [JsonProperty("id")] public string Id { get; init; }
    }
}