using System;
using System.Windows;
using System.Windows.Controls;
using CMCL.LauncherCore.Launch;
using CMCL.LauncherCore.Utilities;
using CMCL.Wpf.Window;
using HandyControl.Controls;
using HandyControl.Data;

namespace CMCL.Wpf.UserControl
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
            if (UcMainTab.Visibility != Visibility.Visible) return;
            var currentVersion = AppConfig.GetAppConfig().CurrentVersion;
            TbSelectedVersion.Text = $"Minecraft版本：{currentVersion}";
        }

        /// <summary>
        ///     开始游戏按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void StartGameBtnClick(object sender, RoutedEventArgs e)
        {
            var startBtn = (Button) sender;
            var loadingFrm = LoadingFrm.GetInstance(System.Windows.Window.GetWindow(this));

            var launcher = new Launcher();

            #region 各类事件注册

            launcher.OnCleanNativesDir += (_, status, _) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    loadingFrm.ShowDialogCustom();
                    loadingFrm.TbLodingTip.Text = status;
                }));
            };
            launcher.OnCheckLibrariesAndAssets += (_, status, _) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    loadingFrm.ShowDialogCustom();
                    loadingFrm.TbLodingTip.Text = status;
                }));
            };
            launcher.OnMojangLogin += (_, status) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    loadingFrm.ShowDialogCustom();
                    loadingFrm.TbLodingTip.Text = status;
                }));
            };
            launcher.OnUnzipNatives += (_, status, _) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    loadingFrm.ShowDialogCustom();
                    loadingFrm.TbLodingTip.Text = status;
                }));
            };
            launcher.BeforeGameLaunch += (s, _, _) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    startBtn.IsEnabled = false;
                    startBtn.Content = "正在启动";
                    loadingFrm.ShowDialog();
                }));
                if (s is Launcher l) Console.WriteLine($"启动器版本：{l.Version}");
            };
            launcher.OnGameStart += (_, v) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    startBtn.Content = "正在运行";
                    if (loadingFrm.IsVisible) loadingFrm.Close();
                }));
            };
            launcher.OnGameOutputReceived += (_, data) =>
            {
                if (!string.IsNullOrWhiteSpace(data)) Console.WriteLine($"游戏进程消息：{data}");
            };
            launcher.OnGameErrorReceived += (_, data) =>
            {
                if (!string.IsNullOrWhiteSpace(data)) Console.WriteLine($"游戏进程错误：{data}");
            };
            launcher.OnGameExit += (_, vInfo, exitCode) => { Console.WriteLine($"游戏进程已结束，退出码：{exitCode.ToString()}"); };

            launcher.OnLaunchError += async (l, exception) =>
            {
                await LogHelper.WriteLogAsync(LogLevel.Error, "启动错误", $"{exception.Message}。启动器内核版本：{l.Version}");
                NotifyIcon.ShowBalloonTip("启动错误", exception.Message, NotifyIconInfoType.Error, "AppNotifyIcon");
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    startBtn.IsEnabled = true;
                    if (loadingFrm.IsVisible) loadingFrm.Close();
                }));
            };

            #endregion

            try
            {
                //开始启动
                var isLaunched = await launcher.Start();

                Dispatcher.BeginInvoke(new Action(() => { startBtn.Content = "正在运行"; }));
                NotifyIcon.ShowBalloonTip("提示", "游戏已启动", NotifyIconInfoType.Info, "AppNotifyIcon");
            }
            catch (Exception exception)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    startBtn.IsEnabled = true;
                    if (loadingFrm.IsVisible) loadingFrm.Close();
                }));
                await LogHelper.LogExceptionAsync(exception);
                NotifyIcon.ShowBalloonTip("错误", exception.Message, NotifyIconInfoType.Error, "AppNotifyIcon");
            }
        }
    }
}