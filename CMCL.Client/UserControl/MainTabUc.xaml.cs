using System;
using System.Diagnostics;
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
using Newtonsoft.Json;

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
            Dispatcher.BeginInvoke(new Action(() => { btn.IsEnabled = false; }));
            var loadingFrm = LoadingFrm.GetInstance("", System.Windows.Window.GetWindow(this));

            try
            {
                #region 检查启动必要条件

                if (GameHelper.GetVersionInfo(config.CurrentVersion) == null)
                    throw new Exception("选择的版本不存在，请重新下载");

                if (string.IsNullOrWhiteSpace(config.CurrentVersion))
                    throw new Exception("未选择启动版本");

                //账号密码
                if (string.IsNullOrWhiteSpace(config.Account) || string.IsNullOrWhiteSpace(config.Password))
                {
                    NotifyIcon.ShowBalloonTip("提醒", "请填写用户名或密码", NotifyIconInfoType.Info, "AppNotifyIcon");
                    return;
                }

                //Java安装
                if (string.IsNullOrWhiteSpace(config.CustomJavaPath) || !File.Exists(config.CustomJavaPath))
                    throw new Exception("Java未安装或未设置Java路径");

                //清理Natives文件夹
                loadingFrm.Dispatcher.BeginInvoke(new Action(() => { loadingFrm.Show("正在清理缓存"); }));
                if (!await GameHelper.CleanNativesDir()) throw new Exception("缓存清理失败");

                loadingFrm.Dispatcher.BeginInvoke(new Action(() => { loadingFrm.Show("正在校验文件"); }));
                var mirror = MirrorManager.GetCurrentMirror();
                //校验各文件
                if (!File.Exists(Path.Combine(baseDir, "versions", config.CurrentVersion,
                        $"{config.CurrentVersion}.json")) || //json文件
                    !File.Exists(Path.Combine(baseDir, "versions", config.CurrentVersion,
                        $"{config.CurrentVersion}.jar")) || //jar文件
                    (await mirror.Library
                        .GetLibrariesDownloadList(config.CurrentVersion, true).ConfigureAwait(false)).Any() || //库文件
                    (await mirror.Asset.GetAssetsDownloadList(config.CurrentVersion, true)
                        .ConfigureAwait(false)).Any()) //资源文件
                    throw new Exception("游戏文件缺失，请尝试重新下载");

                //解压natives文件
                await mirror.Library.UnzipNatives(config.CurrentVersion);

                #endregion

                //登录
                loadingFrm.Dispatcher.BeginInvoke(new Action(() => { loadingFrm.Show("正在登录"); }));
                var loginResult = await MojangLogin.Login(config.Account, config.Password);
                Console.WriteLine(JsonConvert.SerializeObject(loginResult));

                //拼接启动参数
                var versionInfo = GameHelper.GetVersionInfo(config.CurrentVersion);
                var argument = await mirror.Version.GetStartArgument(versionInfo, loginResult);
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo(config.CustomJavaPath, argument)
                    {
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WorkingDirectory = Path.Combine(config.MinecraftDir, ".minecraft")
                    },
                    EnableRaisingEvents = true
                };

                process.Exited += (sender, args) =>
                {
                    Console.WriteLine(JsonConvert.SerializeObject(sender));
                    Console.WriteLine(JsonConvert.SerializeObject(args));
                };
                process.OutputDataReceived += (sender, args) =>
                {
                    Console.WriteLine(JsonConvert.SerializeObject(sender));
                    Console.WriteLine(JsonConvert.SerializeObject(args));
                };
                process.ErrorDataReceived += (sender, args) =>
                {
                    Console.WriteLine(JsonConvert.SerializeObject(sender));
                    Console.WriteLine(JsonConvert.SerializeObject(args));
                };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception exception)
            {
                await LogHelper.WriteLogAsync(exception);
                NotifyIcon.ShowBalloonTip("错误", exception.Message, NotifyIconInfoType.Error, "AppNotifyIcon");
            }
            finally
            {
                Dispatcher.BeginInvoke(new Action(() => { btn.IsEnabled = true; }));
                loadingFrm.Dispatcher.BeginInvoke(new Action(() => { loadingFrm.Hide(); }));
            }
        }
    }
}