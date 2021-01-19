using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CMCL.LauncherCore.GameEntities.JsonClasses;
using Newtonsoft.Json;

namespace CMCL.LauncherCore.Download.Mirrors.Interface
{
    public abstract class Forge
    {
        public virtual string GetPromosUrl { get; } = "";

        /// <summary>
        /// 获取受标记的forge版本
        /// </summary>
        /// <param name="httpClient"></param>
        /// <returns></returns>
        public virtual async ValueTask<ForgeVersion[]> GetForgeVersionList(HttpClient httpClient)
        {
            var jsonStr = await Downloader.GetStringAsync(httpClient, GetPromosUrl).ConfigureAwait(false);
            var promos = JsonConvert.DeserializeObject<ForgeVersion[]>(jsonStr);
            return promos ?? Array.Empty<ForgeVersion>();
        }

        /// <summary>
        /// 获取forge安装器下载地址
        /// </summary>
        /// <param name="selectedForge"></param>
        /// <returns></returns>
        public string GetInstallerDownloadUrl(ForgeVersion selectedForge)
        {
            var build = selectedForge.Build;
            var installer = selectedForge.Build.Files.FirstOrDefault(f => f.Category == "installer") ?? new();
            return
                $"http://bmclapi2.bangbang93.com/maven/net/minecraftforge/forge/{build.McVersion}-{build.Version}{(build.Branch != null ? $"-{build.Branch}" : "")}/forge-{build.McVersion}-{build.Version}{(build.Branch != null ? $"-{build.Branch}" : "")}-{installer.Category}.{installer.Format}";
        }
    }
}