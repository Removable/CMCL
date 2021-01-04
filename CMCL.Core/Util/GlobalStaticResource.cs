#nullable enable
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using CMCL.Core.Annotations;

namespace CMCL.Core.Util
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

        public static LoadingFrmPropertyChangedHandler LoadingFrmDataContext = new();
    }

    /// <summary>
    /// LoadingFrm数据绑定上下文
    /// </summary>
    public sealed class LoadingFrmPropertyChangedHandler : INotifyPropertyChanged
    {
        private string _currentLoadingTip;
        
        /// <summary>
        ///     当前LoadingFrm显示的提示语
        /// </summary>
        public string CurrentLoadingTip
        {
            get => _currentLoadingTip;
            set
            {
                _currentLoadingTip = value;
                OnPropertyChanged(nameof(CurrentLoadingTip));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}