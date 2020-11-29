using System.Text.Json.Serialization;

namespace CMCL.Client.LoginPlugin
{
    public class GameProfile
    {
        [JsonPropertyName("name")] public string Name { get; init; }
        [JsonPropertyName("id")] public string Id { get; init; }
    }
}