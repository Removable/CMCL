using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CMCL.Client.Util;

namespace CMCL.Client.UserControl
{
    /// <summary>
    /// MainTabUc.xaml 的交互逻辑
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
        /// 开始游戏按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartGameBtnClick(object sender, RoutedEventArgs e)
        {

        }
    }
}