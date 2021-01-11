using System.ComponentModel;
using System.Windows;

namespace CMCL.Wpf.Window
{
    public partial class LoadingFrm : System.Windows.Window
    {
        private static LoadingFrm _loadingFrm;

        private LoadingFrm()
        {
            InitializeComponent();
        }

        ~LoadingFrm()
        {
            _loadingFrm = null;
        }

        /// <summary>
        /// 单例：获取窗口
        /// </summary>
        /// <param name="dataContext">数据上下文</param>
        /// <param name="owner">Owner</param>
        /// <returns></returns>
        public static LoadingFrm GetInstance(System.Windows.Window owner = null)
        {
            var frm = _loadingFrm ??= new LoadingFrm();
            if (owner != null && owner != _loadingFrm.Owner) frm.Owner = owner;
            return frm;
        }

        public void ShowDialogCustom()
        {
            if (Visibility != Visibility.Visible)
                ShowDialog();
        }

        private void LoadingFrm_OnClosing(object sender, CancelEventArgs e)
        {
            _loadingFrm = null;
        }
    }
}