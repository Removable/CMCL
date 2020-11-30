using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CMCL.Client.Download;
using CMCL.Client.Game;
using CMCL.Client.UserControl;
using CMCL.Client.Util;
using CMCL.Client.Window;
using HandyControl.Tools.Extension;
using Newtonsoft.Json;

namespace CMCL.Client.GameVersion
{
    public class VersionDownloader
    {
        private static readonly string _versionManifestUrl =
            @"https://bmclapi2.bangbang93.com/mc/game/version_manifest.json";

        private static readonly string _gameDownloadUrl = @"https://bmclapi2.bangbang93.com/version/:version/:category";

        /// <summary>
        /// 获取版本列表
        /// </summary>
        /// <returns></returns>
        public static async ValueTask<GameVersionManifest> LoadGameVersionList(HttpClient httpClient)
        {
            var jsonStr = await Downloader.GetStringAsync(httpClient, _versionManifestUrl).ConfigureAwait(false);
            var gameVersionManifest = JsonConvert.DeserializeObject<GameVersionManifest>(jsonStr);
            return gameVersionManifest;
        }

        /// <summary>
        /// 下载指定版本游戏本体jar包和json文件
        /// </summary>
        /// <param name="mainProgress"></param>
        /// <param name="versionId"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async ValueTask<(bool success, string msg)> DownloadClient(HttpClient httpClient, IProgress<double> mainProgress, string versionId, string path = "")
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }

            var fullPath = Path.Combine(path, ".minecraft", "versions", versionId);
            Downloader.DownloadInfoHandler.TaskFinished = false;

            var progress = new Progress<double>();
            progress.ProgressChanged += (sender, value) => mainProgress.Report(value);
            //await Downloader.GetFileAsync(httpClient,
            //    _gameDownloadUrl.Replace(":version", versionId).Replace(":category", "client"),
            //    progress, fullPath,
            //    new DownloadInfo
            //    {
            //        TotalFilesCount = 1,
            //        CurrentFileIndex = 1,
            //        CurrentFileName = $"{versionId}.jar",
            //        CurrentCategory = "游戏本体",
            //        ReportFinish = false,
            //    }).ConfigureAwait(false);
            
            //先下载版本json
            var jsonPath = _gameDownloadUrl.Replace(":version", versionId).Replace(":category", "json");
            await Downloader.GetFileAsync(httpClient, jsonPath, progress, fullPath,
                new DownloadInfo
                {
                    TotalFilesCount = 1,
                    CurrentFileIndex = 1,
                    CurrentFileName = $"{versionId}.json",
                    CurrentCategory = "游戏版本文件",
                    ReportFinish = true
                }).ConfigureAwait(false);
            //校验json自身哈希值
            var versionInfo = await GameHelper.GetVersionInfo(versionId);
            //var loadingFrm = LoadingFrm.GetInstance();
            //loadingFrm.DoWork("正在校验文件...", () =>
            //{
            //    FileHelper.GetSha1()
            //});
            
            return (true, "");
        }


    }
}