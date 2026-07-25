using System.Windows;
using PosCore.ViewModels;

namespace PosCore.Views
{
    public class InverseBoolToVisibilityConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool b) return b ? Visibility.Collapsed : Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }

    public partial class ShiftWindow : Window
    {
        public ShiftWindow(ShiftViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
