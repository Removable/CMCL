using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CMCL.Client.Game;
using CMCL.Client.GameVersion.JsonClasses;
using CMCL.Client.Util;
using CMCL.Client.Window;
using ComponentUtil.Common.Data;

namespace CMCL.Client.Download.Mirrors.Interface
{
    public abstract class Asset
    {
        protected GameVersionManifest VersionManifest;
        protected virtual string Server { get; } = "";

        protected readonly string AssetIndexJsonDir =
            IOHelper.CombineAndCheckDirectory(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "assets", "indexes");

        /// <summary>
        /// 下载资源目录json文件
        /// </summary>
        /// <param name="versionInfo"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="FileSha1Error"></exception>
        protected virtual async ValueTask GetAssetIndexJson(VersionInfo versionInfo)
        {
            //转换地址
            var url = TransUrl(versionInfo.AssetIndex.Url);

            var savePath =
                IOHelper.CombineAndCheckDirectory(AssetIndexJsonDir, Path.GetFileName(versionInfo.AssetIndex.Url));

            //不存在或sha1不一致则进行下载
            if (!File.Exists(savePath) ||
                !string.Equals(await IOHelper.GetSha1HashFromFileAsync(savePath).ConfigureAwait(false),
                    versionInfo.AssetIndex.Sha1, StringComparison.OrdinalIgnoreCase))
            {
                await Downloader.GetFileAsync(GlobalStaticResource.HttpClientFactory.CreateClient(), url, savePath, "")
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 处理资源目录json文件，获取各项资源下载地址、sha1等信息
        /// <param name="versionId"></param>
        /// </summary>
        /// <returns></returns>
        protected virtual async ValueTask<List<AssetsIndex.Asset>> HandleAssetIndexJson(string versionId)
        {
            var versionInfo = GameHelper.GetVersionInfo(versionId);
            if (versionInfo == null) throw new Exception("找不到指定版本");

            //确保资源json文件存在
            await GetAssetIndexJson(versionInfo);

            var savePath =
                IOHelper.CombineAndCheckDirectory(AssetIndexJsonDir, Path.GetFileName(versionInfo.AssetIndex.Url));

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
                    Size = sizeMatch.Value.ToInt(0),
                });
            }

            return assetList;
        }

        /// <summary>
        /// 获取待下载资源文件的列表
        /// </summary>
        /// <param name="versionId">版本</param>
        /// <param name="checkBeforeDownload">下载前校验文件sha1，如正确则不重复下载</param>
        /// <returns></returns>
        public virtual async ValueTask<List<(string savePath, string downloadUrl)>> GetAssetsDownloadList(
            string versionId, bool checkBeforeDownload = false)
        {
            //处理各项asset信息
            var assetsIndex = await HandleAssetIndexJson(versionId);
            var basePath = Path.Combine(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "assets", "objects");
            var assetsToDownload = new List<(string savePath, string downloadUrl)>();

            foreach (var asset in assetsIndex)
            {
                var savePath = IOHelper.CombineAndCheckDirectory(true, basePath, asset.SavePath);
                //转换地址
                var url = TransUrl(asset.DownloadUrl);
                //校验sha1
                if (checkBeforeDownload && File.Exists(savePath) && string.Equals(
                    await IOHelper.GetSha1HashFromFileAsync(savePath), asset.Hash,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                assetsToDownload.Add((savePath, url));
            }

            return assetsToDownload;
        }

        /// <summary>
        /// 下载Libraries
        /// </summary>
        /// <param name="versionId">游戏版本</param>
        /// <param name="checkBeforeDownload">下载前校验文件sha1，如正确则不重复下载</param>
        /// <returns></returns>
        public virtual async ValueTask DownloadAssets(string versionId, bool checkBeforeDownload = false)
        {
            var loadingFrm = LoadingFrm.GetInstance("校验资源文件", Application.Current.MainWindow);

            try
            {
                loadingFrm.Dispatcher.BeginInvoke(new Action(() => { loadingFrm.ShowDialog(); }));
                //获取待下载列表
                var librariesToDownload = await GetAssetsDownloadList(versionId, checkBeforeDownload);
                var totalCount = librariesToDownload.Count;
                if (totalCount <= 0)
                {
                    loadingFrm.Dispatcher.BeginInvoke(new Action(() => { loadingFrm.Hide(); }));
                    return;
                }

                loadingFrm.Dispatcher.BeginInvoke(new Action(() =>
                {
                    loadingFrm.LoadingControl.LoadingTip = $"下载资源(0/{totalCount.ToString()})";
                }));
                var dic = new ConcurrentDictionary<string, int>();
                var sem = new SemaphoreSlim(AppConfig.GetAppConfig().MaxThreadCount);
                var finishedCount = 0;
                var taskArray = librariesToDownload.Select(libraryInfo => Task.Run(async () =>
                {
                    try
                    {
                        await sem.WaitAsync();
                        if (!dic.TryAdd(libraryInfo.downloadUrl, 0))
                        {
                            return;
                        }

                        finishedCount++;
                        loadingFrm.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            loadingFrm.LoadingControl.LoadingTip =
                                $"下载资源({finishedCount.ToString()}/{totalCount.ToString()})";
                        }));
                        await Downloader.GetFileAsync(GlobalStaticResource.HttpClientFactory.CreateClient(),
                            libraryInfo.downloadUrl, libraryInfo.savePath, "");
                    }
                    finally
                    {
                        sem.Release();
                    }
                }));
                await Task.WhenAll(taskArray);
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                loadingFrm.Hide();
            }
        }

        /// <summary>
        /// 转换下载地址
        /// </summary>
        /// <param name="originUrl"></param>
        /// <returns></returns>
        protected string TransUrl(string originUrl)
        {
            if (originUrl.StartsWith("http"))
            {
                var originServers = new[] {"http://resources.download.minecraft.net"};

                return originServers.Aggregate(originUrl,
                    (current, originServer) => current.Replace(originServer, Server));
            }

            return $"{Server}/{originUrl}";
        }
    }
}