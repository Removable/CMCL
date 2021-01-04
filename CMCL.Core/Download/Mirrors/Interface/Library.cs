using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CMCL.Core.Util;

namespace CMCL.Core.Download.Mirrors.Interface
{
    public abstract class Library
    {
        /// <summary>
        ///     下载Libraries
        /// </summary>
        /// <param name="librariesToDownload">待下载的列表</param>
        /// <returns></returns>
        public virtual async ValueTask DownloadLibrariesAsync(List<(string savePath, string downloadUrl)> librariesToDownload)
        {
            var totalCount = librariesToDownload.Count;
            if (totalCount <= 0) return;

            var dic = new ConcurrentDictionary<string, int>();
            var sem = new SemaphoreSlim(AppConfig.GetAppConfig().MaxThreadCount);
            var finishedCount = 0;
            var taskArray = librariesToDownload.Select(libraryInfo => Task.Run(async () =>
            {
                try
                {
                    await sem.WaitAsync();
                    if (!dic.TryAdd(libraryInfo.downloadUrl, 0)) return;

                    finishedCount++;
                    GlobalStaticResource.LoadingFrmDataContext.CurrentLoadingTip = $"下载库({finishedCount.ToString()}/{totalCount.ToString()})";
                    await Downloader.GetFileAsync(GlobalStaticResource.HttpClientFactory.CreateClient(),
                        libraryInfo.downloadUrl, libraryInfo.savePath, "").ConfigureAwait(false);
                }
                finally
                {
                    sem.Release();
                }
            }));
            await Task.WhenAll(taskArray);
        }

        /// <summary>
        ///     获取待下载资源文件的列表
        /// </summary>
        /// <param name="versionId">版本</param>
        /// <param name="checkBeforeDownload">下载前校验文件sha1，如正确则不重复下载</param>
        /// <param name="containsNative">是否包含native库文件</param>
        /// <returns></returns>
        public virtual async ValueTask<List<(string savePath, string downloadUrl)>> GetLibrariesDownloadList(
            string versionId, bool checkBeforeDownload = false, bool containsNative = true)
        {
            var versionInfo = GameHelper.GetVersionInfo(versionId);
            var libraries = versionInfo.Libraries.Where(i => i != null && i.ShouldDeployOnOs(Utils.GetOS()))
                .ToList();

            var basePath = Path.Combine(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "libraries");

            var librariesToDownload = new ConcurrentDictionary<string, string>();
            var semaphore = new SemaphoreSlim(AppConfig.GetAppConfig().MaxThreadCount);
            var taskArray = libraries.Select(l => Task.Run(async () =>
            {
                try
                {
                    await semaphore.WaitAsync();
                    if (!l.IsNative)
                    {
                        var savePath = IOHelper.CombineAndCheckDirectory(true, basePath, l.Downloads.Artifact.Path);
                        //转换地址
                        var url = TransUrl(l.Downloads.Artifact.Url);
                        if (checkBeforeDownload && File.Exists(savePath) && string.Equals(
                            await IOHelper.GetSha1HashFromFileAsync(savePath).ConfigureAwait(false),
                            l.Downloads.Artifact.Sha1, StringComparison.OrdinalIgnoreCase))
                            return;

                        librariesToDownload.TryAdd(savePath, url);
                    }
                    else if (containsNative)
                    {
                        var nativeInfo = l.GetNative(Utils.GetOS());
                        var savePath = IOHelper.CombineAndCheckDirectory(true, basePath, nativeInfo.Path);
                        //转换地址
                        var url = TransUrl(nativeInfo.Url);
                        if (checkBeforeDownload && File.Exists(savePath) && string.Equals(
                            await IOHelper.GetSha1HashFromFileAsync(savePath).ConfigureAwait(false),
                            nativeInfo.Sha1, StringComparison.OrdinalIgnoreCase))
                            return;

                        librariesToDownload.TryAdd(savePath, url);
                    }
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
            }));
            await Task.WhenAll(taskArray);

            return librariesToDownload.Select(i => (i.Key, librariesToDownload[i.Key].ToString())).ToList();
        }

        /// <summary>
        ///     转换下载地址
        /// </summary>
        /// <param name="originUrl"></param>
        /// <returns></returns>
        protected virtual string TransUrl(string originUrl)
        {
            const string server = "";
            var originServers = new[] {"https://libraries.minecraft.net/", "https://files.minecraftforge.net/maven/"};

            return originServers.Aggregate(originUrl, (current, originServer) => current.Replace(originServer, server));
        }

        /// <summary>
        ///     解压native库文件
        /// </summary>
        /// <param name="versionId"></param>
        /// <returns></returns>
        public async ValueTask UnzipNatives(string versionId)
        {
            var nativesList = GameHelper.GetVersionInfo(versionId).Libraries
                .Where(i => i.IsNative && i.ShouldDeployOnOs(Utils.GetOS())).ToList();

            var nativesDir = GameHelper.GetNativesDir(versionId);

            var taskArray = nativesList.Select(native => Task.Run(async () =>
            {
                var relativePath = native.GetNativePath();
                var absolutePath = Path.Combine(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "libraries",
                    relativePath);
                if (!File.Exists(absolutePath))
                    return;
                await using var fileStream = File.OpenRead(absolutePath);
                using var zip = new ZipArchive(fileStream);
                foreach (var entry in zip.Entries)
                {
                    if (entry.FullName.Contains("META-INF/") || string.IsNullOrWhiteSpace(entry.Name)) continue;
                    await using var stream = entry.Open();
                    await using var f =
                        File.Create(Path.Combine(nativesDir, entry.Name));
                    await stream.CopyToAsync(f);
                }
            }));

            await Task.WhenAll(taskArray);
        }
    }
}