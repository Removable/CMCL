using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CMCL.Client.Download;
using CMCL.Client.GameVersion;

namespace CMCL.Client.Window
{
    /// <summary>
    /// DownloadInfoFrm.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadInfoFrm : HandyControl.Controls.Window
    {
        public DownloadInfoFrm()
        {
            InitializeComponent();
            //数据绑定
            this.TbDownloadFile.DataContext = Downloader.DownloadInfoHandler;
            this.TbDownloadGroup.DataContext = Downloader.DownloadInfoHandler;
            this.DownloadProgressBar.DataContext = Downloader.DownloadInfoHandler;
        }

        private void DownloadInfoFrm_OnClosing(object sender, CancelEventArgs e)
        {
            Downloader.DownloadCancellationToken.Cancel();
            this.DialogResult = Downloader.DownloadInfoHandler.TaskFinished;
        }
        
        public void DoWork(params Action[] actions)
        {

            var currentTaskIndex = 1;
            LoadingControl.LoadingTip = loadingText.Replace("$CurrentTaskIndex", currentTaskIndex.ToString()); ;
            this.Show();
            var taskFactory = new TaskFactory();

            var taskArray = new Task[actions.Length];
            for (var i = 0; i < actions.Length; i++)
            {
                taskArray[i] = taskFactory.StartNew(actions[i]);
            }
            taskFactory.ContinueWhenAny(taskArray, result =>
            {
                currentTaskIndex++;
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    LoadingControl.LoadingTip = loadingText.Replace("$CurrentTaskIndex", currentTaskIndex.ToString());
                }));
            });
            taskFactory.ContinueWhenAll(taskArray, result => { this.Dispatcher.BeginInvoke(new Action(this.Close)); });
        }
    }
}
