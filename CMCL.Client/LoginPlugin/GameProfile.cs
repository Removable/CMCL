using Newtonsoft.Json;

namespace CMCL.Client.LoginPlugin
{
    public class GameProfile
    {
        [JsonProperty("name")] public string Name { get; init; }
        [JsonProperty("id")] public string Id { get; init; }
    }
}