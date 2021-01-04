using System;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using CMCL.Core.Download;
using CMCL.Core.Download.Mirrors;
using CMCL.Core.GameVersion;
using CMCL.Core.Util;
using CMCL.Wpf.Window;
using HandyControl.Controls;
using HandyControl.Data;

namespace CMCL.Wpf.UserControl
{
    /// <summary>
    ///     GameVersionUc.xaml 的交互逻辑
    /// </summary>
    public partial class GameVersionUc : System.Windows.Controls.UserControl
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private GameVersionManifest _gameVersionManifest;

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
            var loadingFrm = LoadingFrm.GetInstance("加载中", Application.Current.MainWindow);
            loadingFrm.Dispatcher.BeginInvoke(new Action(() =>
            {
                loadingFrm.ShowDialog();
            }));

            var mirror = MirrorManager.GetCurrentMirror();

            _gameVersionManifest =
                await mirror.Version.LoadGameVersionList(GlobalStaticResource.HttpClientFactory.CreateClient());

            // _gameVersionManifest = await VersionDownloader.LoadGameVersionList(_httpClientFactory.CreateClient())
            //     .ConfigureAwait(true);
            var dataTable = new DataTable();
            dataTable.Columns.Add("版本");
            dataTable.Columns.Add("发布时间");
            dataTable.Columns.Add("类型");
            dataTable.Columns.Add("url");
            foreach (var gameVersionInfo in _gameVersionManifest.Versions)
            {
                var remark = string.Empty;
                if (gameVersionInfo.Id == _gameVersionManifest.Latest.Release)
                    remark = "(最新稳定版)";
                else if (gameVersionInfo.Id == _gameVersionManifest.Latest.Snapshot) remark = "(最新快照)";

                var dr = dataTable.NewRow();
                dr["版本"] = gameVersionInfo.Id;
                dr["发布时间"] = DateTime.Parse(gameVersionInfo.ReleaseTime).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                dr["类型"] = $"{gameVersionInfo.Type}{remark}";
                dr["url"] = gameVersionInfo.Url;
                dataTable.Rows.Add(dr);
                //从1.13正式版开始支持，更早版本抛弃
                if (gameVersionInfo.Id == "1.13") break;
            }

            VersionListView.ItemsSource = dataTable.DefaultView;

            BtnRefresh.IsEnabled = true;
            BtnRefresh.Content = "刷新列表";
            BtnDownload.IsEnabled = true;
            loadingFrm.Dispatcher.BeginInvoke(new Action(() =>
            {
                loadingFrm.Close();
            }));
        }

        /// <summary>
        ///     刷新列表按钮
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
                NotifyIcon.ShowBalloonTip("错误", "加载版本列表错误", NotifyIconInfoType.Error, "AppNotifyIcon");
            }
        }

        /// <summary>
        ///     下载按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void BtnDownload_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(VersionListView.SelectedItem is DataRowView selectVer))
            {
                NotifyIcon.ShowBalloonTip("提示", "请选择一个版本", NotifyIconInfoType.Warning, "AppNotifyIcon");
                return;
            }

            BtnDownload.IsEnabled = false;
            BtnRefresh.IsEnabled = false;
            Downloader.DownloadInfoHandler.TaskFinished = false;

            try
            {
                var mirror = MirrorManager.GetCurrentMirror();
                var versionId = selectVer["版本"].ToString();

                #region 下载json和jar

                var downloadFrm = DownloadFrm.GetInstance(Application.Current.MainWindow);
                downloadFrm.Dispatcher.BeginInvoke(new Action(() =>
                {
                    downloadFrm.DataContext = Downloader.DownloadInfoHandler;
                    downloadFrm.ShowDialog();
                }));
                try
                {
                    await mirror.Version.DownloadJsonAsync(versionId);
                    await mirror.Version.DownloadJarAsync(versionId);
                }
                finally
                {
                    downloadFrm.Dispatcher.BeginInvoke(new Action(() => { downloadFrm.Close(); }));
                }

                #endregion

                var loadingFrm = LoadingFrm.GetInstance(GlobalStaticResource.LoadingFrmDataContext, Application.Current.MainWindow);

                #region 下载库文件

                loadingFrm.Dispatcher.BeginInvoke(new Action(() =>
                {
                    GlobalStaticResource.LoadingFrmDataContext.CurrentLoadingTip = "校验库文件";
                    loadingFrm.ShowDialog();
                }));
                try
                {
                    var librariesDownloadList = await mirror.Library.GetLibrariesDownloadList(versionId, true);
                    if (librariesDownloadList.Count <= 0)
                    {
                        loadingFrm.Dispatcher.BeginInvoke(new Action(() => { loadingFrm.Hide(); }));
                    }
                    else
                    {
                        GlobalStaticResource.LoadingFrmDataContext.CurrentLoadingTip =
                            $"下载库(0/{librariesDownloadList.Count.ToString()})";
                        await mirror.Library.DownloadLibrariesAsync(librariesDownloadList);
                    }
                }
                finally
                {
                    loadingFrm.Dispatcher.BeginInvoke(new Action(() => { loadingFrm.Hide(); }));
                }

                #endregion

                #region 下载资源文件
                
                loadingFrm.Dispatcher.BeginInvoke(new Action(() =>
                {
                    GlobalStaticResource.LoadingFrmDataContext.CurrentLoadingTip = "校验资源文件";
                    loadingFrm.ShowDialog();
                }));
                try
                {
                    var assetsDownloadList = await mirror.Asset.GetAssetsDownloadList(versionId, true);
                    if (assetsDownloadList.Count <= 0)
                    {
                        loadingFrm.Dispatcher.BeginInvoke(new Action(() => { loadingFrm.Hide(); }));
                    }
                    else
                    {
                        GlobalStaticResource.LoadingFrmDataContext.CurrentLoadingTip =
                            $"下载资源(0/{assetsDownloadList.Count.ToString()})";
                        await mirror.Asset.DownloadAssets(assetsDownloadList);
                    }
                }
                finally
                {
                    loadingFrm.Dispatcher.BeginInvoke(new Action(() => { loadingFrm.Close(); }));
                }

                #endregion
                
                NotifyIcon.ShowBalloonTip("提示", "下载完成", NotifyIconInfoType.Info, "AppNotifyIcon");
            }
            catch (Exception exception)
            {
                await LogHelper.WriteLogAsync(exception);
                NotifyIcon.ShowBalloonTip("错误", "下载失败", NotifyIconInfoType.Error, "AppNotifyIcon");
            }
            finally
            {
                BtnDownload.IsEnabled = true;
                BtnRefresh.IsEnabled = true;
                Downloader.DownloadInfoHandler.TaskFinished = true;
            }
        }
    }
}