using System.Linq;
using CMCL.LauncherCore.Download.Mirrors.Interface;
using Newtonsoft.Json;

namespace CMCL.LauncherCore.GameEntities.JsonClasses
{
    public class ForgeVersion
    {
        [JsonProperty("_id")] public string Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("build")] public ForgeBuild Build { get; set; }
        [JsonProperty("__v")] public string V { get; set; }

        public class ForgeBuild
        {
            [JsonProperty("_id")] public string Id { get; set; }
            [JsonProperty("build")] public string Build { get; set; }
            [JsonProperty("__v")] public string V { get; set; }
            [JsonProperty("version")] public string Version { get; set; }
            [JsonProperty("modified")] public string Modified { get; set; }
            [JsonProperty("mcversion")] public string McVersion { get; set; }
            [JsonProperty("files")] public ForgeBuildFile[] Files { get; set; }
            [JsonProperty("branch")] public string Branch { get; set; }

            public class ForgeBuildFile
            {
                [JsonProperty("category")] public string Category { get; set; }
                [JsonProperty("format")] public string Format { get; set; }
                [JsonProperty("hash")] public string Hash { get; set; }
            }
        }
    }
}