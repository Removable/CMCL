﻿using System;
using System.Collections.Concurrent;
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
        /// 下载Libraries
        /// </summary>
        /// <param name="versionId">游戏版本</param>
        /// <param name="checkBeforeDownload">下载前校验文件sha1，如正确则不重复下载</param>
        /// <returns></returns>
        public virtual async ValueTask DownloadLibrariesAsync(string versionId, bool checkBeforeDownload = false)
        {
            var loadingFrm = LoadingFrm.GetInstance("校验库文件", Application.Current.MainWindow);

            try
            {
                loadingFrm.Dispatcher.BeginInvoke(new Action(() => { loadingFrm.ShowDialog(); }));
                //获取待下载列表
                var librariesToDownload = await GetLibrariesDownloadList(versionId, checkBeforeDownload);
                var totalCount = librariesToDownload.Count;
                if (totalCount <= 0)
                {
                    loadingFrm.Dispatcher.BeginInvoke(new Action(() => { loadingFrm.Hide(); }));
                    return;
                }

                loadingFrm.Dispatcher.BeginInvoke(new Action(() =>
                {
                    loadingFrm.LoadingControl.LoadingTip = $"下载库(0/{totalCount.ToString()})";
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
                                $"下载库({finishedCount.ToString()}/{totalCount.ToString()})";
                        }));
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
        /// 获取待下载资源文件的列表
        /// </summary>
        /// <param name="versionId">版本</param>
        /// <param name="checkBeforeDownload">下载前校验文件sha1，如正确则不重复下载</param>
        /// <returns></returns>
        public virtual async ValueTask<List<(string savePath, string downloadUrl)>> GetLibrariesDownloadList(
            string versionId, bool checkBeforeDownload = false)
        {
            try
            {
                var versionInfo = GameHelper.GetVersionInfo(versionId);
                var hashSet = new HashSet<string>();
                var libraries = versionInfo.Libraries.Select(i => hashSet.Add(i.Downloads.Artifact.Sha1) ? i : null)
                    .Where(i => i != null && i.ShouldDeployOnOs()).ToList();

                var basePath = Path.Combine(AppConfig.GetAppConfig().MinecraftDir, ".minecraft", "libraries");

                var librariesToDownload = new ConcurrentDictionary<string, string>();
                var semaphore = new SemaphoreSlim(AppConfig.GetAppConfig().MaxThreadCount);
                var taskArray = libraries.Select(l => Task.Run(async () =>
                {
                    try
                    {
                        var savePath = IOHelper.CombineAndCheckDirectory(true, basePath, l.Downloads.Artifact.Path);
                        //转换地址
                        var url = TransUrl(l.Downloads.Artifact.Url);

                        await semaphore.WaitAsync();
                        if (checkBeforeDownload && File.Exists(savePath) && string.Equals(
                            await IOHelper.GetSha1HashFromFileAsync(savePath).ConfigureAwait(false),
                            l.Downloads.Artifact.Sha1, StringComparison.OrdinalIgnoreCase))
                            return;

                        librariesToDownload.TryAdd(savePath, url);
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
            catch (Exception e)
            {
                throw;
            }
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

        public async ValueTask UnzipNatives(string versionId)
        {
            var nativesList = GameHelper.GetVersionInfo(versionId).Libraries.Where(i => i.IsNative && i.ShouldDeployOnOs()).ToList();
        }
    }
}