﻿using System.ComponentModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using CMCL.Client.UserControl;
using CMCL.Client.Util;

namespace CMCL.Client
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : HandyControl.Controls.Window, IComponentConnector
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MainWindow(IHttpClientFactory httpClientFactory)
        {
            InitializeComponent();
            _httpClientFactory = httpClientFactory;
            GlobalStaticResource.HttpClientFactory = _httpClientFactory;
        }

        /// <summary>
        ///     窗体载入后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var mainUc = new MainTabUc();
            MainTabItem.Content = mainUc;
        }

        private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl tabControl)
            {
                var selectedItem = (TabItem) tabControl.SelectedItem;
                if (selectedItem.Name == "VersionTabItem" && selectedItem.Content == null)
                {
                    var gameVersionUc = new GameVersionUc(_httpClientFactory);
                    selectedItem.Content = gameVersionUc;
                }
                else if (selectedItem.Name == "SettingsItem" && selectedItem.Content == null)
                {
                    var settingsUc = new SettingsUc();
                    selectedItem.Content = settingsUc;
                }
            }
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            // //清理缓存文件夹
            // var tempDir = GameHelper.GetCmclCacheDir(false);
            // if (System.IO.Directory.Exists(tempDir))
            //     System.IO.Directory.Delete(tempDir, true);
        }
    }
}