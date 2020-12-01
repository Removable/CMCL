using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
        /// 获取单例实例
        /// </summary>
        /// <returns></returns>
        public static DownloadFrm GetInstance()
        {
            return _downloadFrm ??= new DownloadFrm();
        }

        /// <summary>
        /// 打开窗口=>执行任务=>关闭窗口
        /// </summary>
        /// <param name="tasks">要执行的任务数组</param>
        public void DoWork(params Task[] tasks)
        {
            var currentTaskIndex = 0;
            this.Show();

            foreach (var task in tasks)
            {
                task.Start();
                task.ContinueWith(result =>
                {
                    currentTaskIndex++;
                    var index = currentTaskIndex;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // LoadingControl.LoadingTip = loadingText.Replace("$CurrentTaskIndex", currentTaskIndex.ToString());
                        TbCurrentTaskName.Text = index.ToString();
                    }));
                    if (currentTaskIndex >= tasks.Length)
                    {
                        this.Dispatcher.BeginInvoke(new Action(this.Close));
                    }
                });
            }

            // var whenAll = Task.WhenAll(tasks);
            // whenAll.ContinueWith(result => { this.Dispatcher.BeginInvoke(new Action(this.Close)); });
        }

        /// <summary>
        /// 打开窗口=>执行任务=>关闭窗口
        /// </summary>
        /// <param name="funcs">要执行的任务数组</param>
        public async Task DoWork(params Func<ValueTask>[] funcs)
        {
            var currentTaskIndex = 0;
            this.Show();

            foreach (var func in funcs)
            {
                await func();
                // await Task.Run(() => { action(); });
                // action.ContinueWith(result =>
                // {
                //     currentTaskIndex++;
                //     var index = currentTaskIndex;
                //     this.Dispatcher.BeginInvoke(new Action(() =>
                //     {
                //         // LoadingControl.LoadingTip = loadingText.Replace("$CurrentTaskIndex", currentTaskIndex.ToString());
                //         TbCurrentTaskName.Text = index.ToString();
                //     }));
                //     if (currentTaskIndex >= actions.Length)
                //     {
                //         this.Dispatcher.BeginInvoke(new Action(this.Close));
                //     }
                // });
            }

            // var whenAll = Task.WhenAll(tasks);
            // whenAll.ContinueWith(result => { this.Dispatcher.BeginInvoke(new Action(this.Close)); });
            this.Dispatcher.BeginInvoke(new Action(this.Close));
        }

        private void DownloadFrm_OnClosed(object? sender, EventArgs e)
        {
            _downloadFrm = null;
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}