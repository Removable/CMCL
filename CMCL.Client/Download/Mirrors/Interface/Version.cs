using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CMCL.Client.Game;
using CMCL.Client.Util;
using Newtonsoft.Json;

namespace CMCL.Client.Download.Mirrors.Interface
{
    public abstract class Version
    {
        protected GameVersionManifest VersionManifest;
        public virtual string ManifestUrl { get; } = "";

        /// <summary>
        ///     获取版本列表
        /// </summary>
        /// <returns></returns>
        public async ValueTask<GameVersionManifest> LoadGameVersionList(HttpClient httpClient)
        {
            var jsonStr = await Downloader.GetStringAsync(httpClient, ManifestUrl).ConfigureAwait(false);
            var gameVersionManifest = JsonConvert.DeserializeObject<GameVersionManifest>(jsonStr);
            VersionManifest = gameVersionManifest;
            return gameVersionManifest;
        }

        /// <summary>
        ///     下载版本json
        /// </summary>
        /// <param name="versionId">游戏版本号</param>
        /// <returns></returns>
        public async ValueTask DownloadJsonAsync(string versionId)
        {
            var version = VersionManifest.Versions.FirstOrDefault(i => i.Id == versionId);
            if (version == null) throw new Exception("找不到指定版本");

            //转换地址
            var url = TransUrl(version.Url);

            await Downloader.GetFileAsync(GlobalStaticResource.HttpClientFactory.CreateClient(), url,
                Path.Combine(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "versions", versionId,
                    $"{versionId}.json"));
        }

        /// <summary>
        ///     下载版本jar
        /// </summary>
        /// <param name="versionId">游戏版本号</param>
        /// <returns></returns>
        public async ValueTask DownloadJarAsync(string versionId)
        {
            var versionInfo = await GameHelper.GetVersionInfo(versionId).ConfigureAwait(false);

            var filePath = Path.Combine(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "versions", versionId,
                $"{versionId}.jar");

            //转换地址
            var url = TransUrl(versionInfo.Downloads.Client.Url);

            await Downloader.GetFileAsync(GlobalStaticResource.HttpClientFactory.CreateClient(), url, filePath);
            //校验sha1
            if (!string.Equals(await FileHelper.GetSha1HashFromFileAsync(filePath).ConfigureAwait(false),
                versionInfo.Downloads.Client.Sha1, StringComparison.CurrentCultureIgnoreCase))
                throw new FileSha1Error("文件校验错误，请重新下载");
        }

        /// <summary>
        /// 转换下载地址
        /// </summary>
        /// <param name="originUrl"></param>
        /// <returns></returns>
        protected virtual string TransUrl(string originUrl)
        {
            const string server = "";
            var originServers = new[] {"https://launchermeta.mojang.com/", "https://launcher.mojang.com/"};

            return originServers.Aggregate(originUrl, (current, originServer) => current.Replace(originServer, server));
        }
    }
}