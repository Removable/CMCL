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

        private void BtnStopDownload_OnMouseEnter(object sender, MouseEventArgs e)
        {
            // BtnStopDownload.TextDecorations = TextDecorations.Underline;
        }

        private void BtnStopDownload_OnMouseLeave(object sender, MouseEventArgs e)
        {
            // BtnStopDownload.TextDecorations = null;
        }

        private void BtnStopDownload_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
