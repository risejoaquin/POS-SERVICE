using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace PosBuilder.Views.Controls
{
    public partial class FileBrowserControl : UserControl
    {
        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register(
                "FilePath", 
                typeof(string), 
                typeof(FileBrowserControl), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnFilePathChanged));

        public string FilePath
        {
            get => (string)GetValue(FilePathProperty);
            set => SetValue(FilePathProperty, value);
        }

        public event EventHandler<string> FileSelected;

        public FileBrowserControl()
        {
            InitializeComponent();
        }

        private static void OnFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileBrowserControl control && e.NewValue is string path)
            {
                if (File.Exists(path))
                {
                    control.LoadPreview(path);
                }
            }
        }

        private void DropZone_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.gif;*.bmp|Todos los archivos|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                ProcessFileAsync(dialog.FileName);
            }
        }

        private void DropZone_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    ProcessFileAsync(files[0]);
                }
            }
        }

        private async void ProcessFileAsync(string path)
        {
            if (!File.Exists(path)) return;

            DefaultState.Visibility = Visibility.Collapsed;
            PreviewState.Visibility = Visibility.Collapsed;
            LoadingState.Visibility = Visibility.Visible;

            bool isValid = await Task.Run(() => ValidateImageMagicBytes(path));

            if (isValid)
            {
                long size = new FileInfo(path).Length;
                if (size > 500 * 1024)
                {
                    LoadingProgress.IsIndeterminate = false;
                    for (int i = 0; i <= 100; i += 10)
                    {
                        LoadingProgress.Value = i;
                        await Task.Delay(50); // Simulated delay for large files
                    }
                }

                FilePath = path;
                LoadPreview(path);
                FileSelected?.Invoke(this, path);
            }
            else
            {
                MessageBox.Show("El archivo seleccionado no es una imagen válida.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                DefaultState.Visibility = Visibility.Visible;
                LoadingState.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadPreview(string path)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(path);
                bitmap.DecodePixelWidth = 100; // Thumbnail
                bitmap.EndInit();

                Thumbnail.Source = bitmap;
                FileNameText.Text = Path.GetFileName(path);

                DefaultState.Visibility = Visibility.Collapsed;
                LoadingState.Visibility = Visibility.Collapsed;
                PreviewState.Visibility = Visibility.Visible;
            }
            catch
            {
                DefaultState.Visibility = Visibility.Visible;
                LoadingState.Visibility = Visibility.Collapsed;
                PreviewState.Visibility = Visibility.Collapsed;
            }
        }

        private bool ValidateImageMagicBytes(string path)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                if (fs.Length < 4) return false;

                byte[] buffer = new byte[4];
                fs.Read(buffer, 0, 4);

                // JPEG
                if (buffer[0] == 0xFF && buffer[1] == 0xD8) return true;
                // PNG
                if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47) return true;
                // GIF
                if (buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46) return true;
                // BMP
                if (buffer[0] == 0x42 && buffer[1] == 0x4D) return true;
                
                // Allow other types or strict check
            }
            catch { }
            return false;
        }
    }
}
