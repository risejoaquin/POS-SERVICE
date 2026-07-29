const fs = require('fs');

let vm = fs.readFileSync('PosCore/ViewModels/MainViewModel.cs', 'utf8');

vm = vm.replace(
    'private void DecreaseQuantity(OrderItem item)\n    {',
    'private void ModifyItem(OrderItem item)\n    {\n        if (item != null)\n        {\n            var modifierWindow = new PosCore.Views.ItemModifierWindow(item);\n            if (modifierWindow.ShowDialog() == true)\n            {\n                // Force UI update for the cart\n                var index = Cart.IndexOf(item);\n                if (index >= 0) {\n                    Cart[index] = null;\n                    Cart[index] = item;\n                }\n                UpdateTotal();\n            }\n        }\n    }\n\n    [RelayCommand]\n    private void DecreaseQuantity(OrderItem item)\n    {'
);

vm = vm.replace(
    '[RelayCommand]\n    private void DecreaseQuantity(OrderItem item)\n    {',
    '[RelayCommand]\n    private void ModifyItem(OrderItem item)\n    {\n        if (item != null)\n        {\n            var modifierWindow = new PosCore.Views.ItemModifierWindow(item);\n            if (modifierWindow.ShowDialog() == true)\n            {\n                // Force UI update for the cart by replacing the item to trigger property changed\n                var index = Cart.IndexOf(item);\n                if (index >= 0) {\n                    Cart.RemoveAt(index);\n                    Cart.Insert(index, item);\n                }\n                UpdateTotal();\n            }\n        }\n    }\n\n    [RelayCommand]\n    private void DecreaseQuantity(OrderItem item)\n    {'
);


fs.writeFileSync('PosCore/ViewModels/MainViewModel.cs', vm);
