using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CMCL.Client.Util;
using ComponentUtil.Common.Data;
using Newtonsoft.Json;

namespace CMCL.Client.GameVersion.JsonClasses
{
    public class LibraryInfo
    {
        public enum Type
        {
            Library,
            Native
        }


        private Dictionary<string, List<Rule.OSInfo>> _rule;
        [JsonProperty("downloads")] public Download Downloads;
        [JsonProperty("extract")] public ExtractRule Extract;

        [JsonProperty("name")] public string Name;
        [JsonProperty("natives")] public NativesName Natives;
        [JsonProperty("rules")] public Rule[] Rules;

        public bool IsNative => Natives != null;

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (Rules == null) return;
            _rule = new Dictionary<string, List<Rule.OSInfo>>
            {
                {"allow", new List<Rule.OSInfo>()},
                {"disallow", new List<Rule.OSInfo>()}
            };
            foreach (var rule in Rules)
            {
                var ruleList = _rule[rule.Action] ?? new List<Rule.OSInfo>();
                ruleList.Add(rule.OS);
                _rule[rule.Action] = ruleList;
            }
        }

        public bool HasLibrary()
        {
            if (Downloads == null) return true;
            return Downloads.Artifact != null;
        }

        public bool ShouldDeployOnOs(SupportedOS os = SupportedOS.Windows, string version = null)
        {
            if (Rules == null) return true;
            var disallow = _rule["disallow"];
            var allow = _rule["allow"];
            //有允许无禁止时，找有无符合条件的系统
            if (!disallow.Any() && allow.Any())
                return allow.Any(osInfo => osInfo.Name == os.GetDescription() && (osInfo.Version == null ||
                    Regex.IsMatch(Environment.OSVersion.Version.ToString(), osInfo.Version)));
            //无禁止或有禁止但无符合条件的系统
            return !disallow.Any(osInfo => osInfo.Name == os.GetDescription() && (osInfo.Version == null ||
                Regex.IsMatch(Environment.OSVersion.Version.ToString(), osInfo.Version)));
        }

        public async ValueTask<bool> IsValidLibrary(string libraryPath)
        {
            var path = IOHelper.CombineAndCheckDirectory(libraryPath, GetLibraryPath());

            var fileInfo = new FileInfo(path);
            var library = GetLibrary();
            if (library == null) return fileInfo.Exists;
            if (GetLibrary().Size == 0 || GetLibrary().Sha1 == null)
                return fileInfo.Exists;
            return fileInfo.Exists
                   && fileInfo.Length == GetLibrary().Size
                   && await IOHelper.GetSha1HashFromFileAsync(path) == GetLibrary().Sha1;
        }

        public async ValueTask<bool> IsValidNative(string libraryPath)
        {
            var os = Utils.GetOS();
            var path = IOHelper.CombineAndCheckDirectory(libraryPath, GetNativePath());

            var fileInfo = new FileInfo(path);
            if (GetNative(os).Size == 0 || GetNative(os).Sha1 == null)
                return fileInfo.Exists && fileInfo.Length > 0;
            return fileInfo.Exists
                   && fileInfo.Length == GetNative(os).Size
                   && await IOHelper.GetSha1HashFromFileAsync(path) == GetNative(os).Sha1;
        }

        public string GetLibraryPath()
        {
            if (Downloads?.Artifact.Path != null) return Downloads.Artifact.Path;
            var libp = new StringBuilder();
            var split = Name.Split(':'); //0 包;1 名字；2 版本
            if (split.Length != 3) throw new Exception("未知的版本！");
            libp.Append(split[0].Replace('.', '\\')).Append("\\");
            libp.Append(split[1]).Append("\\");
            libp.Append(split[2]).Append("\\");
            libp.Append(split[1]).Append("-");
            libp.Append(split[2]).Append(".jar");
            return libp.ToString();
        }

        public string GetNativePath()
        {
            if (Downloads?.Classifiers != null)
            {
                var classifiers = Downloads.Classifiers;
                if (Environment.Is64BitOperatingSystem && classifiers.Windowsx64 != null)
                    return classifiers.Windowsx64.Path;
                if (classifiers.Windowsx32 != null) return classifiers.Windowsx32.Path;
                return classifiers.Windows.Path;
            }

            var libp = new StringBuilder();
            var split = Name.Split(':'); //0 包;1 名字；2 版本
            libp.Append(split[0].Replace('.', '\\'));
            libp.Append("\\");
            libp.Append(split[1]).Append("\\");
            libp.Append(split[2]).Append("\\");
            libp.Append(split[1]).Append("-").Append(split[2]).Append("-").Append(Natives.Windows);
            libp.Append(".jar");
            libp.Replace("${arch}", Environment.Is64BitOperatingSystem ? "64" : "32");
            return libp.ToString();
        }

        public Download.ArtifactInfo GetLibrary()
        {
            return Downloads?.Artifact;
        }

        public Download.ArtifactInfo GetNative(SupportedOS os)
        {
            Download.ArtifactInfo path = null;
            if (Downloads?.Classifiers != null)
                path = os switch
                {
                    SupportedOS.Windows => (Environment.Is64BitOperatingSystem
                        ? Downloads.Classifiers.Windowsx64
                        : Downloads.Classifiers.Windowsx32) ?? Downloads.Classifiers.Windows,
                    SupportedOS.Osx => Downloads.Classifiers.OSX,
                    SupportedOS.Unix => Downloads.Classifiers.Linux,
                    _ => Downloads.Classifiers.Windows
                };

            if (path != null) return path;
            path = new Download.ArtifactInfo
            {
                Path = GetNativePath()
            };
            return path;
        }

        private Download.ArtifactInfo GetArtifact()
        {
            if (IsNative && Downloads.Classifiers.Windows == null)
                return Environment.Is64BitOperatingSystem
                    ? Downloads.Classifiers.Windowsx64
                    : Downloads.Classifiers.Windowsx32;
            return Downloads.Classifiers?.Windows ?? Downloads.Artifact;
        }

        public class ExtractRule
        {
            [JsonProperty("exclude")] public string[] Exclude;
        }


        public class Download
        {
            [JsonProperty("artifact")] public ArtifactInfo Artifact;
            [JsonProperty("classifiers")] public ClassifiersInfo Classifiers;

            public class ArtifactInfo
            {
                [JsonProperty("path")] public string Path;
                [JsonProperty("sha1")] public string Sha1;
                [JsonProperty("size")] public int Size;
                [JsonProperty("url")] public string Url;
            }


            public class ClassifiersInfo
            {
                [JsonProperty("natives-linux")] public ArtifactInfo Linux;
                [JsonProperty("natives-osx")] public ArtifactInfo OSX;
                [JsonProperty("natives-windows")] public ArtifactInfo Windows;
                [JsonProperty("natives-windows-32")] public ArtifactInfo Windowsx32;
                [JsonProperty("natives-windows-64")] public ArtifactInfo Windowsx64;
            }
        }


        public class Rule
        {
            [JsonProperty("action")] public string Action;
            [JsonProperty("os")] public OSInfo OS;

            public class OSInfo
            {
                [JsonProperty("arch")] public string Arch;
                [JsonProperty("name")] public string Name;
                [JsonProperty("version")] public string Version;
            }
        }


        public class NativesName
        {
            [JsonProperty("linux")] public string Linux;
            [JsonProperty("osx")] public string OSX;
            [JsonProperty("windows")] public string Windows;
        }
    }
}