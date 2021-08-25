using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CMCL.LauncherCore.Utilities;
using Downloader;
using Flurl.Http;
using DownloadProgressChangedEventArgs = Downloader.DownloadProgressChangedEventArgs;

namespace CMCL.LauncherCore.Download
{
    public static class Downloader
    {

        /// <summary>
        ///     异步获取字符串
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> GetStringAsync(string url)
        {
            var json = await url.GetStringAsync();
            return json;
        }

        /*
        /// <summary>
        ///     异步下载文件
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="url">下载地址</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="taskName">任务名</param>
        /// <returns></returns>
        public static async Task GetFileAsync(HttpClient httpClient, string url, string savePath,
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
        */

        /// <summary>
        /// 获取Downloader配置
        /// </summary>
        /// <returns></returns>
        public static DownloadConfiguration GetConfiguration()
        {
            return new DownloadConfiguration
            {
                BufferBlockSize = 4096, // usually, hosts support max to 8000 bytes, default values is 8000
                ChunkCount = 1, // file parts to download, default value is 1
                // MaximumBytesPerSecond = 1024 * 1024, // download speed limited to 1MB/s, default values is zero or unlimited
                MaxTryAgainOnFailover = 3, // the maximum number of times to fail
                OnTheFlyDownload = true, // caching in-memory or not? default values is true
                ParallelDownload = true, // download parts of file as parallel or not. Default value is false
                TempDirectory =
                    Path.GetTempPath(), // Set the temp path for buffering chunk files, the default path is Path.GetTempPath()
                Timeout = 3000, // timeout (millisecond) per stream block reader, default values is 1000
                RequestConfiguration = // config and customize request headers
                {
                    Accept = "*/*",
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    CookieContainer = new CookieContainer(), // Add your cookies
                    Headers = new WebHeaderCollection(), // Add your custom headers
                    KeepAlive = false,
                    ProtocolVersion = HttpVersion.Version11, // Default value is HTTP 1.1
                    UseDefaultCredentials = false,
                    UserAgent =
                        "CMCL", //$"DownloaderSample/{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}"
                }
            };
        }
    }
}