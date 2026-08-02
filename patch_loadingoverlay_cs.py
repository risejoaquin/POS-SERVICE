import re
with open("PosBuilder/Views/Controls/LoadingOverlay.xaml.cs", "r", encoding="utf-8") as f:
    content = f.read()

new_methods = """
        public void ShowLog()
        {
            LogContainer.Visibility = Visibility.Visible;
            LogTextBox.Text = "";
        }

        public void AppendLog(string text)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText(text + Environment.NewLine);
                LogScroller.ScrollToEnd();
            });
        }
        
        private void CopyLogs_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LogTextBox.Text))
            {
                Clipboard.SetText(LogTextBox.Text);
                MessageBox.Show("Logs copiados al portapapeles.", "Copiar", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
"""

old_class = """    public partial class LoadingOverlay : UserControl
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
    }"""

new_class = """    public partial class LoadingOverlay : UserControl
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
            LogContainer.Visibility = Visibility.Collapsed;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
            LogTextBox.Text = "";
        }
""" + new_methods + """    }"""

content = content.replace(old_class, new_class)
with open("PosBuilder/Views/Controls/LoadingOverlay.xaml.cs", "w", encoding="utf-8") as f:
    f.write(content)
print("Replaced LoadingOverlay cs")
