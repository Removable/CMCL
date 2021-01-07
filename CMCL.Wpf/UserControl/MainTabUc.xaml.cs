using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CMCL.Core.Download.Mirrors;
using CMCL.Core.LaunchGame;
using CMCL.Core.LoginPlugin;
using CMCL.Core.Util;
using CMCL.Wpf.Window;
using HandyControl.Controls;
using HandyControl.Data;
using Newtonsoft.Json;

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
            var startResult = false;
            var startBtn = (Button) sender;
            var loadingFrm = LoadingFrm.GetInstance(System.Windows.Window.GetWindow(this));

            var launcher = new Launcher();
            launcher.BeforeGameLaunch += (s, _, _) =>
            {
                Dispatcher.BeginInvoke(new Action(() => { startBtn.IsEnabled = false; }));
                Dispatcher.BeginInvoke(new Action(() => { startBtn.Content = "正在启动"; }));
                Dispatcher.BeginInvoke(new Action(() => { loadingFrm.ShowDialog(); }));
                if (s is Launcher l)
                {
                    Console.WriteLine($"启动器版本：{l.Version}");
                }
            };
            launcher.OnGameOutputReceived += (_, data) =>
            {
                if (!string.IsNullOrWhiteSpace(data))
                {
                    Console.WriteLine($"游戏进程消息：{data}");
                }
            };
            launcher.OnGameErrorReceived += (_, data) =>
            {
                if (!string.IsNullOrWhiteSpace(data))
                {
                    Console.WriteLine($"游戏进程错误：{data}");
                }
            };
            launcher.OnGameExit += (_, vInfo, exitCode) => { Console.WriteLine($"游戏进程已结束，退出码：{exitCode.ToString()}"); };

            launcher.OnLaunchError += async (l, exception) =>
            {
                await LogHelper.WriteLogAsync(LogLevel.Error, "启动错误", $"{exception.Message}。启动器内核版本：{l.Version}");
                NotifyIcon.ShowBalloonTip("启动错误", exception.Message, NotifyIconInfoType.Error, "AppNotifyIcon");
            };
            try
            {
                //开始启动
                var isLaunched = await launcher.Start();

                // if (!isLaunched)
                //     throw new Exception(msg);

                #region 废弃

                // #region 检查启动必要条件
                //
                // if (GameHelper.GetVersionInfo(config.CurrentVersion) == null)
                //     throw new Exception("选择的版本不存在，请重新下载");
                //
                // if (string.IsNullOrWhiteSpace(config.CurrentVersion))
                //     throw new Exception("未选择启动版本");
                //
                // //账号密码
                // if (string.IsNullOrWhiteSpace(config.Account) || string.IsNullOrWhiteSpace(config.Password))
                // {
                //     NotifyIcon.ShowBalloonTip("提醒", "请填写用户名或密码", NotifyIconInfoType.Info, "AppNotifyIcon");
                //     return;
                // }
                //
                // //Java安装
                // if (string.IsNullOrWhiteSpace(config.CustomJavaPath) || !File.Exists(config.CustomJavaPath))
                //     throw new Exception("Java未安装或未设置Java路径");
                //
                // //清理Natives文件夹
                // loadingFrm.Dispatcher.BeginInvoke(new Action(() =>
                // {
                //     GlobalStaticResource.LoadingFrmDataContext.CurrentLoadingTip = "正在清理缓存";
                //     loadingFrm.Show();
                // }));
                // if (!await GameHelper.CleanNativesDir()) throw new Exception("缓存清理失败");
                //
                // loadingFrm.Dispatcher.BeginInvoke(new Action(() =>
                // {
                //     GlobalStaticResource.LoadingFrmDataContext.CurrentLoadingTip = "正在校验文件";
                //     loadingFrm.Show();
                // }));
                // var mirror = MirrorManager.GetCurrentMirror();
                // //校验各文件
                // if (!File.Exists(Path.Combine(baseDir, "versions", config.CurrentVersion,
                //         $"{config.CurrentVersion}.json")) || //json文件
                //     !File.Exists(Path.Combine(baseDir, "versions", config.CurrentVersion,
                //         $"{config.CurrentVersion}.jar")) || //jar文件
                //     (await mirror.Library
                //         .GetLibrariesDownloadList(config.CurrentVersion, true).ConfigureAwait(false)).Any() || //库文件
                //     (await mirror.Asset.GetAssetsDownloadList(config.CurrentVersion, true)
                //         .ConfigureAwait(false)).Any()) //资源文件
                //     throw new Exception("游戏文件缺失，请尝试重新下载");
                //
                // //解压natives文件
                // await mirror.Library.UnzipNatives(config.CurrentVersion);
                //
                // #endregion
                //
                // //登录
                // loadingFrm.Dispatcher.BeginInvoke(new Action(() =>
                // {
                //     GlobalStaticResource.LoadingFrmDataContext.CurrentLoadingTip = "正在登录";
                //     loadingFrm.Show();
                // }));
                // var loginResult = await MojangLogin.Login(config.Account, config.Password);
                // Console.WriteLine(JsonConvert.SerializeObject(loginResult));
                //
                // //拼接启动参数
                // var versionInfo = GameHelper.GetVersionInfo(config.CurrentVersion);
                // var argument = await mirror.Version.GetStartArgument(versionInfo, loginResult);
                // var process = new Process
                // {
                //     StartInfo = new ProcessStartInfo(config.CustomJavaPath, argument)
                //     {
                //         RedirectStandardError = true,
                //         RedirectStandardOutput = true,
                //         RedirectStandardInput = true,
                //         CreateNoWindow = true,
                //         UseShellExecute = false,
                //         WorkingDirectory = Path.Combine(config.MinecraftDir, ".minecraft")
                //     },
                //     EnableRaisingEvents = true
                // };
                //
                // process.Exited += (sender, args) =>
                // {
                //     Dispatcher.BeginInvoke(new Action(() => { btn.IsEnabled = true; }));
                //     Dispatcher.BeginInvoke(new Action(() => { btn.Content = "开始游戏"; }));
                // };
                // process.OutputDataReceived += (sender, args) =>
                // {
                //     // Console.WriteLine(JsonConvert.SerializeObject(sender));
                //     // Console.WriteLine(JsonConvert.SerializeObject(args));
                // };
                // process.ErrorDataReceived += (sender, args) =>
                // {
                //     // Console.WriteLine(JsonConvert.SerializeObject(sender));
                //     // Console.WriteLine(JsonConvert.SerializeObject(args));
                // };
                // startResult = process.Start();
                //
                // process.BeginOutputReadLine();
                // process.BeginErrorReadLine();

                #endregion

                Dispatcher.BeginInvoke(new Action(() => { startBtn.Content = "正在运行"; }));
                NotifyIcon.ShowBalloonTip("提示", "游戏已启动", NotifyIconInfoType.Info, "AppNotifyIcon");
            }
            catch (Exception exception)
            {
                await LogHelper.LogExceptionAsync(exception);
                NotifyIcon.ShowBalloonTip("错误", exception.Message, NotifyIconInfoType.Error, "AppNotifyIcon");
            }
            finally
            {
                if (!startResult) Dispatcher.BeginInvoke(new Action(() => { startBtn.IsEnabled = true; }));
                loadingFrm.Dispatcher.BeginInvoke(new Action(() => { loadingFrm.Hide(); }));
            }
        }
    }
}