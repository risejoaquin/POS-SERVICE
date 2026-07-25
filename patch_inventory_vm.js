const fs = require('fs');
let code = fs.readFileSync('PosCore/ViewModels/InventoryViewModel.cs', 'utf8');

code = code.replace(
    `            MessageBox.Show($"Error al guardar producto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);`,
    `            MessageBox.Show($"Error al guardar producto: {ex.Message}\\nDetalle: {ex.InnerException?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);`
);

fs.writeFileSync('PosCore/ViewModels/InventoryViewModel.cs', code);
