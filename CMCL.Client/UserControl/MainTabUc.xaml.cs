using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
            var btn = (Button) sender;
            var loadingFrm = LoadingFrm.GetInstance("", System.Windows.Window.GetWindow(this));
            
            try
            {
                #region 检查启动必要条件

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
                if (!await GameHelper.CleanNativesDir())
                {
                    throw new Exception("缓存清理失败");
                }

                #endregion
                
                //登录
                loadingFrm.Show();
                loadingFrm.LoadingControl.LoadingTip = "正在登录";
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