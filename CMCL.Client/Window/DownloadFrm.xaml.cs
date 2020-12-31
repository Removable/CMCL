using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CMCL.Client.Download;
using CMCL.Client.Util;

namespace CMCL.Client.Window
{
    public partial class DownloadFrm : System.Windows.Window
    {
        private static DownloadFrm _downloadFrm;

        private DownloadFrm()
        {
            InitializeComponent();
        }

        ~DownloadFrm()
        {
            _downloadFrm = null;
        }

        /// <summary>
        ///     获取单例实例
        /// </summary>
        /// <returns></returns>
        public static DownloadFrm GetInstance(System.Windows.Window owner = null)
        {
            var frm = _downloadFrm ??= new DownloadFrm();
            if (owner != null && owner != _downloadFrm.Owner) frm.Owner = owner;

            return frm;
        }

        /// <summary>
        ///     打开窗口=>执行任务=>关闭窗口
        /// </summary>
        /// <param name="disappearType">任务执行结束后是如何处理窗口</param>
        /// <param name="funcs">要执行的任务数组</param>
        public async Task DoWork(WindowDisappear disappearType, params Func<ValueTask>[] funcs)
        {
            DataContext = Downloader.DownloadInfoHandler;
            var currentTaskIndex = 0;
            Show();

            foreach (var func in funcs)
            {
                currentTaskIndex++;
                var index = currentTaskIndex;
                Downloader.DownloadInfoHandler.CurrentTaskGroup = $"({index.ToString()}/{funcs.Length.ToString()})";
                if (func == null) continue;
                await func();
            }

            switch (disappearType)
            {
                default:
                case WindowDisappear.None:
                    break;
                case WindowDisappear.Close:
                    Close();
                    break;
                case WindowDisappear.Hide:
                    Hide();
                    break;
            }
        }

        private void DownloadFrm_OnClosed(object sender, EventArgs e)
        {
            _downloadFrm = null;
            GlobalStaticResource.GetDownloadCancellationToken().Cancel();
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}