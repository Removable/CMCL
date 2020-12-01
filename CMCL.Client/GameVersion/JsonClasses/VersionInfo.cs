using Newtonsoft.Json;

namespace CMCL.Client.GameVersion.JsonClasses
{
    public class VersionInfo
    {
        [JsonProperty("arguments")] public VersionArguments Arguments;
        [JsonProperty("assetIndex")] public VersionAssetIndex AssetIndex;
        [JsonProperty("assets")] public string Assets;
        [JsonProperty("downloads")] public VersionDownload Downloads;

        [JsonProperty("id")] public string Id;

        [JsonProperty("inheritsFrom")] public string InheritsFrom;
        [JsonProperty("jar")] public string Jar;
        [JsonProperty("libraries")] public LibraryInfo[] Libraries;

        [JsonProperty("mainClass")] public string MainClass;

        [JsonProperty("minecraftArguments")] public string MinecraftArguments;

        [JsonProperty("minimumLauncherVersion")]
        public int MinimumLauncherVersion;

        [JsonProperty("releaseTime")] public string ReleaseTime;
        [JsonProperty("time")] public string Time;
        [JsonProperty("type")] public string Type;

        public class VersionAssetIndex
        {
            [JsonProperty("id")] public string Id;
            [JsonProperty("sha1")] public string Sha1;
            [JsonProperty("size")] public string Size;
            [JsonProperty("totalSize")] public string TotalSize;
            [JsonProperty("url")] public string Url;
        }


        public class VersionDownload
        {
            [JsonProperty("client")] public FileType Client;
            [JsonProperty("server")] public FileType Server;

            public class FileType
            {
                [JsonProperty("sha1")] public string Sha1;
                [JsonProperty("size")] public string Size;
                [JsonProperty("url")] public string Url;
            }
        }


        public class VersionArguments
        {
            [JsonProperty("game")] public object[] Game;
            [JsonProperty("jvm")] public object[] Jvm;
        }
    }
}