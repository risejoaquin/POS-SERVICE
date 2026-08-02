using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PosCore.ViewModels;

namespace PosCore.Views.Controls
{
    public partial class ProductsPanelControl : UserControl
    {
        public ProductsPanelControl()
        {
            InitializeComponent();
        }

        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.ProcessBarcode();
                }
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.SelectAll();
            }
        }
    }
}
