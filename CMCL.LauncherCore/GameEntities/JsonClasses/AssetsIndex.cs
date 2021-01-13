using Newtonsoft.Json;

namespace CMCL.LauncherCore.GameEntities.JsonClasses
{
    public class AssetsIndex
    {
        public class Asset
        {
            [JsonProperty("hash")] public string Hash { get; set; }
            [JsonProperty("size")] public int Size { get; set; }

            public string DownloadUrl => $"{Hash.Substring(0, 2)}/{Hash}";
            public string SavePath => $"{Hash.Substring(0, 2)}\\{Hash}";
        }
    }
}