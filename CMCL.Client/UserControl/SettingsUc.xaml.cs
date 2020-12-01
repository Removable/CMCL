using System;
using System.Windows;
using CMCL.Client.Download;
using CMCL.Client.Util;
using ComponentUtil.Common.Data;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;

namespace CMCL.Client.UserControl
{
    /// <summary>
    ///     SettingsUc.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsUc : System.Windows.Controls.UserControl
    {
        public SettingsUc()
        {
            InitializeComponent();
        }

        private void SettingsUc_OnLoaded(object sender, RoutedEventArgs e)
        {
            ComboSelectedDownloadSource.ItemsSource = EnumHelper.GetAllDescriptions<DownloadSource>();
            InitSettingsControls();
        }

        /// <summary>
        ///     读取配置并显示到界面
        /// </summary>
        private void InitSettingsControls()
        {
            var appConfig = AppConfig.GetAppConfig();
            TbAccount.Text = appConfig.Account;
            TbPassword.Password = appConfig.Password;
            LoadDownloadVersion(appConfig.CurrentVersion);
            TbJavaPath.Text = appConfig.CustomJavaPath;
            TbMinecraftDir.Text = appConfig.MinecraftDir;
            ComboSelectedDownloadSource.Text = appConfig.DownloadSource;
            if (appConfig.UseDefaultGameDir)
                CbUseDefaultGameDir.IsChecked = true;
            else
                CbUseCustomGameDir.IsChecked = true;
        }

        /// <summary>
        ///     载入已有版本到下拉框
        /// </summary>
        private void LoadDownloadVersion(string selectedVersion = "")
        {
            var versions = GameHelper.GetDownloadedVersions();
            ComboSelectedVersion.ItemsSource = versions;
            //选中
            if (!string.IsNullOrWhiteSpace(selectedVersion))
            {
                var index = 0;
                foreach (var v in versions)
                {
                    if (v == selectedVersion) break;

                    index++;
                }

                ComboSelectedVersion.SelectedIndex = index;
            }
        }

        /// <summary>
        ///     手动选择Java路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChooseJavaPath(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                RestoreDirectory = true,
                Filter = @"Javaw.exe|Javaw.exe",
                Multiselect = false,
                CheckFileExists = true
            };
            if (dialog.ShowDialog() == true) TbJavaPath.Text = dialog.FileName;
        }

        /// <summary>
        ///     保存配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveConfig(object sender, RoutedEventArgs e)
        {
            try
            {
                var newConfig = new CmclConfig
                {
                    Account = TbAccount.Text,
                    Password = TbPassword.Password,
                    CurrentVersion = ComboSelectedVersion.Text,
                    CustomJavaPath = TbJavaPath.Text,
                    MinecraftDir = TbMinecraftDir.Text,
                    UseDefaultGameDir = CbUseDefaultGameDir.IsChecked ?? false,
                    DownloadSource = ComboSelectedDownloadSource.Text
                };

                await AppConfig.SaveAppConfig(newConfig);
                NotifyIcon.ShowBalloonTip("提示", "保存成功", NotifyIconInfoType.Info, "AppNotifyIcon");
            }
            catch (Exception exception)
            {
                await LogHelper.WriteLogAsync(exception);
                NotifyIcon.ShowBalloonTip("错误", "保存失败", NotifyIconInfoType.Error, "AppNotifyIcon");
            }
        }

        /// <summary>
        ///     重载已保存的配置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReloadConfig(object sender, RoutedEventArgs e)
        {
            InitSettingsControls();
        }

        #region 自定义.Minecraft文件夹

        /// <summary>
        ///     选择自定义文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChooseCustomGameDir(object sender, RoutedEventArgs e)
        {
            var ookiiDialog = new VistaFolderBrowserDialog();
            if (ookiiDialog.ShowDialog() == true) TbMinecraftDir.Text = ookiiDialog.SelectedPath;
        }

        /// <summary>
        ///     使用默认文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void UseDefaultGameDir(object sender, RoutedEventArgs e)
        {
            TbMinecraftDir.Text = GameHelper.GetDefaultMinecraftDir();
            BtnChooseMcDir.IsEnabled = false;
        }

        private void UseCustomGameDir(object sender, RoutedEventArgs e)
        {
            TbMinecraftDir.Text = AppConfig.GetAppConfig().MinecraftDir;
            BtnChooseMcDir.IsEnabled = true;
        }

        #endregion
    }
}