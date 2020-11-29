using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace CMCL.Client.GameVersion.JsonClasses
{
    public class VersionInfo
    {
        public class VersionAssetIndex
        {
            [JsonPropertyName("id")] public string Id;
            [JsonPropertyName("sha1")] public string Sha1;
            [JsonPropertyName("size")] public string Size;
            [JsonPropertyName("url")] public string Url;
            [JsonPropertyName("totalSize")] public string TotalSize;
        }


        public class VersionDownload
        {
            public class File
            {
                [JsonPropertyName("sha1")] public string Sha1;
                [JsonPropertyName("size")] public string Size;
                [JsonPropertyName("url")] public string Url;
            }

            [JsonPropertyName("client")] public File Client;
            [JsonPropertyName("server")] public File Server;
        }


        public class VersionArguments
        {
            [JsonPropertyName("game")] public object[] Game;
            [JsonPropertyName("jvm")] public object[] Jvm;
        }

        [JsonPropertyName("id")] public string Id;
        [JsonPropertyName("time")] public string Time;
        [JsonPropertyName("releaseTime")] public string ReleaseTime;
        [JsonPropertyName("type")] public string Type;

        [JsonPropertyName("minecraftArguments")]
        public string MinecraftArguments;

        [JsonPropertyName("mainClass")] public string MainClass;

        [JsonPropertyName("minimumLauncherVersion")]
        public int MinimumLauncherVersion;

        [JsonPropertyName("inheritsFrom")] public string InheritsFrom;
        [JsonPropertyName("assetIndex")] public VersionAssetIndex AssetIndex;
        [JsonPropertyName("libraries")] public LibraryInfo[] Libraries;
        [JsonPropertyName("downloads")] public VersionDownload Downloads;
        [JsonPropertyName("assets")] public string Assets;
        [JsonPropertyName("jar")] public string Jar;
        [JsonPropertyName("arguments")] public VersionArguments Arguments;
    }
}