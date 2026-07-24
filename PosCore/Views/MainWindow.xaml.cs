using System.Windows;
using PosCore.ViewModels;

namespace PosCore.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        
        // Asignamos el ViewModel al DataContext para que los Bindings de XAML funcionen
        DataContext = viewModel;
    }
}
