﻿using System;
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
using HandyControl.Tools.Extension;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var gameVersionManifest = JsonSerializer.Deserialize<GameVersionManifest>(jsonStr, options);
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
            Downloader.DownloadInfoHandler.DownloadFinished = false;

            var progress = new Progress<double>();
            progress.ProgressChanged += (sender, value) => mainProgress.Report(value);
            await Downloader.GetFileAsync(httpClient,
                _gameDownloadUrl.Replace(":version", versionId).Replace(":category", "client"),
                progress, fullPath,
                new DownloadInfo
                {
                    TotalFilesCount = 1,
                    CurrentFileIndex = 1,
                    CurrentFileName = $"{versionId}.jar",
                    CurrentCategory = "游戏本体",
                    ReportFinish = false,
                }).ConfigureAwait(false);
            await Downloader.GetFileAsync(httpClient,
                _gameDownloadUrl.Replace(":version", versionId).Replace(":category", "json"),
                progress, fullPath,
                new DownloadInfo
                {
                    TotalFilesCount = 1,
                    CurrentFileIndex = 1,
                    CurrentFileName = $"{versionId}.json",
                    CurrentCategory = "JSON文件",
                    ReportFinish = true
                }).ConfigureAwait(false);
            //校验哈希值
            var versionInfo = await GameHelper.GetVersionInfo(versionId);
            
            return (true, "");
        }
    }
}