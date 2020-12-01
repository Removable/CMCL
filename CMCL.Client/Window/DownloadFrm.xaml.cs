using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CMCL.Client.Download;

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
        public static DownloadFrm GetInstance()
        {
            return _downloadFrm ??= new DownloadFrm();
        }

        /// <summary>
        ///     打开窗口=>执行任务=>关闭窗口
        /// </summary>
        /// <param name="funcs">要执行的任务数组</param>
        public async Task DoWork(params Func<ValueTask>[] funcs)
        {
            DataContext = Downloader.DownloadInfoHandler;
            var currentTaskIndex = 0;
            Show();

            foreach (var func in funcs)
            {
                currentTaskIndex++;
                var index = currentTaskIndex;
                Downloader.DownloadInfoHandler.CurrentTaskName = index.ToString();
                await func();
            }

            Close();
        }

        private void DownloadFrm_OnClosed(object sender, EventArgs e)
        {
            _downloadFrm = null;
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}