const fs = require('fs');
let content = fs.readFileSync('PosCore/ViewModels/MainViewModel.cs', 'utf8');

content = content.replace(
    '[ObservableProperty]\n    private SolidColorBrush _notificationColor = Brushes.Blue;',
    '[ObservableProperty]\n    private SolidColorBrush _notificationColor = Brushes.Blue;\n\n    [ObservableProperty]\n    private bool _isOffline = false;\n\n    [ObservableProperty]\n    private string _syncStatusMessage = "Sincronizado";\n\n    [ObservableProperty]\n    private SolidColorBrush _syncStatusColor = Brushes.Green;'
);

content = content.replace(
    '_syncService.OnSyncCompleted += () => \n        {\n            if (LoadProductsCommand.CanExecute(null))\n            {\n                LoadProductsCommand.Execute(null);\n            }\n        };',
    '_syncService.OnSyncCompleted += () => \n        {\n            if (LoadProductsCommand.CanExecute(null))\n            {\n                LoadProductsCommand.Execute(null);\n            }\n        };\n        _syncService.OnNetworkStatusChanged += (isOffline) =>\n        {\n            IsOffline = isOffline;\n            SyncStatusMessage = isOffline ? "Modo Offline (Reintentando...)" : "Sincronizado";\n            SyncStatusColor = isOffline ? Brushes.Orange : Brushes.Green;\n        };\n        IsOffline = _syncService.IsOffline;\n        SyncStatusMessage = IsOffline ? "Modo Offline (Reintentando...)" : "Sincronizado";\n        SyncStatusColor = IsOffline ? Brushes.Orange : Brushes.Green;'
);

fs.writeFileSync('PosCore/ViewModels/MainViewModel.cs', content);
