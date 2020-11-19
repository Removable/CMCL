using System.Windows;
using System.Windows.Controls;

namespace CMCL.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : HandyControl.Controls.Window, System.Windows.Markup.IComponentConnector
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 窗体载入后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            //await Downloader.GetFile("http://zn.mcxiaoying.com:9201/download/1d87e7d75a99172f0cffc4f96cdc44da?name=client.jar", @"d:\", "a.jar");

            //var progress = new Progress<double>();
            //progress.ProgressChanged += (sender, value) => VersionTabItem.Header = ("\r%{0:N0}", value);
            //var cancellationToken = new CancellationTokenSource();
            //await Downloader.GetFileAsync("https://bmclapi2.bangbang93.com/version/1.15.2/client", progress, cancellationToken.Token, @"D:\", "b.jar");
        }

        private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
