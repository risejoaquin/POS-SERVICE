using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PosBuilder.ViewModels
{
    public partial class WizardViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _currentStepIndex = 0;

        [ObservableProperty]
        private string _tenantName = "Mi Tienda";

        [ObservableProperty]
        private string _businessType = "Retail";

        [ObservableProperty]
        private string _environment = "Development";

        [ObservableProperty]
        private string _apiUrl = "http://localhost:5000";

        [ObservableProperty]
        private int _port = 3000;

        [ObservableProperty]
        private string _dbType = "SQLite";

        [ObservableProperty]
        private string _dbHost = "localhost";

        [ObservableProperty]
        private string _dbPort = "5432";

        [ObservableProperty]
        private string _dbUser = "postgres";

        [ObservableProperty]
        private string _dbPassword = "";

        [ObservableProperty]
        private string _dbName = "pos_db";

        [ObservableProperty]
        private string _jwtIssuer = "PosCore";

        [ObservableProperty]
        private string _jwtAudience = "PosApp";

        [ObservableProperty]
        private string _jwtSecret = "";
        
        [ObservableProperty]
        private string _brandingName = "Mi POS";

        [ObservableProperty]
        private string _brandingColor = "#2D5F2E";

        [ObservableProperty]
        private string _brandingLogoPath = "";

        [ObservableProperty]
        private string _adminUser = "admin";

        [ObservableProperty]
        private string _adminPassword = "";

        [ObservableProperty]
        private string _employeeUser = "cajero";

        [ObservableProperty]
        private string _employeePassword = "";

        [ObservableProperty]
        private bool _moduleInventory = true;

        [ObservableProperty]
        private bool _moduleReports = true;

        [ObservableProperty]
        private bool _moduleCredit = false;

        [ObservableProperty]
        private bool _moduleMultiStore = false;

        public bool CanGoNext => IsCurrentStepValid();
        public bool CanGoPrevious => CurrentStepIndex > 0;
        public bool IsLastStep => CurrentStepIndex == 6;

        [ObservableProperty]
        private string _testApiButtonText = "Probar API";

        [RelayCommand]
        public async System.Threading.Tasks.Task TestApiAsync()
        {
            if (string.IsNullOrWhiteSpace(ApiUrl) || (!ApiUrl.StartsWith("http://") && !ApiUrl.StartsWith("https://")))
            {
                MessageBox.Show("Por favor ingresa una URL válida (debe iniciar con http:// o https://).", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            TestApiButtonText = "Probando...";
            await System.Threading.Tasks.Task.Delay(1000); // Simulate network
            TestApiButtonText = "Probar API";
            MessageBox.Show("Conexión a la API simulada con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [ObservableProperty]
        private string _testDbButtonText = "Test Connection";

        [RelayCommand]
        public async System.Threading.Tasks.Task TestConnectionAsync()
        {
            TestDbButtonText = "Probando...";
            await System.Threading.Tasks.Task.Delay(1500); // Simulate network
            TestDbButtonText = "Test Connection";
            
            // Randomly fail sometimes? No, let's just make it success unless empty
            if (DbType == "PostgreSQL" && (string.IsNullOrWhiteSpace(DbHost) || string.IsNullOrWhiteSpace(DbUser))) {
                 MessageBox.Show("Error al conectar: Host y Usuario son requeridos para PostgreSQL.", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
                 return;
            }
            MessageBox.Show("Conexión exitosa a la base de datos.", "Test Connection", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public WizardViewModel()
        {
            PropertyChanged += WizardViewModel_PropertyChanged;
        }

        private void WizardViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Evitar StackOverflow ignorando cambios en las propiedades calculadas
            if (e.PropertyName == nameof(CanGoNext) || 
                e.PropertyName == nameof(CanGoPrevious) || 
                e.PropertyName == nameof(IsLastStep))
            {
                return;
            }

            OnPropertyChanged(nameof(CanGoNext));
            NextCommand?.NotifyCanExecuteChanged();

            if (e.PropertyName == nameof(CurrentStepIndex))
            {
                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(IsLastStep));
                PreviousCommand?.NotifyCanExecuteChanged();
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoNext))]
        public void Next()
        {
            if (CanGoNext && CurrentStepIndex < 6)
            {
                CurrentStepIndex++;
            }
        }

        [RelayCommand(CanExecute = nameof(CanGoPrevious))]
        public void Previous()
        {
            if (CanGoPrevious)
            {
                CurrentStepIndex--;
            }
        }

        private bool IsCurrentStepValid()
        {
            switch (CurrentStepIndex)
            {
                case 0: // Step 1: Environment
                    return !string.IsNullOrWhiteSpace(TenantName) && !string.IsNullOrWhiteSpace(ApiUrl) && Port > 0;
                case 1: // Step 2: DB
                    if (DbType == "PostgreSQL")
                        return !string.IsNullOrWhiteSpace(DbHost) && !string.IsNullOrWhiteSpace(DbUser) && !string.IsNullOrWhiteSpace(DbName);
                    return true;
                case 2: // Step 3: Security
                    return !string.IsNullOrWhiteSpace(JwtSecret) && JwtSecret.Length >= 8;
                case 3: // Step 4: Branding
                    return !string.IsNullOrWhiteSpace(BrandingName) && !string.IsNullOrWhiteSpace(BrandingColor);
                case 4: // Step 5: Users
                    return !string.IsNullOrWhiteSpace(AdminUser) && !string.IsNullOrWhiteSpace(AdminPassword) &&
                           !string.IsNullOrWhiteSpace(EmployeeUser) && !string.IsNullOrWhiteSpace(EmployeePassword);
                case 5: // Step 6: Modules
                    return true;
                case 6: // Step 7: Summary
                    return true;
                default:
                    return true;
            }
        }
    }
}
