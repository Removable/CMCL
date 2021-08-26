using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using CMCL.LauncherCore.Download;
using CMCL.LauncherCore.GameEntities;
using CMCL.LauncherCore.GameEntities.JsonClasses;
using CMCL.LauncherCore.Utilities;
using CMCL.Wpf.Window;
using ComponentUtil.Common.Data;
using HandyControl.Controls;
using HandyControl.Data;
using Newtonsoft.Json;

namespace CMCL.Wpf.UserControl
{
    /// <summary>
    ///     GameVersionUc.xaml 的交互逻辑
    /// </summary>
    public partial class GameVersionUc : System.Windows.Controls.UserControl
    {
        private static VersionManifest _gameVersionManifest;
        private static ForgeVersion[] _forgePromos;

        public GameVersionUc()
        {
            InitializeComponent();
        }

        private void GameVersionUc_OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// 加载版本列表
        /// </summary>
        /// <param name="forceRefresh">强制刷新</param>
        /// <returns></returns>
        private async ValueTask LoadGameVersionList(bool forceRefresh)
        {
            var mirror = MirrorManager.GetCurrentMirror();

            if (forceRefresh)
            {
                _gameVersionManifest = await mirror.Version.LoadGameVersionList(Utils.HttpClientFactory.CreateClient());
                _forgePromos = await mirror.Forge.GetForgeVersionList(Utils.HttpClientFactory.CreateClient());
            }

            var dataTable = new DataTable();
            dataTable.Columns.Add("版本");
            dataTable.Columns.Add("发布时间");
            dataTable.Columns.Add("类型");
            dataTable.Columns.Add("Forge");
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
                dr["Forge"] = _forgePromos.Any(f => f.Build != null && f.Build.McVersion == gameVersionInfo.Id)
                    ? "支持"
                    : "-";
                dr["url"] = gameVersionInfo.Url;
                dataTable.Rows.Add(dr);
                //从1.13正式版开始支持，更早版本抛弃
                if (gameVersionInfo.Id == "1.13") break;
            }

            VersionListView.ItemsSource = dataTable.DefaultView;
        }

