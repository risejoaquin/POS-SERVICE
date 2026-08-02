with open("PosBuilder/MainWindow.xaml.cs", "r", encoding="utf-8") as f:
    content = f.read()

import re

old_code = """            if (ok1 && ok2 && ok3)
            {
                NotificationService.Instance.ShowSuccess("Archivos generados exitosamente.");
                string logPath = System.IO.Path.Combine(outputDir, "validation.log");
                await System.IO.File.WriteAllTextAsync(logPath, $"Configuración validada exitosamente: {DateTime.Now}");
                string creds = $"Administrador: {config.AdminUser} / {config.AdminPassword}\\nEmpleado: {config.EmployeeUser} / {config.EmployeePassword}";
                
                var modal = new SuccessModal(outputDir, creds);"""

new_code = """            if (ok1 && ok2 && ok3)
            {
                MainOverlay.Show("Compilando cliente POS (PosCore)...");
                try 
                {
                    // Copy appsettings.json to PosCore before compiling
                    string corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "..", "..", "..", "PosCore"));
                    if (!System.IO.Directory.Exists(corePath)) {
                        corePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.Environment.CurrentDirectory, "..", "PosCore")); // Fallback
                    }
                    if (System.IO.Directory.Exists(corePath)) 
                    {
                        System.IO.File.Copy(appSettingsPath, System.IO.Path.Combine(corePath, "appsettings.json"), true);
                        
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "dotnet",
                            Arguments = $"publish \"{corePath}\" -c Release -o \"{System.IO.Path.Combine(outputDir, "PosClient")}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        };
                        using var process = System.Diagnostics.Process.Start(psi);
                        await process.WaitForExitAsync();
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
                string creds = $"Administrador: {config.AdminUser} / {config.AdminPassword}\\nEmpleado: {config.EmployeeUser} / {config.EmployeePassword}\\n\\nEl cliente compilado está en la carpeta Output/PosClient";
                
                var modal = new SuccessModal(outputDir, creds);"""

content = content.replace(old_code, new_code)
with open("PosBuilder/MainWindow.xaml.cs", "w", encoding="utf-8") as f:
    f.write(content)
print("Replaced!")
