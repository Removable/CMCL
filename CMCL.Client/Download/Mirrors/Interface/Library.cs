using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CMCL.Client.Game;
using CMCL.Client.Util;
using CMCL.Client.Window;

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
                var savePath = IOHelper.CombineAndCheckDirectory(AppConfig.GetAppConfig().MinecraftDir, ".minecraft",
                    "libraries", libraries[i].Downloads.Artifact.Path);
                var newFunc = new Func<ValueTask>(async () =>
                {
                    await Downloader.GetFileAsync(GlobalStaticResource.HttpClientFactory.CreateClient(), url,
                        savePath, "下载Library文件");
                });
                funcArray[i] = newFunc;
            }

            return funcArray;
        }
        
        /// <summary>
        ///     下载Libraries
        /// </summary>
        /// <param name="librariesToDownload">待下载的库文件列表</param>
        /// <returns></returns>
        public virtual Func<ValueTask>[] DownloadLibrariesAsync(List<(string savePath, string downloadUrl)> librariesToDownload)
        {
            var funcArray = new Func<ValueTask>[librariesToDownload.Count];
            for (var i = 0; i < librariesToDownload.Count; i++)
            {
                var k = i;
                var newFunc = new Func<ValueTask>(async () =>
                {
                    await Downloader.GetFileAsync(GlobalStaticResource.HttpClientFactory.CreateClient(), librariesToDownload[k].downloadUrl,
                        librariesToDownload[k].savePath, "下载Library文件");
                });
                funcArray[i] = newFunc;
            }

            return funcArray;
        }

        /// <summary>
        /// 获取待下载资源文件的列表
        /// </summary>
        /// <param name="versionId">版本</param>
        /// <param name="checkBeforeDownload">下载前校验文件sha1，如正确则不重复下载</param>
        /// <returns></returns>
        public virtual async ValueTask<List<(string savePath, string downloadUrl)>> GetLibrariesDownloadList(
            string versionId, bool checkBeforeDownload = false)
        {
            var loadingFrm = LoadingFrm.GetInstance("", Application.Current.MainWindow);
            loadingFrm.Dispatcher.BeginInvoke(new Action(() =>
            {
                loadingFrm.LoadingControl.LoadingTip = "正在校验库文件...";
                loadingFrm.Show();
            }));
            var versionInfo = await GameHelper.GetVersionInfo(versionId).ConfigureAwait(false);
            var libraries = versionInfo.Libraries.Where(i => i.ShouldDeployOnOs()).ToList();
            var basePath = Path.Combine(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "libraries");
            var librariesToDownload = new List<(string savePath, string downloadUrl)>();

            var semaphore = new SemaphoreSlim(AppConfig.GetAppConfig().MaxThreadCount);
            await Task.WhenAll(libraries.Select(l => Task.Run(async () =>
            {
                await semaphore.WaitAsync();

                try
                {
                    var savePath = IOHelper.CombineAndCheckDirectory(true, basePath, l.Downloads.Artifact.Path);
                    //转换地址
                    var url = TransUrl(l.Downloads.Artifact.Url);

                    if (checkBeforeDownload && File.Exists(savePath) && string.Equals(
                        await IOHelper.GetSha1HashFromFileAsync(savePath), l.Downloads.Artifact.Sha1,
                        StringComparison.OrdinalIgnoreCase))
                        return;

                    librariesToDownload.Add((savePath, url));
                }
                catch (Exception e)
                {
                    await LogHelper.WriteLogAsync(e);
                    throw new Exception("校验库文件失败");
                }
                finally
                {
                    semaphore.Release();
                }
            })));

            loadingFrm.Dispatcher.BeginInvoke(new Action(() =>
            {
                loadingFrm.Hide();
            }));
            return librariesToDownload;
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