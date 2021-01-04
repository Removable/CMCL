using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using CMCL.Core.Download;
using CMCL.Core.Util;

namespace CMCL.Wpf.Window
{
    public partial class LoadingFrm : System.Windows.Window
    {
        private static LoadingFrm _loadingFrm;

        private LoadingFrm()
        {
            InitializeComponent();
        }

        ~LoadingFrm()
        {
            _loadingFrm = null;
        }

        /// <summary>
        /// 单例：获取窗口
        /// </summary>
        /// <param name="dataContext">数据上下文</param>
        /// <param name="owner">Owner</param>
        /// <returns></returns>
        public static LoadingFrm GetInstance(object dataContext, System.Windows.Window owner = null)
        {
            var frm = _loadingFrm ??= new LoadingFrm();
            if (owner != null && owner != _loadingFrm.Owner) frm.Owner = owner;

            frm.DataContext = dataContext;
            return frm;
        }

        /// <summary>
        ///     打开窗口=>执行任务=>关闭窗口
        /// </summary>
        /// <param name="disappearType">任务执行结束后是如何处理窗口</param>
        /// <param name="funcs">要执行的任务数组</param>
        public async Task DoWork(WindowDisappear disappearType, params Func<ValueTask>[] funcs)
        {
            Show();

            var currentTaskIndex = 0;
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

        private void LoadingFrm_OnClosing(object sender, CancelEventArgs e)
        {
            _loadingFrm = null;
        }
    }
}