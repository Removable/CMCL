using Newtonsoft.Json;

namespace CMCL.Client.GameVersion.JsonClasses
{
    public class VersionInfo
    {
        public class VersionAssetIndex
        {
            [JsonProperty("id")] public string Id;
            [JsonProperty("sha1")] public string Sha1;
            [JsonProperty("size")] public string Size;
            [JsonProperty("url")] public string Url;
            [JsonProperty("totalSize")] public string TotalSize;
        }


        public class VersionDownload
        {
            public class FileType
            {
                [JsonProperty("sha1")] public string Sha1;
                [JsonProperty("size")] public string Size;
                [JsonProperty("url")] public string Url;
            }

            [JsonProperty("client")] public FileType Client;
            [JsonProperty("server")] public FileType Server;
        }


        public class VersionArguments
        {
            [JsonProperty("game")] public object[] Game;
            [JsonProperty("jvm")] public object[] Jvm;
        }

        [JsonProperty("id")] public string Id;
        [JsonProperty("time")] public string Time;
        [JsonProperty("releaseTime")] public string ReleaseTime;
        [JsonProperty("type")] public string Type;

        [JsonProperty("minecraftArguments")]
        public string MinecraftArguments;

        [JsonProperty("mainClass")] public string MainClass;

        [JsonProperty("minimumLauncherVersion")]
        public int MinimumLauncherVersion;

        [JsonProperty("inheritsFrom")] public string InheritsFrom;
        [JsonProperty("assetIndex")] public VersionAssetIndex AssetIndex;
        [JsonProperty("libraries")] public LibraryInfo[] Libraries;
        [JsonProperty("downloads")] public VersionDownload Downloads;
        [JsonProperty("assets")] public string Assets;
        [JsonProperty("jar")] public string Jar;
        [JsonProperty("arguments")] public VersionArguments Arguments;
    }
}