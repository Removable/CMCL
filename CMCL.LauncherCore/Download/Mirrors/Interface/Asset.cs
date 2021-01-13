using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CMCL.LauncherCore.GameEntities;
using CMCL.LauncherCore.GameEntities.JsonClasses;
using CMCL.LauncherCore.Utilities;
using ComponentUtil.Common.Data;

namespace CMCL.LauncherCore.Download.Mirrors.Interface
{
    public abstract class Asset
    {
        protected readonly string AssetIndexJsonDir =
            Utils.CombineAndCheckDirectory(false, AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "assets",
                "indexes");

        private BeforeDownloadStart _beforeDownloadStart;
        private OnDownloadFinish _onDownloadFinish;

        protected VersionManifest VersionManifest;
        protected virtual string Server { get; } = "";

        /// <summary>
        ///     下载资源目录json文件
        /// </summary>
        /// <param name="versionInfo"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected virtual async ValueTask GetAssetIndexJson(VersionInfo versionInfo)
        {
            //转换地址
            var url = TransUrl(versionInfo.AssetIndex.Url);

            var savePath =
                Utils.CombineAndCheckDirectory(true, AssetIndexJsonDir, Path.GetFileName(versionInfo.AssetIndex.Url));

            //不存在或sha1不一致则进行下载
            if (!File.Exists(savePath) ||
                !string.Equals(await Utils.GetSha1HashFromFileAsync(savePath).ConfigureAwait(false),
                    versionInfo.AssetIndex.Sha1, StringComparison.OrdinalIgnoreCase))
                await Downloader.GetFileAsync(Utils.HttpClientFactory.CreateClient(), url, savePath, null)
                    .ConfigureAwait(false);
        }

        /// <summary>
        ///     处理资源目录json文件，获取各项资源下载地址、sha1等信息
        ///     <param name="versionId"></param>
        /// </summary>
        /// <returns></returns>
        protected virtual async ValueTask<List<AssetsIndex.Asset>> HandleAssetIndexJson(string versionId)
        {
            var versionInfo = GameHelper.GetVersionInfo(versionId);
            if (versionInfo == null) throw new Exception("找不到指定版本");

            //确保资源json文件存在
            await GetAssetIndexJson(versionInfo);

            var savePath =
                Utils.CombineAndCheckDirectory(true, AssetIndexJsonDir, Path.GetFileName(versionInfo.AssetIndex.Url));

            //匹配每一条Asset信息
            var pattern = "\"\\S+\":\\s?{\"hash\":\\s\"\\w{40}\", \"size\":\\s?\\d+}";
            var matches = Regex.Matches(await File.ReadAllTextAsync(savePath).ConfigureAwait(false), pattern);

            //匹配Asset信息中的hash字段内容
            var hashPattern = "(?<=^\"\\S+\":\\s?{\"hash\":\\s\")\\w{40}(?=\", \"size\":\\s?\\d+})";
            //匹配Asset信息中的size字段内容
            var sizePattern = "(?<=^\"\\S+\":\\s?{\"hash\":\\s\"\\w{40}\", \"size\":\\s?)\\d+(?=})";

            var assetList = new List<AssetsIndex.Asset>();
            foreach (var match in matches.Where(m => m.Success))
            {
                var hashMatch = Regex.Match(match.Value, hashPattern);
                var sizeMatch = Regex.Match(match.Value, sizePattern);

                assetList.Add(new AssetsIndex.Asset
                {
                    Hash = hashMatch.Value,
                    Size = sizeMatch.Value.ToInt(0)
                });
            }

            return assetList;
        }

        /// <summary>
        ///     获取待下载资源文件的列表
        /// </summary>
        /// <param name="versionId">版本</param>
        /// <param name="checkBeforeDownload">下载前校验文件sha1，如正确则不重复下载</param>
        /// <returns></returns>
        public virtual async ValueTask<List<(string savePath, string downloadUrl)>> GetAssetsDownloadList(
            string versionId, bool checkBeforeDownload = false)
        {
            //处理各项asset信息
            var assetsIndex = await HandleAssetIndexJson(versionId).ConfigureAwait(false);
            var basePath = Path.Combine(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "assets", "objects");
            var assetsToDownload = new ConcurrentDictionary<string, string>();

            var sem = new SemaphoreSlim(AppConfig.GetAppConfig().MaxThreadCount);
            var taskArray = assetsIndex.Select(assetInfo => Task.Run(async () =>
            {
                var savePath = Utils.CombineAndCheckDirectory(true, basePath, assetInfo.SavePath);
                //转换地址
                var url = TransUrl(assetInfo.DownloadUrl);
                try
                {
                    if (!assetsToDownload.TryAdd(savePath, url))
                        return;

                    //校验sha1
                    await sem.WaitAsync();
                    if (checkBeforeDownload && File.Exists(savePath) && string.Equals(
                        await Utils.GetSha1HashFromFileAsync(savePath), assetInfo.Hash,
                        StringComparison.OrdinalIgnoreCase))
                        assetsToDownload.Remove(savePath, out var s);
                }
                finally
                {
                    sem.Release();
                }
            }));

            await Task.WhenAll(taskArray);

            return assetsToDownload.Select(i => (i.Key, assetsToDownload[i.Key].ToString())).ToList();
        }

        /// <summary>
        ///     下载Assets
        /// </summary>
        /// <param name="assetsToDownload">待下载列表</param>
        /// <returns></returns>
        public virtual async ValueTask DownloadAssets(List<(string savePath, string downloadUrl)> assetsToDownload)
        {
            var totalCount = assetsToDownload.Count;
            if (totalCount <= 0) return;

            var dic = new ConcurrentDictionary<string, int>();
            var sem = new SemaphoreSlim(AppConfig.GetAppConfig().MaxThreadCount);
            var finishedCount = 0;
            var taskArray = assetsToDownload.Select(assetInfo => Task.Run(async () =>
            {
                try
                {
                    await sem.WaitAsync();
                    if (!dic.TryAdd(assetInfo.downloadUrl, 0)) return;

                    _beforeDownloadStart?.Invoke("下载资源", totalCount, finishedCount);

                    await Downloader.GetFileAsync(Utils.HttpClientFactory.CreateClient(), assetInfo.downloadUrl,
                        assetInfo.savePath, null);

                    finishedCount++;
                    _onDownloadFinish?.Invoke("下载资源", totalCount, finishedCount);
                }
                finally
                {
                    sem.Release();
                }
            }));
            await Task.WhenAll(taskArray);
        }

        /// <summary>
        ///     转换下载地址
        /// </summary>
        /// <param name="originUrl"></param>
        /// <returns></returns>
        protected virtual string TransUrl(string originUrl)
        {
            if (originUrl.StartsWith("http"))
            {
                var originServers = new[] {"http://resources.download.minecraft.net"};

                return originServers.Aggregate(originUrl,
                    (current, originServer) => current.Replace(originServer, Server));
            }

            return $"{Server}/{originUrl}";
        }

        public event OnDownloadFinish OnDownloadFinish
        {
            add => _onDownloadFinish += value;
            remove => _onDownloadFinish -= value;
        }

        public event BeforeDownloadStart BeforeDownloadStart
        {
            add => _beforeDownloadStart += value;
            remove => _beforeDownloadStart -= value;
        }
    }
}