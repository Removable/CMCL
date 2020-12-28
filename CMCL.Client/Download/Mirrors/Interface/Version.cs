using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using CMCL.Client.Game;
using CMCL.Client.Util;
using CMCL.Client.Window;
using Newtonsoft.Json;

namespace CMCL.Client.Download.Mirrors.Interface
{
    public abstract class Version
    {
        public GameVersionManifest VersionManifest;
        public virtual string ManifestUrl { get; } = "";

        /// <summary>
        ///     获取版本列表
        /// </summary>
        /// <returns></returns>
        public virtual async ValueTask<GameVersionManifest> LoadGameVersionList(HttpClient httpClient)
        {
            var jsonStr = await Downloader.GetStringAsync(httpClient, ManifestUrl).ConfigureAwait(false);
            var gameVersionManifest = JsonConvert.DeserializeObject<GameVersionManifest>(jsonStr);
            VersionManifest = gameVersionManifest;
            return gameVersionManifest;
        }

        /// <summary>
        /// 下载版本json和jar，并显示下载进度
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public virtual async ValueTask DownloadJsonAndJarAsync(string versionId)
        {
            var downloadFrm = DownloadFrm.GetInstance(Application.Current.MainWindow);
            downloadFrm.Dispatcher.BeginInvoke(new Action(() =>
            {
                downloadFrm.DataContext = Downloader.DownloadInfoHandler;
                downloadFrm.ShowDialog();
            }));

            try
            {
                await DownloadJsonAsync(versionId);

                await DownloadJarAsync(versionId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                downloadFrm.Dispatcher.BeginInvoke(new Action(() =>
                {
                    downloadFrm.Close();
                }));
            }
        }
        
        /// <summary>
        ///     下载版本json
        /// </summary>
        /// <param name="versionId">游戏版本号</param>
        /// <returns></returns>
        public virtual async ValueTask DownloadJsonAsync(string versionId)
        {
            var version = VersionManifest.Versions.FirstOrDefault(i => i.Id == versionId);
            if (version == null) throw new Exception("找不到指定版本");

            //转换地址
            var url = TransUrl(version.Url);

            await Downloader.GetFileAsync(GlobalStaticResource.HttpClientFactory.CreateClient(), url,
                IOHelper.CombineAndCheckDirectory(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "versions",
                    versionId, $"{versionId}.json"), $"下载{versionId}.json");

            //重新加载版本信息列表
            await GameHelper.LoadVersionInfoList();
        }

        /// <summary>
        ///     下载版本jar
        /// </summary>
        /// <param name="versionId">游戏版本号</param>
        /// <returns></returns>
        public virtual async ValueTask DownloadJarAsync(string versionId)
        {
            var versionInfo = GameHelper.GetVersionInfo(versionId);

            var filePath = IOHelper.CombineAndCheckDirectory(AppConfig.GetAppConfig().MinecraftDir, ".minecraft",
                "versions", versionId,
                $"{versionId}.jar");

            //转换地址
            var url = TransUrl(versionInfo.Downloads.Client.Url);

            //校验sha1
            if (!File.Exists(filePath) ||
                !string.Equals(await IOHelper.GetSha1HashFromFileAsync(filePath).ConfigureAwait(false),
                    versionInfo.Downloads.Client.Sha1, StringComparison.CurrentCultureIgnoreCase))
                await Downloader.GetFileAsync(GlobalStaticResource.HttpClientFactory.CreateClient(), url, filePath, "");
        }

        /// <summary>
        ///     转换下载地址
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