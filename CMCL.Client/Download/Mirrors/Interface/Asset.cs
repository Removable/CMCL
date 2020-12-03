using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CMCL.Client.Game;
using CMCL.Client.Util;

namespace CMCL.Client.Download.Mirrors.Interface
{
    public abstract class Asset
    {
        protected GameVersionManifest VersionManifest;
        protected virtual string Server { get; } = "";
        protected string AssetIndexJsonStr = string.Empty;

        /// <summary>
        /// 下载资源目录json问卷
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

            var savePath = IOHelper.CombineAndCheckDirectory(GameHelper.GetCmclCacheDir(false), "AssetJson",
                Path.GetFileName(versionInfo.AssetIndex.Url));
            await Downloader.GetFileAsync(GlobalStaticResource.HttpClientFactory.CreateClient(), url, savePath)
                .ConfigureAwait(false);

            //校验sha1
            var sha1 = await IOHelper.GetSha1HashFromFileAsync(savePath);
            if (!string.Equals(sha1, versionInfo.AssetIndex.Sha1, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(savePath);
                throw new FileSha1Error("资源文件下载错误，请重试");
            }

            AssetIndexJsonStr = await File.ReadAllTextAsync(savePath);
        }

        /// <summary>
        /// 处理资源目录json文件，获取各项资源下载地址、sha1等信息
        /// </summary>
        /// <returns></returns>
        public virtual async ValueTask HandleAssetIndexJson()
        {
            var pattern = "\"\\S+\":\\s?{\"hash\":\\s\"\\w{40}\", \"size\":\\s?\\d+}";
            var matches = Regex.Matches(AssetIndexJsonStr, pattern);
        }

        /// <summary>
        /// 转换下载地址
        /// </summary>
        /// <param name="originUrl"></param>
        /// <returns></returns>
        protected virtual string TransUrl(string originUrl)
        {
            const string server = "";
            var originServers = new[] {"http://resources.download.minecraft.net"};

            return originServers.Aggregate(originUrl, (current, originServer) => current.Replace(originServer, server));
        }
    }
}