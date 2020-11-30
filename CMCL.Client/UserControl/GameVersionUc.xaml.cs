﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CMCL.Client.Download;
using CMCL.Client.Game;
using CMCL.Client.GameVersion;
using CMCL.Client.Util;
using CMCL.Client.Window;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using MessageBox = HandyControl.Controls.MessageBox;

namespace CMCL.Client.UserControl
{
    /// <summary>
    /// GameVersionUc.xaml 的交互逻辑
    /// </summary>
    public partial class GameVersionUc : System.Windows.Controls.UserControl
    {
        private GameVersionManifest _gameVersionManifest = null;
        private readonly IHttpClientFactory _httpClientFactory;

        public GameVersionUc(IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();
            _httpClientFactory = httpClientFactory;
        }

        private void GameVersionUc_OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private async ValueTask LoadGameVersionList()
        {
            BtnRefresh.IsEnabled = false;
            BtnDownload.IsEnabled = false;
            LoadingBlock.Visibility = Visibility.Visible;
            LoadingBlock.LoadingTip = "加载中...";

            _gameVersionManifest = await VersionDownloader.LoadGameVersionList(_httpClientFactory.CreateClient()).ConfigureAwait(true);
            var dataTable = new DataTable();
            dataTable.Columns.Add("版本");
            dataTable.Columns.Add("发布时间");
            dataTable.Columns.Add("类型");
            foreach (var gameVersionInfo in _gameVersionManifest.Versions)
            {
                var remark = string.Empty;
                if (gameVersionInfo.Id == _gameVersionManifest.Latest.Release)
                {
                    remark = "(最新稳定版)";
                }
                else if (gameVersionInfo.Id == _gameVersionManifest.Latest.Snapshot)
                {
                    remark = "(最新快照)";
                }

                var dr = dataTable.NewRow();
                dr["版本"] = gameVersionInfo.Id;
                dr["发布时间"] = DateTime.Parse(gameVersionInfo.ReleaseTime).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                dr["类型"] = $"{gameVersionInfo.Type}{remark}";
                dataTable.Rows.Add(dr);
                //从1.13开始支持，更早版本抛弃
                if (gameVersionInfo.Id == "1.13") break;
            }

            VersionListView.ItemsSource = dataTable.DefaultView;

            BtnRefresh.IsEnabled = true;
            BtnRefresh.Content = "刷新列表";
            BtnDownload.IsEnabled = true;
            LoadingBlock.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// 刷新列表按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadGameVersionList().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await LogHelper.WriteLogAsync(exception).ConfigureAwait(false);
                await LoadGameVersionList().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 下载按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDownload_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(VersionListView.SelectedItem is DataRowView selectVer))
            {
                NotifyIcon.ShowBalloonTip("提示", "请选择一个版本", NotifyIconInfoType.Warning, "AppNotifyIcon");
                return;
            }

            BtnDownload.IsEnabled = false;
            BtnRefresh.IsEnabled = false;

            var downloadInfoFrm = new DownloadInfoFrm()
            {
                Owner = App.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            //下载本体文件和json
            var thread = new Thread(async () =>
            {
                var progress = new Progress<double>();
                progress.ProgressChanged += (sender, value) =>
                {
                    Downloader.DownloadInfoHandler.CurrentTaskProgress = value;
                    if (Downloader.DownloadInfoHandler.TaskFinished)
                    {
                        this.Dispatcher.BeginInvoke(new Action(() => { downloadInfoFrm.Close(); }));
                    }
                };
                try
                {
                    await VersionDownloader.DownloadClient(_httpClientFactory.CreateClient(), progress, selectVer["版本"].ToString(), AppConfig.GetAppConfig().MinecraftDir).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    await LogHelper.WriteLogAsync(exception);
                }
            });
            thread.Start();

            if (downloadInfoFrm.ShowDialog() == true)
            {
                NotifyIcon.ShowBalloonTip("提示", "下载成功", NotifyIconInfoType.Info, "AppNotifyIcon");
                var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var fullPath = System.IO.Path.Combine(path, ".minecraft", "versions", selectVer["版本"].ToString());
                System.Diagnostics.Process.Start("Explorer.exe", fullPath);
            }

            BtnDownload.IsEnabled = true;
            BtnRefresh.IsEnabled = true;
        }
    }
}