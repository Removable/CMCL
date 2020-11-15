﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CMCL.Client.Util;
using Flurl.Http;

namespace CMCL.Client.Download
{
    public class Downloader
    {
        public static DownloadPropertyChangedHandler DownloadInfoHandler = new DownloadPropertyChangedHandler();

        /// <summary>
        ///     异步获取字符串
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> GetStringAsync(string url)
        {
            using var response = await url.GetAsync();

            if (response.StatusCode == (int) HttpStatusCode.OK)
                return await response.ResponseMessage.Content.ReadAsStringAsync();
            throw new Exception($"获取地址出错，状态码：{response.StatusCode}");
        }

        /// <summary>
        ///     异步下载文件
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="progress">进度报告器</param>
        /// <param name="token">任务取消控制</param>
        /// <param name="directory">保存的文件夹</param>
        /// <param name="downloadInfo">下载信息</param>
        /// <returns></returns>
        public static async Task GetFileAsync(string url, IProgress<double> progress, string directory,
            DownloadInfo downloadInfo, CancellationToken token)
        {
            DownloadInfoHandler.CurrentDownloadProgress = 0;
            DownloadInfoHandler.CurrentDownloadFile = downloadInfo.CurrentFileName;
            DownloadInfoHandler.CurrentDownloadGroup =
                $"正在下载 {downloadInfo.CurrentCategory}({downloadInfo.CurrentFileIndex}/{downloadInfo.TotalFilesCount})";

            FileHelper.CreateDirectoryIfNotExist(directory);
            var filePath = Path.Combine(directory, downloadInfo.CurrentFileName);

            var uri = new Uri(url);

            //获取重定向后的响应
            var response = await GetFinalResponse(uri);

            var total = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = total != -1 && progress != null;

            try
            {
                //读取流并写到文件
                await using Stream stream = await response.Content.ReadAsStreamAsync(token),
                    fileStream = new FileStream(filePath, FileMode.Create,
                        FileAccess.Write, FileShare.None, 4096, true);
                var totalRead = 0L;
                var buffer = new byte[4096];
                var isMoreToRead = true;

                do
                {
                    token.ThrowIfCancellationRequested();

                    var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), token);

                    if (read == 0)
                    {
                        isMoreToRead = false;
                    }
                    else
                    {
                        var data = new byte[read];
                        buffer.ToList().CopyTo(0, data, 0, read);

                        await fileStream.WriteAsync(buffer.AsMemory(0, read), token);

                        totalRead += read;

                        //进度报告
                        if (canReportProgress)
                        {
                            //TODO 通过这里比上次多的数据差，统计下载速度
                            var progressValue = totalRead * 1d / (total * 1d) * 100;
                            if (progressValue >= 100 && downloadInfo.ReportFinish)
                                DownloadInfoHandler.DownloadFinished = true;
                            progress.Report(progressValue);
                        }
                    }
                } while (isMoreToRead);

                response.Dispose();
            }
            catch (Exception ex)
            {
                response.Dispose();
                //若以bangbang93.com源下载失败，切换mcbbs源尝试
                if (url.Contains("bangbang93.com"))
                {
                    url = url.Replace("bangbang93.com", "download.mcbbs.net");
                    if (!url.StartsWith("https"))
                    {
                        url.Replace("http", "https");
                    }

                    await GetFileAsync(url, progress, directory, downloadInfo, token);
                }
                else
                    throw new Exception("下载失败");
            }

            //获取经过重定向后的最终响应
            async Task<HttpResponseMessage> GetFinalResponse(Uri thisUri)
            {
                //实例化一个HttpClient并将允许自动重定向设为false
                var client = new HttpClient(new HttpClientHandler {AllowAutoRedirect = false});
                try
                {
                    response =
                        await client.GetAsync(thisUri.ToString(), HttpCompletionOption.ResponseHeadersRead, token);
                }
                catch
                {
                    return await GetFinalResponse(thisUri);
                }

                //若返回的状态码为302
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Found:
                        if (response.Headers.Location == null)
                            throw new Exception("未找到重定向地址");
                        //若重定向的新地址为相对地址，则将其重新拼接
                        if (!response.Headers.Location.IsAbsoluteUri)
                            thisUri = new Uri($"{thisUri.Scheme}://{thisUri.Host}{response.Headers.Location}");
                        else
                            thisUri = response.Headers.Location;
                        //递归
                        return await GetFinalResponse(thisUri);
                    case HttpStatusCode.OK:
                        return response;
                    default:
                        throw new Exception($"出错啦，状态码：{response.StatusCode}");
                }
            }
        }
    }
}