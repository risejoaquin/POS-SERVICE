const fs = require('fs');

let vm = fs.readFileSync('PosCore/ViewModels/MainViewModel.cs', 'utf8');

vm = vm.replace(
    '    private void ModifyItem(OrderItem item)',
    '    [RelayCommand]\n    private void ModifyItem(OrderItem item)'
);

fs.writeFileSync('PosCore/ViewModels/MainViewModel.cs', vm);
