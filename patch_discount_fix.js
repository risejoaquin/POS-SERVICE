const fs = require('fs');

let vm = fs.readFileSync('PosCore/ViewModels/MainViewModel.cs', 'utf8');

vm = vm.replace(
    '[ObservableProperty]\n    private decimal _discountAmount = 0m;\n\n    [ObservableProperty]\n    private decimal _subTotal = 0m;',
    '[ObservableProperty]\n    private decimal _discountAmount = 0m;\n\n    [ObservableProperty]\n    private bool _isDiscountApplied = false;\n\n    [ObservableProperty]\n    private decimal _subTotal = 0m;'
);

vm = vm.replace(
    'DiscountAmount = SubTotal * 0.10m;\n        }\n        else\n        {\n            DiscountAmount = 0;\n        }',
    'DiscountAmount = SubTotal * 0.10m;\n            IsDiscountApplied = true;\n        }\n        else\n        {\n            DiscountAmount = 0;\n            IsDiscountApplied = false;\n        }'
);

fs.writeFileSync('PosCore/ViewModels/MainViewModel.cs', vm);

let view = fs.readFileSync('PosCore/Views/MainWindow.xaml', 'utf8');
view = view.replace(
    'Visibility="{Binding DiscountAmount, Converter={StaticResource BooleanToVisibilityConverter}}"',
    'Visibility="{Binding IsDiscountApplied, Converter={StaticResource BooleanToVisibilityConverter}}"'
);
fs.writeFileSync('PosCore/Views/MainWindow.xaml', view);
