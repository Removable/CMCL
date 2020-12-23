using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using CMCL.Client.Download;
using CMCL.Client.Download.Mirrors;
using CMCL.Client.Game;
using CMCL.Client.Util;
using CMCL.Client.Window;
using HandyControl.Controls;
using HandyControl.Data;

namespace CMCL.Client.UserControl
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
            LoadingBlock.Visibility = Visibility.Visible;
            LoadingBlock.LoadingTip = "加载中...";

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

            var mirror = MirrorManager.GetCurrentMirror();

            var downloadFrm = DownloadFrm.GetInstance(System.Windows.Window.GetWindow(this));
            // downloadFrm.Owner = System.Windows.Window.GetWindow(this);
            var versionId = selectVer["版本"].ToString();
            //下载版本json和资源json
            await downloadFrm.DoWork(WindowDisappear.None,
                async () => { await mirror.Version.DownloadJsonAsync(versionId); },
                async () => { await mirror.Asset.GetAssetIndexJson(versionId); });

            // var funcList = new List<Func<ValueTask>>();
            //下载jar
            // funcList.Add(async () => { await mirror.Version.DownloadJarAsync(versionId); });
            await downloadFrm.DoWork(WindowDisappear.Hide, async () => { await mirror.Version.DownloadJarAsync(versionId); });
            // //下载库文件
            // funcList.AddRange(await mirror.Library.DownloadLibrariesAsync(versionId));

            #region 下载库文件

            var librariesDownloadList = await mirror.Library.GetLibrariesDownloadList(versionId, true);

            #endregion

            #region 下载资源文件

            LoadingBlock.Visibility = Visibility.Visible;
            LoadingBlock.LoadingTip = "检查资源文件...";
            var assetsToDownload = await mirror.Asset.GetAssetsDownloadList(versionId, true);
            LoadingBlock.Visibility = Visibility.Hidden;

            if (assetsToDownload.Count > 0)
            {
                await downloadFrm.DoWork(WindowDisappear.Close, mirror.Asset.DownloadAssets(assetsToDownload));
            }

            #endregion


            BtnDownload.IsEnabled = true;
            BtnRefresh.IsEnabled = true;
            Downloader.DownloadInfoHandler.TaskFinished = true;
        }
    }
}