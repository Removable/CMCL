﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using CMCL.Client.Util;

namespace CMCL.Client.GameVersion.JsonClasses
{
  
  public class LibraryInfo
  {
    
    public class ExtractRule
    {
      [JsonPropertyName("exclude")] public string[] Exclude;
    }

    
    public class Download
    {
      
      public class ArtifactInfo
      {
        [JsonPropertyName("size")] public int Size;
        [JsonPropertyName("sha1")] public string Sha1;
        [JsonPropertyName("path")] public string Path;
        [JsonPropertyName("url")] public string Url;
      }

      
      public class ClassifiersInfo
      {
        [JsonPropertyName("natives-linux")] public ArtifactInfo Linux;
        [JsonPropertyName("natives-osx")] public ArtifactInfo OSX;
        [JsonPropertyName("natives-windows")] public ArtifactInfo Windows;
        [JsonPropertyName("natives-windows-32")] public ArtifactInfo Windowsx32;
        [JsonPropertyName("natives-windows-64")] public ArtifactInfo Windowsx64;
      }

      [JsonPropertyName("artifact")] public ArtifactInfo Artifact;
      [JsonPropertyName("classifiers")] public ClassifiersInfo Classifiers;
    }

    
    public class Rule
    {
      
      public class OSInfo
      {
        [JsonPropertyName("name")] public string Name;
      }

      [JsonPropertyName("action")] public string Action;
      [JsonPropertyName("os")] public OSInfo OS;
    }

    
    public class NativesName
    {
      [JsonPropertyName("linux")] public string Linux;
      [JsonPropertyName("osx")] public string OSX;
      [JsonPropertyName("windows")] public string Windows;
    }

    [JsonPropertyName("name")] public string Name;
    [JsonPropertyName("downloads")] public Download Downloads;
    [JsonPropertyName("rules")] public Rule[] Rules;
    [JsonPropertyName("extract")] public ExtractRule Extract;
    [JsonPropertyName("natives")] public NativesName Natives;

    public enum Type
    {
      Library,
      Native
    };


    private Dictionary<string, List<Rule.OSInfo>> _rule;

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
      if (Downloads == null)
      {
        return true;
      }
      return Downloads.Artifact != null;
    }

    public bool IsNative => Natives != null;

    public bool ShouldDeployOnOs(string os = "windows", string version = null)
    {
      if (Rules == null) return true;
      var disallow = _rule["disallow"];
      var allow = _rule["allow"];
      if (disallow.Count != 0 && allow.Count == 0)
      {
        return disallow.All(osInfo => osInfo.Name != os);
      }
      if (allow.Count != 0 && disallow.Count == 0)
      {
        return allow.Any(osInfo => osInfo.Name == os);
      }
      return true;
    }

    public bool IsVaildLibrary(string libraryPath)
    {
      var path = Path.Combine(libraryPath, GetLibraryPath());

      var fileInfo = new FileInfo(path);
      var library = GetLibrary();
      if (library == null)
      {
        return fileInfo.Exists;
      }
      if (GetLibrary().Size == 0 || GetLibrary().Sha1 == null)
        return fileInfo.Exists;
      return fileInfo.Exists
             && fileInfo.Length == GetLibrary().Size
             && CryptoHelper.GetSha1HashFromFile(path) == GetLibrary().Sha1;
    }

    public bool IsVaildNative(string libraryPath)
    {
      var path = Path.Combine(libraryPath, GetNativePath());

      var fileInfo = new FileInfo(path);
      if (GetNative().Size == 0 || GetNative().Sha1 == null)
        return fileInfo.Exists && fileInfo.Length > 0;
      return fileInfo.Exists
             && fileInfo.Length == GetNative().Size
             && CryptoHelper.GetSha1HashFromFile(path) == GetNative().Sha1;
    }

    public string GetLibraryPath() 
    {
      if (Downloads?.Artifact.Path != null)
      {
        return Downloads.Artifact.Path;
      }
      var libp = new StringBuilder();
      var split = Name.Split(':'); //0 包;1 名字；2 版本
      if (split.Length != 3)
      {
        throw new Exception("未知的版本！");
      }
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
        {
          return classifiers.Windowsx64.Path;
        }
        if (classifiers.Windowsx32 != null)
        {
          return classifiers.Windowsx32.Path;
        }
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

    public Download.ArtifactInfo GetNative()
    {
      Download.ArtifactInfo path = null;
      if (Downloads?.Classifiers != null)
      {
        path = (Environment.Is64BitOperatingSystem
                 ? Downloads.Classifiers.Windowsx64
                 : Downloads.Classifiers.Windowsx32) ?? Downloads.Classifiers.Windows;
      }
      if (path != null) return path;
      path = new Download.ArtifactInfo()
      {
        Path = this.GetNativePath()
      };
      return path;
    }

    private Download.ArtifactInfo GetArtifact()
    {
      if (IsNative && Downloads.Classifiers.Windows == null)
      {
        return Environment.Is64BitOperatingSystem
          ? Downloads.Classifiers.Windowsx64
          : Downloads.Classifiers.Windowsx32;
      }
      return Downloads.Classifiers?.Windows ?? Downloads.Artifact;
    }
  }
}
