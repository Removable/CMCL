using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CMCL.Client.Game;
using CMCL.Client.GameVersion.JsonClasses;
using CMCL.Client.Util;
using ComponentUtil.Common.Data;

namespace CMCL.Client.Download.Mirrors.Interface
{
    public abstract class Asset
    {
        protected GameVersionManifest VersionManifest;
        protected virtual string Server { get; } = "";

        protected string AssetIndexJsonDir =
            IOHelper.CombineAndCheckDirectory(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "assets", "indexes");

        /// <summary>
        /// 下载资源目录json文件
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="FileSha1Error"></exception>
        public virtual async ValueTask GetAssetIndexJson(string versionId)
        {
            var versionInfo = await GameHelper.GetVersionInfo(versionId).ConfigureAwait(false);
            if (versionInfo == null) throw new Exception("找不到指定版本");

            //转换地址
            var url = TransUrl(versionInfo.AssetIndex.Url);

            var savePath =
                IOHelper.CombineAndCheckDirectory(AssetIndexJsonDir, Path.GetFileName(versionInfo.AssetIndex.Url));

            //不存在就下载
            if (!File.Exists(savePath))
            {
                await Downloader.GetFileAsync(GlobalStaticResource.HttpClientFactory.CreateClient(), url, savePath, "")
                    .ConfigureAwait(false);
            }

            //校验sha1，如果sha1不正确就重新下载一次
            var sha1 = await IOHelper.GetSha1HashFromFileAsync(savePath);
            if (!string.Equals(sha1, versionInfo.AssetIndex.Sha1, StringComparison.OrdinalIgnoreCase))
            {
                await Downloader.GetFileAsync(GlobalStaticResource.HttpClientFactory.CreateClient(), url, savePath, "")
                    .ConfigureAwait(false);
                sha1 = await IOHelper.GetSha1HashFromFileAsync(savePath);
                if (!string.Equals(sha1, versionInfo.AssetIndex.Sha1, StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(savePath);
                    throw new FileSha1Error("资源文件下载错误，请重试");
                }
            }
        }

        /// <summary>
        /// 处理资源目录json文件，获取各项资源下载地址、sha1等信息
        /// <param name="versionId"></param>
        /// </summary>
        /// <returns></returns>
        protected virtual async ValueTask<List<AssetsIndex.Asset>> HandleAssetIndexJson(string versionId)
        {
            var versionInfo = await GameHelper.GetVersionInfo(versionId).ConfigureAwait(false);
            if (versionInfo == null) throw new Exception("找不到指定版本");

            var savePath =
                IOHelper.CombineAndCheckDirectory(AssetIndexJsonDir, Path.GetFileName(versionInfo.AssetIndex.Url));

            var pattern = "\"\\S+\":\\s?{\"hash\":\\s\"\\w{40}\", \"size\":\\s?\\d+}";
            var matches = Regex.Matches(await File.ReadAllTextAsync(savePath).ConfigureAwait(false), pattern);

            // var pathPattern = "^\"\\S+\"(?=:\\s?{\"hash\":\\s\"\\w{40}\", \"size\":\\s?\\d+})";
            var hashPattern = "(?<=^\"\\S+\":\\s?{\"hash\":\\s\")\\w{40}(?=\", \"size\":\\s?\\d+})";
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
        /// 下载资源文件
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public virtual async ValueTask<Func<ValueTask>[]> DownloadAssets(string versionId)
        {
            var assetsIndex = await HandleAssetIndexJson(versionId);

            var funcArray = new List<Func<ValueTask>>();
            for (var i = 0; i < assetsIndex.Count; i++)
            {
                //转换地址
                var url = TransUrl(assetsIndex[i].DownloadUrl);
                var savePath = IOHelper.CombineAndCheckDirectory(true, AppConfig.GetAppConfig().MinecraftDir,
                    ".minecraft", "assets", "objects", assetsIndex[i].SavePath);
                if (!File.Exists(savePath) || !string.Equals(await IOHelper.GetSha1HashFromFileAsync(savePath),
                    assetsIndex[i].Hash, StringComparison.OrdinalIgnoreCase))
                {
                    var newFunc = new Func<ValueTask>(async () =>
                    {
                        await Downloader.GetFileAsync(GlobalStaticResource.HttpClientFactory.CreateClient(), url,
                            savePath, "下载Asset文件");
                    });
                    funcArray.Add(newFunc);
                }
            }

            return funcArray.ToArray();
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