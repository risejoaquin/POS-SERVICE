import re
with open("PosBuilder/MainWindow.xaml.cs", "r", encoding="utf-8") as f:
    content = f.read()

old_code = """            if (ok1 && ok2 && ok3)
            {
                MainOverlay.Show("Compilando binarios de cliente POS (PosCore). Esto puede tomar unos segundos...");
                try 
                {
                    // Copy appsettings.json to PosCore before compiling"""

new_code = """            if (ok1 && ok2 && ok3)
            {
                MainOverlay.Show("Compilando binarios de cliente POS (PosCore). Esto puede tomar unos segundos...");
                MainOverlay.ShowLog();
                try 
                {
                    // Copy appsettings.json to PosCore before compiling"""

old_process = """                        using var process = System.Diagnostics.Process.Start(psi);
                        if (process != null)
                        {
                            string output = await process.StandardOutput.ReadToEndAsync();
                            string error = await process.StandardError.ReadToEndAsync();
                            await process.WaitForExitAsync();
                            
                            await System.IO.File.WriteAllTextAsync(logFilePath, $"=== Salida Estándar ===\\n{output}\\n=== Salida de Error ===\\n{error}");
                            
                            if (process.ExitCode != 0)
                            {
                                throw new Exception($"El proceso de compilación falló con código {process.ExitCode}. Revisa build.log para más detalles.");
                            }
                        }"""

new_process = """                        using var process = new System.Diagnostics.Process { StartInfo = psi };
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
                        
                        await System.IO.File.WriteAllTextAsync(logFilePath, $"=== Salida Estándar ===\\n{fullOutput.ToString()}\\n=== Salida de Error ===\\n{fullError.ToString()}");
                        
                        if (process.ExitCode != 0)
                        {
                            throw new Exception($"El proceso de compilación falló con código {process.ExitCode}. Revisa build.log para más detalles.");
                        }"""

if old_code in content and old_process in content:
    content = content.replace(old_code, new_code)
    content = content.replace(old_process, new_process)
    with open("PosBuilder/MainWindow.xaml.cs", "w", encoding="utf-8") as f:
        f.write(content)
    print("Replaced MainWindow logging logic")
else:
    print("Could not find old_code or old_process")
