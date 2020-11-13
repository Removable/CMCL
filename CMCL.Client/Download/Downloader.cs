using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            FlurlHttp.Configure(settings =>
            {
                settings.Redirects.AllowSecureToInsecure = true;
                settings.Redirects.Enabled = true;
                settings.Redirects.MaxAutoRedirects = 5;
            });
            var response = await url.GetAsync();

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
            var uri = new Uri(url);

            //实例化一个HttpClient并将允许自动重定向设为false
            var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
            //若返回的状态码为302
            if (response.StatusCode == HttpStatusCode.Found)
            {
                //若重定向的新地址为相对地址，则将其重新拼接
                url = response.Headers.Location.ToString();
                if (!response.Headers.Location.IsAbsoluteUri) url = $"{uri.Scheme}://{uri.Host}{url}";
                //递归
                await GetFileAsync(url, progress, directory, downloadInfo, token);
                return;
            }

            //若返回了其他表示错误的状态码
            if (!response.IsSuccessStatusCode) throw new Exception($"出错啦，状态码：{response.StatusCode}");

            var total = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = total != -1 && progress != null;

            //读取流并写到文件
            await using Stream stream = await response.Content.ReadAsStreamAsync(token),
                fileStream = new FileStream(Path.Combine(directory, downloadInfo.CurrentFileName), FileMode.Create,
                    FileAccess.Write, FileShare.None, 8192, true);
            var totalRead = 0L;
            var buffer = new byte[4096];
            var isMoreToRead = true;

            do
            {
                token.ThrowIfCancellationRequested();

                var read = await stream.ReadAsync(buffer, 0, buffer.Length, token);

                if (read == 0)
                {
                    isMoreToRead = false;
                }
                else
                {
                    var data = new byte[read];
                    buffer.ToList().CopyTo(0, data, 0, read);

                    await fileStream.WriteAsync(buffer, 0, read, token);

                    totalRead += read;

                    //进度报告
                    if (canReportProgress)
                    {
                        var progressValue = totalRead * 1d / (total * 1d) * 100;
                        if (progressValue >= 100 && downloadInfo.ReportFinish)
                            DownloadInfoHandler.DownloadFinished = true;
                        progress.Report(progressValue);
                    }
                }
            } while (isMoreToRead);
        }
    }
}