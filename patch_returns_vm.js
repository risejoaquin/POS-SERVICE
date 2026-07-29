const fs = require('fs');
let content = fs.readFileSync('PosCore/ViewModels/ReturnsViewModel.cs', 'utf8');

content = content.replace(
    'var result = MessageBox.Show($"¿Está seguro que desea devolver la orden {order.Id} por {order.TotalAmount:C}?\\nEsto sumará los productos al inventario.", "Confirmar Devolución", MessageBoxButton.YesNo, MessageBoxImage.Question);\n                \n        if (result == MessageBoxResult.Yes)\n        {\n            try',
    `var result = MessageBox.Show($"¿Está seguro que desea devolver la orden {order.Id} por {order.TotalAmount:C}?\\nEsto sumará los productos al inventario.", "Confirmar Devolución", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
        if (result == MessageBoxResult.Yes)
        {
            // 1. Pedir Autorización de Gerente
            var overrideWindow = new PosCore.Views.ManagerOverrideWindow($"Devolución de Orden #{order.Id} por {order.TotalAmount:C}", _dbContext);
            if (overrideWindow.ShowDialog() != true)
            {
                return;
            }

            // 2. Pedir Motivo de la Anulación
            var reasonWindow = new PosCore.Views.ReasonWindow();
            if (reasonWindow.ShowDialog() != true)
            {
                return;
            }

            try`
);

content = content.replace(
    'order.IsReturned = true;\n                order.LastUpdated = DateTime.Now;',
    'order.IsReturned = true;\n                order.ReturnReason = reasonWindow.SelectedReason;\n                order.AuthorizedBy = overrideWindow.AuthorizedBy;\n                order.LastUpdated = DateTime.Now;'
);

fs.writeFileSync('PosCore/ViewModels/ReturnsViewModel.cs', content);
