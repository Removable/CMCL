using System;
using System.Windows;
using System.Windows.Controls;
using CMCL.Client.LoginPlugin;
using CMCL.Client.Util;
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
            TbSelectedVersion.Text = string.Format(TbSelectedVersion.Text, currentVersion);
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
            try
            {
                btn.IsEnabled = false;
                var result = await MojangLogin.Login(config.Account, config.Password);
                if (result.IsSuccess)
                {
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
            }
        }
    }
}