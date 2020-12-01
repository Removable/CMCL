using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CMCL.Client.Game;
using CMCL.Client.Util;
using Newtonsoft.Json;

namespace CMCL.Client.Download.Mirrors.Interface
{
    public abstract class Version
    {
        public virtual string ManifestUrl { get; } = "https://launchermeta.mojang.com/mc/game/version_manifest.json";
        protected GameVersionManifest VersionManifest;

        /// <summary>
        /// 获取版本列表
        /// </summary>
        /// <returns></returns>
        public async ValueTask<GameVersionManifest> LoadGameVersionList(HttpClient httpClient)
        {
            var jsonStr = await Downloader.GetStringAsync(httpClient, this.ManifestUrl).ConfigureAwait(false);
            var gameVersionManifest = JsonConvert.DeserializeObject<GameVersionManifest>(jsonStr);
            VersionManifest = gameVersionManifest;
            return gameVersionManifest;
        }

        /// <summary>
        /// 下载版本json
        /// </summary>
        /// <param name="versionId">游戏版本号</param>
        /// <returns></returns>
        public async ValueTask DownloadJsonAsync(string versionId)
        {
            var version = VersionManifest.Versions.FirstOrDefault(i => i.Id == versionId);
            if (version == null) throw new Exception("找不到指定版本");

            await Downloader.GetFileAsync(GlobalStaticResource.HttpClientFactory.CreateClient(), version.Url,
                Path.Combine(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "versions", versionId,
                    $"{versionId}.json"));
        }

        /// <summary>
        /// 下载版本jar
        /// </summary>
        /// <param name="versionId">游戏版本号</param>
        /// <returns></returns>
        public async ValueTask DownloadJarAsync(string versionId)
        {
            var versionInfo = await GameHelper.GetVersionInfo(versionId).ConfigureAwait(false);

            var filePath = Path.Combine(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "versions", versionId,
                $"{versionId}.jar");
            await Downloader.GetFileAsync(GlobalStaticResource.HttpClientFactory.CreateClient(),
                versionInfo.Downloads.Client.Url, filePath);
            //校验sha1
            if (!string.Equals(await FileHelper.GetSha1HashFromFileAsync(filePath).ConfigureAwait(false), versionInfo.Downloads.Client.Sha1, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new FileSha1Error("文件校验错误，请重新下载");
            }
        }
    }
}