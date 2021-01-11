using System;
using System.Windows.Input;
using CMCL.LauncherCore.Utilities;

namespace CMCL.Wpf.Window
{
    public partial class DownloadFrm : System.Windows.Window
    {
        private static DownloadFrm _downloadFrm;

        private DownloadFrm()
        {
            InitializeComponent();
        }

        ~DownloadFrm()
        {
            _downloadFrm = null;
        }

        /// <summary>
        ///     获取单例实例
        /// </summary>
        /// <returns></returns>
        public static DownloadFrm GetInstance(System.Windows.Window owner = null)
        {
            var frm = _downloadFrm ??= new DownloadFrm();
            if (owner != null && owner != _downloadFrm.Owner) frm.Owner = owner;

            return frm;
        }

        private void DownloadFrm_OnClosed(object sender, EventArgs e)
        {
            _downloadFrm = null;
            GameHelper.GetDownloadCancellationToken().Cancel();
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}