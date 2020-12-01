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
    public static class Downloader
    {
        public static CancellationTokenSource DownloadCancellationToken = new CancellationTokenSource();
        public static DownloadPropertyChangedHandler DownloadInfoHandler = new DownloadPropertyChangedHandler();

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
        /// <param name="progress">进度报告器</param>
        /// <param name="directory">保存的文件夹</param>
        /// <param name="downloadInfo">下载信息</param>
        /// <returns></returns>
        public static async ValueTask GetFileAsync(HttpClient httpClient, string url, IProgress<double> progress,
            string directory, DownloadInfo downloadInfo)
        {
            DownloadInfoHandler.CurrentTaskProgress = 0;
            DownloadInfoHandler.CurrentDownloadSpeed = 0;
            DownloadInfoHandler.CurrentTaskName = downloadInfo.CurrentFileName;
            DownloadInfoHandler.CurrentTaskGroup =
                $"正在下载 {downloadInfo.CurrentCategory}({downloadInfo.CurrentFileIndex}/{downloadInfo.TotalFilesCount})";

            FileHelper.CreateDirectoryIfNotExist(directory);
            var filePath = Path.Combine(directory, downloadInfo.CurrentFileName);

            var uri = new Uri(url);

            //获取重定向后的响应
            var response = await GetFinalResponse(httpClient, uri, 0).ConfigureAwait(false);

            var total = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = total != -1 && progress != null;

            try
            {
                //读取流并写到文件
                await using Stream stream =
                        await response.Content.ReadAsStreamAsync(DownloadCancellationToken.Token).ConfigureAwait(false),
                    fileStream = new FileStream(filePath, FileMode.Create,
                        FileAccess.Write, FileShare.None, 4096, true);
                var totalRead = 0L;
                var buffer = new byte[4096];
                var isMoreToRead = true;

                do
                {
                    DownloadCancellationToken.Token.ThrowIfCancellationRequested();

                    var read = await stream
                        .ReadAsync(buffer.AsMemory(0, buffer.Length), DownloadCancellationToken.Token)
                        .ConfigureAwait(false);

                    if (read == 0)
                    {
                        isMoreToRead = false;
                    }
                    else
                    {
                        var data = new byte[read];
                        buffer.ToList().CopyTo(0, data, 0, read);

                        await fileStream.WriteAsync(buffer.AsMemory(0, read), DownloadCancellationToken.Token)
                            .ConfigureAwait(false);

                        totalRead += read;

                        //进度报告
                        if (canReportProgress)
                        {
                            //TODO 通过这里比上次多的数据差，统计下载速度
                            var progressValue = totalRead * 1d / (total * 1d) * 100;
                            if (progressValue >= 100 && downloadInfo.ReportFinish)
                                DownloadInfoHandler.TaskFinished = true;
                            // progress.Report(progressValue);
                            DownloadInfoHandler.CurrentTaskProgress = progressValue;
                        }
                    }
                } while (isMoreToRead);

                response.Dispose();
            }
            catch (Exception ex)
            {
                await LogHelper.WriteLogAsync(ex);
                response.Dispose();
                ////若以bmcl源下载失败，切换mcbbs源尝试
                //var bmclMirror = new BMCLMirror();
                //if (bmclMirror.IsCurrentMirror(url))
                //{
                //    url = bmclMirror.TranslateToCurrentMirrorUrl(url);

                //    await GetFileAsync(httpClient, url, progress, directory, downloadInfo).ConfigureAwait(false);
                //}
                //else
                throw new Exception("下载失败");
            }
        }

        /// <summary>
        ///     异步下载文件
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="progress"></param>
        /// <param name="url">下载地址</param>
        /// <param name="savePath">保存路径</param>
        /// <returns></returns>
        public static async ValueTask GetFileAsync(HttpClient httpClient, IProgress<double> progress, string url,
            string savePath)
        {
            DownloadInfoHandler.CurrentTaskProgress = 0;
            DownloadInfoHandler.CurrentDownloadSpeed = 0;
            DownloadInfoHandler.CurrentTaskName = Path.GetFileName(savePath);

            FileHelper.CreateDirectoryIfNotExist(Path.GetDirectoryName(savePath));

            var uri = new Uri(url);

            //获取重定向后的响应
            var response = await GetFinalResponse(httpClient, uri, 0).ConfigureAwait(false);

            var total = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = total != -1 && progress != null;

            try
            {
                //读取流并写到文件
                await using Stream stream =
                        await response.Content
                            .ReadAsStreamAsync(GlobalStaticResource.GetDownloadCancellationToken().Token)
                            .ConfigureAwait(false),
                    fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096,
                        true);
                var totalRead = 0L;
                var buffer = new byte[4096];
                var isMoreToRead = true;

                do
                {
                    GlobalStaticResource.GetDownloadCancellationToken().Token.ThrowIfCancellationRequested();

                    var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length),
                        GlobalStaticResource.GetDownloadCancellationToken().Token).ConfigureAwait(false);

                    if (read == 0)
                    {
                        isMoreToRead = false;
                    }
                    else
                    {
                        var data = new byte[read];
                        buffer.ToList().CopyTo(0, data, 0, read);

                        await fileStream.WriteAsync(buffer.AsMemory(0, read),
                            GlobalStaticResource.GetDownloadCancellationToken().Token).ConfigureAwait(false);

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
                await LogHelper.WriteLogAsync(ex);
                response.Dispose();
                throw new Exception("下载失败");
            }
        }

        //public static ValueTask CheckFileSha1(string filePath, IProgress<double> progress, string fileName = "")
        //{
        //    fileName = string.IsNullOrWhiteSpace(fileName) ? Path.GetFileName(filePath) : fileName;
        //    DownloadInfoHandler.CurrentTaskProgress = 0;
        //    DownloadInfoHandler.CurrentTaskName = $"正在校验 {fileName}";
        //    DownloadInfoHandler.CurrentTaskGroup = "";
        //}

        /// <summary>
        /// 获取经过重定向后的最终响应
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
                await LogHelper.WriteLogAsync(ex);
                if (tryCount <= 3)
                    return await GetFinalResponse(httpClient, thisUri, tryCount + 1).ConfigureAwait(false);
                throw;
            }
        }
    }
}