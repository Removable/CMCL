using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CMCL.Client.Util;

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
            using var response = await GetFinalResponse(new Uri(url), default(CancellationToken), 0);

            if (response.StatusCode == HttpStatusCode.OK)
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
            DownloadInfoHandler.CurrentDownloadSpeed = 0;
            DownloadInfoHandler.CurrentDownloadFile = downloadInfo.CurrentFileName;
            DownloadInfoHandler.CurrentDownloadGroup =
                $"正在下载 {downloadInfo.CurrentCategory}({downloadInfo.CurrentFileIndex}/{downloadInfo.TotalFilesCount})";

            FileHelper.CreateDirectoryIfNotExist(directory);
            var filePath = Path.Combine(directory, downloadInfo.CurrentFileName);

            var uri = new Uri(url);

            //获取重定向后的响应
            var response = await GetFinalResponse(uri, token, 0);

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
                //若以bmcl源下载失败，切换mcbbs源尝试
                var bmclMirror = new BMCLMirror();
                if (bmclMirror.IsCurrentMirror(url))
                {
                    url = bmclMirror.TranslateToCurrentMirrorUrl(url);

                    await GetFileAsync(url, progress, directory, downloadInfo, token);
                }
                else
                    throw new Exception("下载失败");
            }
        }

        /// <summary>
        /// 获取经过重定向后的最终响应
        /// </summary>
        /// <param name="thisUri"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static async Task<HttpResponseMessage> GetFinalResponse(Uri thisUri, CancellationToken token,
            int tryCount)
        {
            try
            {
                using var clientPack = HttpClientPool.GetHttpClient();
                var response = await clientPack.client.GetAsync(thisUri.ToString(),
                    HttpCompletionOption.ResponseHeadersRead, token);

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
                        return await GetFinalResponse(thisUri, token, tryCount);
                    case HttpStatusCode.OK:
                        return response;
                    default:
                        throw new Exception($"出错啦，状态码：{response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                if (tryCount <= 3)
                    return await GetFinalResponse(thisUri, token, tryCount + 1); 
                throw;
            }
        }
    }
}