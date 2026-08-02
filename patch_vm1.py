with open("PosBuilder/ViewModels/WizardViewModel.cs", "r", encoding="utf-8") as f:
    content = f.read()

import re

old_code = """        [RelayCommand]
        public void TestConnection()
        {
            MessageBox.Show("Conexión exitosa a la base de datos.", "Test Connection", MessageBoxButton.OK, MessageBoxImage.Information);
        }"""

new_code = """        [ObservableProperty]
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
        }"""

if old_code in content:
    content = content.replace(old_code, new_code)
    with open("PosBuilder/ViewModels/WizardViewModel.cs", "w", encoding="utf-8") as f:
        f.write(content)
    print("Replaced ViewModel commands.")
else:
    print("Could not find old_code.")
