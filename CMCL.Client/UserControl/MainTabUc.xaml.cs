using System;
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
                #region 检查启动必要条件

                await GameHelper.CleanNativesDir();

                //账号密码
                if (string.IsNullOrWhiteSpace(config.Account) || string.IsNullOrWhiteSpace(config.Password))
                {
                    NotifyIcon.ShowBalloonTip("提醒", "请填写用户名或密码", NotifyIconInfoType.Info, "AppNotifyIcon");
                    return;
                }

                #endregion
                
                //登录
                var loadingFrm = LoadingFrm.GetInstance("正在登录...", System.Windows.Window.GetWindow(this));
                loadingFrm.Show();
                btn.IsEnabled = false;
                var result = await MojangLogin.Login(config.Account, config.Password);
                if (result.IsSuccess)
                {
                    loadingFrm.Close();
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