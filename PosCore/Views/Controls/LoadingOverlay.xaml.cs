using System.Windows;
using System.Windows.Controls;

namespace PosCore.Views.Controls
{
    public partial class LoadingOverlay : UserControl
    {
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(LoadingOverlay), new PropertyMetadata("Cargando...", OnMessageChanged));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public LoadingOverlay()
        {
            InitializeComponent();
        }

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LoadingOverlay control && e.NewValue is string message)
            {
                control.MessageText.Text = message;
            }
        }

        public void Show(string message = "Cargando...")
        {
            Message = message;
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
