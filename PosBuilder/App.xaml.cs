using System;
using System.IO;
using System.Windows;

namespace PosBuilder
{
    public partial class App : Application
    {
        private string _logFilePath;

        public App()
        {
            _logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "posbuilder_error.log");
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogException("DispatcherUnhandledException", e.Exception);
            MessageBox.Show($"Error fatal de UI:\n{e.Exception.Message}\n\nRevisa el archivo {_logFilePath} en tu Escritorio para más detalles.", "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogException("CurrentDomain_UnhandledException", ex);
                MessageBox.Show($"Error fatal en la aplicación:\n{ex.Message}\n\nRevisa el archivo {_logFilePath} en tu Escritorio para más detalles.", "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TaskScheduler_UnobservedTaskException(object sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
        {
            LogException("TaskScheduler_UnobservedTaskException", e.Exception);
            e.SetObserved();
        }

        private void LogException(string source, Exception ex)
        {
            try
            {
                string errorMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{source}] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n";
                if (ex.InnerException != null)
                {
                    errorMessage += $"--- Inner Exception ---\n{ex.InnerException.GetType().Name}: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}\n";
                }
                errorMessage += new string('-', 80) + "\n";
                File.AppendAllText(_logFilePath, errorMessage);
            }
            catch
            {
                // No se puede hacer mucho si falla el registro
            }
        }
    }
}
