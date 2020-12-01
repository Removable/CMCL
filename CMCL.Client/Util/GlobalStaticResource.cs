using System.Net.Http;
using System.Threading;

namespace CMCL.Client.Util
{
    public static class GlobalStaticResource
    {
        /// <summary>
        ///     HttpClientFactory
        /// </summary>
        public static IHttpClientFactory HttpClientFactory;

        #region 获取下载CancellationTokenSource

        private static CancellationTokenSource _downloadCancellationTokenSource;

        /// <summary>
        ///     获取下载CancellationTokenSource
        /// </summary>
        /// <returns></returns>
        public static CancellationTokenSource GetDownloadCancellationToken()
        {
            if (_downloadCancellationTokenSource == null || _downloadCancellationTokenSource.IsCancellationRequested)
                _downloadCancellationTokenSource = new CancellationTokenSource();

            return _downloadCancellationTokenSource;
        }

        #endregion
    }
}