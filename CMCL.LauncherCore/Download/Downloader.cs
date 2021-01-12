using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CMCL.LauncherCore.Utilities;

namespace CMCL.LauncherCore.Download
{
    public static class Downloader
    {
        public static CancellationTokenSource DownloadCancellationToken = new();

        /// <summary>
        ///     异步获取字符串
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async ValueTask<string> GetStringAsync(HttpClient httpClient, string url)
        {
            using var response = await GetFinalResponse(httpClient, new Uri(url), 0).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return json;
            }

            throw new Exception($"获取地址出错，状态码：{response.StatusCode}");
        }

        /// <summary>
        ///     异步下载文件
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="url">下载地址</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="taskName">任务名</param>
        /// <returns></returns>
        public static async ValueTask GetFileAsync(HttpClient httpClient, string url, string savePath,
            IProgress<double> progress)
        {
            Utils.CombineAndCheckDirectory(true, Path.GetDirectoryName(savePath));

            var uri = new Uri(url);

            //获取重定向后的响应
            var response = await GetFinalResponse(httpClient, uri, 0).ConfigureAwait(false);

            var total = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = total != -1 && progress != null;

            try
            {
                //读取流并写到文件
                await using Stream stream =
                        await response.Content.ReadAsStreamAsync(GameHelper.GetDownloadCancellationToken().Token)
                            .ConfigureAwait(false),
                    fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096,
                        true);
                var totalRead = 0L;
                var buffer = new byte[4096];
                var isMoreToRead = true;

                do
                {
                    GameHelper.GetDownloadCancellationToken().Token.ThrowIfCancellationRequested();

                    var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length),
                        GameHelper.GetDownloadCancellationToken().Token).ConfigureAwait(false);

                    if (read == 0)
                    {
                        isMoreToRead = false;
                    }
                    else
                    {
                        var data = new byte[read];
                        buffer.ToList().CopyTo(0, data, 0, read);

                        await fileStream.WriteAsync(buffer.AsMemory(0, read),
                            GameHelper.GetDownloadCancellationToken().Token).ConfigureAwait(false);

                        totalRead += read;

                        //进度报告
                        if (canReportProgress)
                        {
                            var progressValue = totalRead * 1d / (total * 1d) * 100;
                            progress.Report(progressValue);
                        }
                    }
                } while (isMoreToRead);

                response.Dispose();
            }
            catch (Exception ex)
            {
                await LogHelper.LogExceptionAsync(ex);
                response.Dispose();
                throw new Exception("下载失败");
            }
        }

        /// <summary>
        ///     获取经过重定向后的最终响应
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="thisUri"></param>
        /// <param name="token"></param>
        /// <param name="tryCount"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static async ValueTask<HttpResponseMessage> GetFinalResponse(HttpClient httpClient, Uri thisUri,
            int tryCount)
        {
            try
            {
                var response = await httpClient.GetAsync(thisUri.ToString(),
                    HttpCompletionOption.ResponseHeadersRead, DownloadCancellationToken.Token).ConfigureAwait(false);

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
                        return await GetFinalResponse(httpClient, thisUri, tryCount).ConfigureAwait(false);
                    case HttpStatusCode.OK:
                        return response;
                    default:
                        throw new Exception($"出错啦，状态码：{response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                await LogHelper.LogExceptionAsync(ex);
                if (tryCount <= 3)
                    return await GetFinalResponse(httpClient, thisUri, tryCount + 1).ConfigureAwait(false);
                throw;
            }
        }
    }
}