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

namespace CMCL.Client.UserControl
{
    /// <summary>
    /// LoadingUc.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingUc : System.Windows.Controls.UserControl
    {
        public LoadingUc()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("LoadingTip", typeof(string),
            typeof(LoadingUc), new PropertyMetadata("Text", new PropertyChangedCallback(OnTextChanged)));

        public string LoadingTip
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        static void OnTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((LoadingUc)sender).OnValueChanged(args);
        }
        protected void OnValueChanged(DependencyPropertyChangedEventArgs e)
        {
            this.LoadingText.Text = e.NewValue.ToString();
        }
    }
}
