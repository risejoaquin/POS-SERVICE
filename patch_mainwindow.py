import re
with open("PosBuilder/MainWindow.xaml.cs", "r", encoding="utf-8") as f:
    content = f.read()

old_code = """            MainOverlay.Show("Generando instalador...");
            
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

            // Simulate some loading time
            await Task.Delay(1500);

            bool ok1 = await generator.WriteWithIntegrityValidationAsync(appSettingsPath, generator.GenerateAppSettings(config));
            bool ok2 = await generator.WriteWithIntegrityValidationAsync(envPath, generator.GenerateEnvFile(config));
            bool ok3 = await generator.WriteWithIntegrityValidationAsync(sqlPath, generator.GenerateSqlScript(config));

            MainOverlay.Hide();

            if (ok1 && ok2 && ok3)
            {
                MainOverlay.Show("Compilando cliente POS (PosCore)...");"""

new_code = """            MainOverlay.Show("Validando configuración...");
            
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
                MainOverlay.Show("Compilando binarios de cliente POS (PosCore). Esto puede tomar unos segundos...");"""

if old_code in content:
    content = content.replace(old_code, new_code)
    with open("PosBuilder/MainWindow.xaml.cs", "w", encoding="utf-8") as f:
        f.write(content)
    print("Replaced Generation progress messages")
else:
    print("Could not find generation progress code")
