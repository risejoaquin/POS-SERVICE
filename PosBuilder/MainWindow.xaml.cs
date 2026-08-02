using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PosBuilder.ViewModels;
using PosBuilder.Views;
using PosBuilder.Services;
using System.Threading.Tasks;

namespace PosBuilder
{
    public partial class MainWindow : Window
    {
        private WizardViewModel _viewModel;
        
        private UserControl[] _steps;

        public ObservableCollection<StepIndicator> StepIndicators { get; set; } = new ObservableCollection<StepIndicator>();

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new WizardViewModel();
            DataContext = _viewModel;
            
            _steps = new UserControl[]
            {
                new Step1Environment { DataContext = _viewModel },
                new Step2Database { DataContext = _viewModel },
                new Step3Security { DataContext = _viewModel },
                new Step4Branding { DataContext = _viewModel },
                new Step5Users { DataContext = _viewModel },
                new Step6Modules { DataContext = _viewModel },
                new Step7Summary { DataContext = _viewModel }
            };

            var stepNames = new string[] 
            {
                "Entorno", "Base de Datos", "Seguridad", "Branding", "Usuarios", "Módulos", "Resumen"
            };

            for (int i = 0; i < stepNames.Length; i++)
            {
                StepIndicators.Add(new StepIndicator { Title = stepNames[i], Index = i });
            }
            StepList.ItemsSource = StepIndicators;

            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            UpdateStepView();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WizardViewModel.CurrentStepIndex))
            {
                UpdateStepView();
            }
        }

        private void UpdateStepView()
        {
            if (_viewModel.CurrentStepIndex >= 0 && _viewModel.CurrentStepIndex < _steps.Length)
            {
                StepContentControl.Content = _steps[_viewModel.CurrentStepIndex];
            }

            foreach (var item in StepIndicators)
            {
                if (item.Index < _viewModel.CurrentStepIndex)
                {
                    item.Icon = "✔";
                    item.Color = Brushes.Green;
                }
                else if (item.Index == _viewModel.CurrentStepIndex)
                {
                    item.Icon = "●";
                    item.Color = Brushes.Blue;
                }
                else
                {
                    item.Icon = "○";
                    item.Color = Brushes.Gray;
                }
            }
        }

        private async void Generate_Click(object sender, RoutedEventArgs e)
        {
            MainOverlay.Show("Validando configuración...");
            
            var config = new PosBuilder.Models.ConfigModel
            {
                ApiBaseUrl = _viewModel.ApiUrl,
                CompanyName = _viewModel.BrandingName,
                PrimaryColor = _viewModel.BrandingColor,
                LogoPath = _viewModel.BrandingLogoPath,
                TenantId = _viewModel.TenantName.Replace(" ", "").ToLower(),
                BusinessType = _viewModel.BusinessType,
                DbType = _viewModel.DbType,
                DbHost = _viewModel.DbHost,
                DbPort = _viewModel.DbPort,
                DbUser = _viewModel.DbUser,
                DbPassword = _viewModel.DbPassword,
                DbName = _viewModel.DbName,
                JwtSecret = _viewModel.JwtSecret,
                AdminUser = _viewModel.AdminUser,
                AdminPassword = _viewModel.AdminPassword,
                EmployeeUser = _viewModel.EmployeeUser,
                EmployeePassword = _viewModel.EmployeePassword,
                Environment = _viewModel.Environment
            };

            var generator = new ConfigurationGenerator();
            
            string outputDir = System.IO.Path.Combine(System.Environment.CurrentDirectory, "Output");
            
            string appSettingsPath = System.IO.Path.Combine(outputDir, "appsettings.json");
            string envPath = System.IO.Path.Combine(outputDir, "railway.env.example");
            string sqlPath = System.IO.Path.Combine(outputDir, "init.sql");

            await Task.Delay(500);
            
            MainOverlay.Show("Generando archivos de configuración (appsettings.json)...");
            bool ok1 = await generator.WriteWithIntegrityValidationAsync(appSettingsPath, generator.GenerateAppSettings(config));
            
            await Task.Delay(500);
            MainOverlay.Show("Generando script de base de datos SQL e Inyección de Dependencias...");
            bool ok2 = await generator.WriteWithIntegrityValidationAsync(envPath, generator.GenerateEnvFile(config));
            bool ok3 = await generator.WriteWithIntegrityValidationAsync(sqlPath, generator.GenerateSqlScript(config));

            if (ok1 && ok2 && ok3)
            {
                MainOverlay.Show("Compilando binarios de cliente POS (PosCore). Esto puede tomar unos segundos...");
                MainOverlay.ShowLog();
                try 
                {
                    // Copy appsettings.json to PosCore before compiling
                    string corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "..", "..", "..", "PosCore"));
                    if (!System.IO.Directory.Exists(corePath)) {
                        corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "PosCore")); // Fallback
                    }
                    if (!System.IO.Directory.Exists(corePath)) {
                        corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "PosCore")); // Fallback 2
                    }

                    // Setup PosServer AppSettings automatically
                    string serverPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "..", "..", "..", "PosServer"));
                    if (!System.IO.Directory.Exists(serverPath)) {
                        serverPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "PosServer")); // Fallback
                    }
                    if (!System.IO.Directory.Exists(serverPath)) {
                        serverPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "PosServer")); // Fallback 2
                    }
                    if (System.IO.Directory.Exists(serverPath)) 
                    {
                        string serverAppSettingsJson = generator.GenerateServerAppSettings(config);
                        System.IO.File.WriteAllText(System.IO.Path.Combine(serverPath, "appsettings.Development.json"), serverAppSettingsJson);
                        System.IO.File.WriteAllText(System.IO.Path.Combine(serverPath, "appsettings.json"), serverAppSettingsJson);
                    }
                    
                    string logFilePath = System.IO.Path.Combine(outputDir, "build.log");
                    
                    if (System.IO.Directory.Exists(corePath)) 
                    {
                        System.IO.File.Copy(appSettingsPath, System.IO.Path.Combine(corePath, "appsettings.json"), true);
                        
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "dotnet",
                            Arguments = $"publish \"{corePath}\" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o \"{System.IO.Path.Combine(outputDir, "PosClient")}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        };
                        using var process = new System.Diagnostics.Process { StartInfo = psi };
                        var fullOutput = new System.Text.StringBuilder();
                        var fullError = new System.Text.StringBuilder();

                        process.OutputDataReceived += (s, ev) => {
                            if (ev.Data != null) {
                                MainOverlay.AppendLog(ev.Data);
                                fullOutput.AppendLine(ev.Data);
                            }
                        };
                        process.ErrorDataReceived += (s, ev) => {
                            if (ev.Data != null) {
                                MainOverlay.AppendLog("ERROR: " + ev.Data);
                                fullError.AppendLine(ev.Data);
                            }
                        };

                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        
                        await process.WaitForExitAsync();
                        
                        await System.IO.File.WriteAllTextAsync(logFilePath, $"=== Salida Estándar ===\n{fullOutput.ToString()}\n=== Salida de Error ===\n{fullError.ToString()}");
                        
                        if (process.ExitCode != 0)
                        {
                            throw new Exception($"El proceso de compilación falló con código {process.ExitCode}. Revisa build.log para más detalles.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    NotificationService.Instance.ShowError("Error al compilar PosCore: " + ex.Message);
                }
                MainOverlay.Hide();

                NotificationService.Instance.ShowSuccess("Archivos generados exitosamente.");
                string logPath = System.IO.Path.Combine(outputDir, "validation.log");
                await System.IO.File.WriteAllTextAsync(logPath, $"Configuración validada exitosamente: {DateTime.Now}");
                string creds = $"Administrador: {config.AdminUser} / {config.AdminPassword}\nEmpleado: {config.EmployeeUser} / {config.EmployeePassword}\n\nEl cliente compilado está en la carpeta Output/PosClient";

                
                var modal = new SuccessModal(outputDir, creds);
                modal.Owner = this;
                modal.ShowDialog();
                
                try
                {
                    string corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "..", "..", "..", "PosCore"));
                    if (!System.IO.Directory.Exists(corePath)) { corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "PosCore")); }
                    if (!System.IO.Directory.Exists(corePath)) { corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "PosCore")); }

                    string serverPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "..", "..", "..", "PosServer"));
                    if (!System.IO.Directory.Exists(serverPath)) { serverPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "PosServer")); }
                    if (!System.IO.Directory.Exists(serverPath)) { serverPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "PosServer")); }

                    if (System.IO.Directory.Exists(serverPath))
                    {
                        var serverProcess = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "dotnet",
                            Arguments = $"run --project \"{serverPath}\"",
                            UseShellExecute = true
                        };
                        System.Diagnostics.Process.Start(serverProcess);
                    }
                    
                    string clientExe = System.IO.Path.Combine(outputDir, "PosClient", "PosCore.exe");
                    if (System.IO.File.Exists(clientExe))
                    {
                        var clientProcess = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = clientExe,
                            UseShellExecute = true,
                            WorkingDirectory = System.IO.Path.Combine(outputDir, "PosClient")
                        };
                        System.Diagnostics.Process.Start(clientProcess);
                    }
                    else if (System.IO.Directory.Exists(corePath))
                    {
                         var clientFallbackProcess = new System.Diagnostics.ProcessStartInfo
                         {
                             FileName = "dotnet",
                             Arguments = $"run --project \"{corePath}\"",
                             UseShellExecute = true
                         };
                         System.Diagnostics.Process.Start(clientFallbackProcess);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error al iniciar las aplicaciones: " + ex.Message, "Ejecución Automática", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
                
                Close();
            }
            else
            {
                NotificationService.Instance.ShowError("Error de integridad al generar los archivos.");
            }
        }


    }

    public class StepIndicator : System.ComponentModel.INotifyPropertyChanged
    {
        public int Index { get; set; }
        
        private string _title;
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        private string _icon;
        public string Icon
        {
            get => _icon;
            set { _icon = value; OnPropertyChanged(nameof(Icon)); }
        }

        private Brush _color;
        public Brush Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(nameof(Color)); }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
}
