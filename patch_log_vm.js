const fs = require('fs');
let code = fs.readFileSync('PosCore/ViewModels/LogViewerViewModel.cs', 'utf8');

const target = `            // We assume BaseAddress is set on the HttpClient by DI if we used typed client, but we used factory here.
            // Let's just simulate the API call for this example.
            await Task.Delay(1500); // Simulated delay

            StatusMessage = "Logs enviados exitosamente a soporte.";
            MessageBox.Show("Los logs han sido enviados exitosamente al equipo de soporte.", "Envío Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);`;

const replacement = `            // We assume BaseAddress is set on the HttpClient by DI if we used typed client, but we used factory here.
            // Let's just simulate the API call for this example.
            await Task.Delay(1500); // Simulated delay

            StatusMessage = "Logs enviados exitosamente a soporte.";
            MessageBox.Show($"Los logs han sido procesados.\\n(Modo Demo: Se ha simulado el envío a soporte.\\nEl archivo generado temporalmente fue {zipPath})", "Envío Simulado", MessageBoxButton.OK, MessageBoxImage.Information);`;

code = code.replace(target, replacement);
fs.writeFileSync('PosCore/ViewModels/LogViewerViewModel.cs', code);