        /// <summary>
        ///     刷新列表按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            BtnRefresh.IsEnabled = false;
            BtnDownload.IsEnabled = false;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadingFrm.GetInstance(Application.Current.MainWindow).ShowDialog();
            }));
            try
            {
                await LoadGameVersionList(true).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                await LogHelper.LogExceptionAsync(exception).ConfigureAwait(false);
                NotifyIcon.ShowBalloonTip("错误", "加载版本列表错误", NotifyIconInfoType.Error, "AppNotifyIcon");
            }
            finally
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    LoadingFrm.GetInstance(Application.Current.MainWindow).Close();
                    BtnRefresh.IsEnabled = true;
                    BtnRefresh.Content = "刷新列表";
                }));
            }
        }

        /// <summary>
        /// 下载指定版本
        /// </summary>
        /// <param name="versionId">mc版本</param>
        /// <param name="forgeVersion">forge版本，不安装则留空</param>
        private async void DownloadSelectedVersion(string versionId, string forgeVersion = "")
        {
            try
            {
                BtnDownload.IsEnabled = false;
                BtnRefresh.IsEnabled = false;

                var mirror = MirrorManager.GetCurrentMirror();

                #region 原版游戏下载

                #region 下载json和jar

                var downloadFrm = DownloadFrm.GetInstance(Application.Current.MainWindow);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (!downloadFrm.IsVisible) downloadFrm.ShowDialog();
                }));
                try
                {
                    //注册事件
                    mirror.Version.BeforeDownloadStart += (taskName, _, _) =>
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            downloadFrm.DownloadProgressBar.Value = 0;
                            downloadFrm.TbCurrentTaskName.Text = "正在下载";
                            downloadFrm.TbCurrentTaskDetail.Text = taskName;
                        }));
                    };
                    mirror.Version.OnDownloadProgressChanged += (_, progress) =>
                    {
                        downloadFrm.DownloadProgressBar.Value = progress;
                    };

                    await mirror.Version.DownloadJsonAsync(versionId);
                    await mirror.Version.DownloadJarAsync(versionId);
                }
                finally
                {
                    Dispatcher.BeginInvoke(new Action(() => { downloadFrm.Close(); }));
                }

                #endregion

                var loadingFrm = LoadingFrm.GetInstance(Application.Current.MainWindow);

                #region 下载库文件

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    loadingFrm.TbLodingTip.Text = "校验库文件";
                    loadingFrm.ShowDialog();
                }));
                try
                {
                    var librariesDownloadList = await mirror.Library.GetLibrariesDownloadList(versionId, true);
                    if (librariesDownloadList.Count <= 0)
                    {
                        Dispatcher.BeginInvoke(new Action(() => { loadingFrm.Hide(); }));
                    }
                    else
                    {
                        loadingFrm.TbLodingTip.Text = $"下载库(0/{librariesDownloadList.Count.ToString()})";
                        mirror.Library.OnDownloadFinish += (msg, tCount, fCount) =>
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                loadingFrm.TbLodingTip.Text = $"{msg}({fCount.ToString()}/{tCount.ToString()})";
                            }));
                        };
                        await mirror.Library.DownloadLibrariesAsync(librariesDownloadList);
                    }
                }
                finally
                {
                    Dispatcher.BeginInvoke(new Action(() => { loadingFrm.Hide(); }));
                }

                #endregion

                #region 下载资源文件

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    loadingFrm.TbLodingTip.Text = "校验资源文件";
                    loadingFrm.ShowDialog();
                }));
                try
                {
                    var assetsDownloadList = await mirror.Asset.GetAssetsDownloadList(versionId, true);
                    if (assetsDownloadList.Count <= 0)
                    {
                        Dispatcher.BeginInvoke(new Action(() => { loadingFrm.Hide(); }));
                    }
                    else
                    {
                        mirror.Asset.OnDownloadFinish += (msg, tCount, fCount) =>
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                loadingFrm.TbLodingTip.Text = $"{msg}({fCount.ToString()}/{tCount.ToString()})";
                            }));
                        };
                        await mirror.Asset.DownloadAssets(assetsDownloadList);
                    }
                }
                finally
                {
                    Dispatcher.BeginInvoke(new Action(() => { loadingFrm.Close(); }));
                }

                #endregion

                #endregion

                #region Forge下载和安装

                if (!string.IsNullOrWhiteSpace(forgeVersion))
                {
                    var selectedForge =
                        _forgePromos.FirstOrDefault(f => f.Build != null && f.Build.Version == forgeVersion);

                    #region forge安装器下载

                    //注册事件
                    mirror.Forge.BeforeDownloadStart += (taskName, _, _) =>
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            downloadFrm = DownloadFrm.GetInstance(Application.Current.MainWindow);
                            downloadFrm.DownloadProgressBar.Value = 0;
                            downloadFrm.TbCurrentTaskName.Text = "正在下载";
                            downloadFrm.TbCurrentTaskDetail.Text = taskName;
                            if (!downloadFrm.IsVisible) downloadFrm.ShowDialog();
                        }));
                    };
                    mirror.Forge.OnDownloadProgressChanged += (_, progress) =>
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            downloadFrm.DownloadProgressBar.Value = progress;
                        }));
                    };
                    mirror.Forge.OnDownloadFinish += (_, _, _) =>
                    {
                        Dispatcher.BeginInvoke(new Action(() => { downloadFrm.Hide(); }));
                    };
                    var installerPath = await mirror.Forge.DownloadForgeInstaller(selectedForge);

                    #endregion

                    #region forge安装

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        loadingFrm = LoadingFrm.GetInstance(Application.Current.MainWindow);
                        loadingFrm.TbLodingTip.Text = $"下载Forge库";
                        loadingFrm.ShowDialog();
                    }));
                    var a = 0;
                    mirror.Forge.OnDownloadFinish += (msg, tCount, fCount) =>
                    {
                        if (fCount > a) a = fCount;
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            loadingFrm.TbLodingTip.Text = $"{msg}({fCount.ToString()}/{tCount.ToString()})";
                            if (fCount == tCount) loadingFrm.Close();
                        }));
                    };
                    await mirror.Forge.InstallForge(selectedForge, installerPath);

                    #endregion
                }

                #endregion

                NotifyIcon.ShowBalloonTip("提示", "下载完成", NotifyIconInfoType.Info, "AppNotifyIcon");
            }
            catch (Exception exception)
            {
                await LogHelper.LogExceptionAsync(exception);
                NotifyIcon.ShowBalloonTip("错误", $"下载失败:{exception.Message}", NotifyIconInfoType.Error, "AppNotifyIcon");
            }
            finally
            {
                BtnDownload.IsEnabled = true;
                BtnRefresh.IsEnabled = true;
            }
        }

        /// <summary>
        /// 选择了一个版本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void SelectVersion(object sender, SelectionChangedEventArgs e)
        {
            if (!(VersionListView.SelectedItem is DataRowView selectVer))
            {
                return;
            }

            var versionId = selectVer["版本"].ToString();

            SpDownloadTypes.Children.Clear();
            var originMenuItem = new MenuItem {Header = $"下载原版{versionId}"};
            originMenuItem.Click += (_, _) => { DownloadSelectedVersion(versionId); };
            SpDownloadTypes.Children.Add(originMenuItem);

            var forgeVersions = _forgePromos.Where(f => f.Build != null && f.Build.McVersion == versionId).ToArray();
            foreach (var fv in forgeVersions)
            {
                var forgeMenuItem = new MenuItem
                    {Header = $"下载并安装Forge-{fv.Build.Version}({fv.Name.Replace("recommended", "推荐版").Replace("latest", "最新版")})"};
                forgeMenuItem.Click += (_, _) => { DownloadSelectedVersion(versionId, fv.Build.Version); };

                SpDownloadTypes.Children.Add(forgeMenuItem);
            }

            BtnDownload.IsEnabled = true;
        }
    }
}