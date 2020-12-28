using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CMCL.Client.Download.Mirrors;
using CMCL.Client.LoginPlugin;
using CMCL.Client.Util;
using CMCL.Client.Window;
using HandyControl.Controls;
using HandyControl.Data;

namespace CMCL.Client.UserControl
{
    /// <summary>
    ///     MainTabUc.xaml 的交互逻辑
    /// </summary>
    public partial class MainTabUc : System.Windows.Controls.UserControl
    {
        public MainTabUc()
        {
            InitializeComponent();
        }

        private void MainTabUc_OnLoaded(object sender, RoutedEventArgs e)
        {
            var currentVersion = AppConfig.GetAppConfig().CurrentVersion;
            TbSelectedVersion.Text = $"Minecraft版本：{currentVersion}";
        }

        private void MainTabUc_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (UcMainTab.Visibility == Visibility.Visible)
            {
                var currentVersion = AppConfig.GetAppConfig().CurrentVersion;
                TbSelectedVersion.Text = $"Minecraft版本：{currentVersion}";
            }
        }

        /// <summary>
        ///     开始游戏按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void StartGameBtnClick(object sender, RoutedEventArgs e)
        {
            var config = AppConfig.GetAppConfig();
            var baseDir = Path.Combine(config.MinecraftDir, ".minecraft");
            var btn = (Button) sender;
            var loadingFrm = LoadingFrm.GetInstance("", System.Windows.Window.GetWindow(this));

            try
            {
                #region 检查启动必要条件

                if (string.IsNullOrWhiteSpace(config.CurrentVersion))
                    throw new Exception("启动版本错误");

                //账号密码
                if (string.IsNullOrWhiteSpace(config.Account) || string.IsNullOrWhiteSpace(config.Password))
                {
                    NotifyIcon.ShowBalloonTip("提醒", "请填写用户名或密码", NotifyIconInfoType.Info, "AppNotifyIcon");
                    return;
                }

                //Java安装
                if (string.IsNullOrWhiteSpace(config.CustomJavaPath) || !File.Exists(config.CustomJavaPath))
                {
                    throw new Exception("Java未安装或未设置Java路径");
                }

                //清理Natives文件夹
                loadingFrm.Show("正在清理缓存");
                if (!await GameHelper.CleanNativesDir())
                {
                    throw new Exception("缓存清理失败");
                }

                var a = await MirrorManager.GetCurrentMirror().Library
                    .GetLibrariesDownloadList(config.CurrentVersion, true);

                loadingFrm.Show("正在校验文件");
                //校验各文件
                if (!File.Exists(Path.Combine(baseDir, "versions", $"{config.CurrentVersion}.json")) || //json文件
                    !File.Exists(Path.Combine(baseDir, "versions", $"{config.CurrentVersion}.jar")) || //jar文件
                    (await MirrorManager.GetCurrentMirror().Library
                        .GetLibrariesDownloadList(config.CurrentVersion, true).ConfigureAwait(false)).Any() || //库文件
                    (await MirrorManager.GetCurrentMirror().Asset.GetAssetsDownloadList(config.CurrentVersion, true)
                        .ConfigureAwait(false)).Any()) //资源文件
                {
                    throw new Exception("游戏文件缺失，请尝试重新下载");
                }

                #endregion

                //登录
                loadingFrm.Show("正在登录");
                btn.IsEnabled = false;
                var result = await MojangLogin.Login(config.Account, config.Password);
                if (result.IsSuccess)
                {
                    loadingFrm.Hide();
                }
            }
            catch (Exception exception)
            {
                await LogHelper.WriteLogAsync(exception);
                NotifyIcon.ShowBalloonTip("错误", exception.Message, NotifyIconInfoType.Error, "AppNotifyIcon");
            }
            finally
            {
                btn.IsEnabled = true;
                loadingFrm.Hide();
            }
        }
    }
}