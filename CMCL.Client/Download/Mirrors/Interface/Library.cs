using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CMCL.Client.Game;
using CMCL.Client.Util;

namespace CMCL.Client.Download.Mirrors.Interface
{
    public abstract class Library
    {
        /// <summary>
        ///     下载Libraries
        /// </summary>
        /// <param name="versionId">游戏版本号</param>
        /// <returns></returns>
        public virtual async ValueTask<Func<ValueTask>[]> DownloadLibrariesAsync(string versionId)
        {
            var versionInfo = await GameHelper.GetVersionInfo(versionId).ConfigureAwait(false);
            var libraries = versionInfo.Libraries.Where(i => i.ShouldDeployOnOs()).ToList();

            var funcArray = new Func<ValueTask>[libraries.Count];
            for (var i = 0; i < libraries.Count; i++)
            {
                //转换地址
                var url = TransUrl(libraries[i].Downloads.Artifact.Url);
                var savePath = IOHelper.CombineAndCheckDirectory(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "libraries",
                    libraries[i].Downloads.Artifact.Path);
                var newFunc = new Func<ValueTask>(async () =>
                {
                    await Downloader.GetFileAsync(GlobalStaticResource.HttpClientFactory.CreateClient(), url,
                        savePath);
                });
                funcArray[i] = newFunc;
            }

            return funcArray;
        }

        /// <summary>
        /// 转换下载地址
        /// </summary>
        /// <param name="originUrl"></param>
        /// <returns></returns>
        protected virtual string TransUrl(string originUrl)
        {
            const string server = "";
            var originServers = new[] {"https://libraries.minecraft.net/", "https://files.minecraftforge.net/maven/"};

            return originServers.Aggregate(originUrl, (current, originServer) => current.Replace(originServer, server));
        }
    }
}