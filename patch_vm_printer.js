const fs = require('fs');
let content = fs.readFileSync('PosCore/ViewModels/MainViewModel.cs', 'utf8');

// Add Hardware error properties
content = content.replace(
    '[ObservableProperty]\n    private SolidColorBrush _syncStatusColor = Brushes.Green;',
    '[ObservableProperty]\n    private SolidColorBrush _syncStatusColor = Brushes.Green;\n\n    [ObservableProperty]\n    private bool _isHardwareError = false;\n\n    [ObservableProperty]\n    private string _hardwareErrorMessage = string.Empty;'
);

// Update CheckoutAsync to check printer result
content = content.replace(
    '_ticketPrinterService.PrintTicket(order);\n                        Cart.Clear();\n            UpdateTotal();\n            _ = ShowNotification("Venta completada exitosamente.", false);',
    'bool printSuccess = _ticketPrinterService.PrintTicket(order);\n            if (!printSuccess)\n            {\n                IsHardwareError = true;\n                HardwareErrorMessage = "Error en la impresora. Revise el papel o conexión.";\n            }\n            else\n            {\n                IsHardwareError = false;\n            }\n\n            Cart.Clear();\n            UpdateTotal();\n            _ = ShowNotification("Venta completada exitosamente.", false);'
);

// Update TestPrinter to check printer result
content = content.replace(
    '_ticketPrinterService.TestPrinter();\n            _ = ShowNotification("Prueba de impresión enviada.", false);',
    'bool success = _ticketPrinterService.TestPrinter();\n            if (!success)\n            {\n                IsHardwareError = true;\n                HardwareErrorMessage = "Error de impresora detectado durante prueba.";\n                _ = ShowNotification("Fallo al imprimir", true);\n            }\n            else\n            {\n                IsHardwareError = false;\n                _ = ShowNotification("Prueba de impresión enviada.", false);\n            }'
);

fs.writeFileSync('PosCore/ViewModels/MainViewModel.cs', content);
