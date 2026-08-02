using System;
using System.IO;
using System.Windows;

namespace PosBuilder
{
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Error fatal de UI:\n{e.Exception.Message}\n\n{e.Exception.StackTrace}", "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"Error fatal en la aplicación:\n{ex.Message}\n\n{ex.StackTrace}", "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
