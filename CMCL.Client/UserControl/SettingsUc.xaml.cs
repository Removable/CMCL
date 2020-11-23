using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Microsoft.Win32;

namespace CMCL.Client.UserControl
{
    /// <summary>
    /// SettingsUc.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsUc : System.Windows.Controls.UserControl
    {
        public SettingsUc()
        {
            InitializeComponent();
        }

        private void SettingsUc_OnLoaded(object sender, RoutedEventArgs e)
        {
            TbJavaPath.Text = FileHelper.GetJavaDir();
        }

        /// <summary>
        /// 手动选择Java路径
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
            if (dialog.ShowDialog() == true)
            {
                TbJavaPath.Text = dialog.FileName;
            }
        }
    }
}
