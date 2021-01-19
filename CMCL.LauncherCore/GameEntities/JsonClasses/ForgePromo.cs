using Newtonsoft.Json;

namespace CMCL.LauncherCore.GameEntities.JsonClasses
{
    public class ForgePromo
    {
        [JsonProperty("_id")] public string Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("build")] public ForgePromoBuild Build { get; set; }
        [JsonProperty("__v")] public string V { get; set; }
        
        
        public class ForgePromoBuild
        {
            [JsonProperty("_id")] public string Id { get; set; }
            [JsonProperty("build")] public string Build { get; set; }
            [JsonProperty("__v")] public string V { get; set; }
            [JsonProperty("version")] public string Version { get; set; }
            [JsonProperty("modified")] public string Modified { get; set; }
            [JsonProperty("mcversion")] public string McVersion { get; set; }
            [JsonProperty("files")] public ForgePromoBuildFile[] Files { get; set; }
            [JsonProperty("branch")] public string Branch { get; set; }
            
            public class ForgePromoBuildFile
            {
                [JsonProperty("category")] public string Category { get; set; }
                [JsonProperty("format")] public string Format { get; set; }
            }
        }
    }
}