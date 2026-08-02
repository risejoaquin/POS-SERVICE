using System;
using System.Diagnostics;
using System.Windows;

namespace PosBuilder.Views
{
    public partial class SuccessModal : Window
    {
        private string _credentials;
        private string _outputFolder;

        public SuccessModal(string outputFolder, string credentials)
        {
            InitializeComponent();
            _credentials = credentials;
            _outputFolder = outputFolder;
            
            LocationText.Text = System.IO.Path.Combine(outputFolder, "Setup.exe");
            CredentialsText.Text = credentials;
        }

        private void CopyCredentials_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                Clipboard.SetText(_credentials);
                MessageBox.Show("Credenciales copiadas al portapapeles.", "Copiado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch { }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = _outputFolder,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir la carpeta: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
