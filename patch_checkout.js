const fs = require('fs');
let content = fs.readFileSync('PosCore/ViewModels/MainViewModel.cs', 'utf8');

content = content.replace(
    'var activeShift = await _dbContext.CashRegisterShifts.FirstOrDefaultAsync(s => !s.IsClosed);\n        if (activeShift == null)\n        {\n            _ = ShowNotification("No hay un turno abierto. Por favor, abra un turno.", true);\n            return;\n        }\n\n        try\n        {\n            // Validar stock antes de continuar',
    `var activeShift = await _dbContext.CashRegisterShifts.FirstOrDefaultAsync(s => !s.IsClosed);
        if (activeShift == null)
        {
            _ = ShowNotification("No hay un turno abierto. Por favor, abra un turno.", true);
            return;
        }

        // Mostrar Modal de Pago (Lealtad y Método)
        var paymentWindow = new PosCore.Views.PaymentWindow(Total);
        if (paymentWindow.ShowDialog() != true)
        {
            return;
        }

        try
        {
            // Validar stock antes de continuar`
);

fs.writeFileSync('PosCore/ViewModels/MainViewModel.cs', content);
