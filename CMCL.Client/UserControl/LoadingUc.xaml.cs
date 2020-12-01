using System.Windows;

namespace CMCL.Client.UserControl
{
    /// <summary>
    ///     LoadingUc.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingUc : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("LoadingTip",
            typeof(string),
            typeof(LoadingUc), new PropertyMetadata("Text", OnTextChanged));

        public LoadingUc()
        {
            InitializeComponent();
        }

        public string LoadingTip
        {
            get => (string) GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((LoadingUc) sender).OnValueChanged(args);
        }

        protected void OnValueChanged(DependencyPropertyChangedEventArgs e)
        {
            LoadingBlock.Text = e.NewValue.ToString();
        }
    }
}