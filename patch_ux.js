const fs = require('fs');

// Patch SyncService.cs
let syncService = fs.readFileSync('PosCore/Services/SyncService.cs', 'utf8');

syncService = syncService.replace(
    'public event Action? OnSyncCompleted;',
    'public event Action? OnSyncCompleted;\n    public event Action<bool>? OnNetworkStatusChanged;\n    private bool _isOffline = false;\n    public bool IsOffline\n    {\n        get => _isOffline;\n        private set\n        {\n            if (_isOffline != value)\n            {\n                _isOffline = value;\n                System.Windows.Application.Current.Dispatcher.Invoke(() => OnNetworkStatusChanged?.Invoke(_isOffline));\n            }\n        }\n    }'
);

syncService = syncService.replace(
    'catch (Exception ex)\n        {\n            _logger.LogError(ex, "Error crítico durante el proceso de sincronización.");\n        }',
    'catch (Exception ex)\n        {\n            _logger.LogError(ex, "Error crítico durante el proceso de sincronización.");\n            IsOffline = true;\n        }'
);

syncService = syncService.replace(
    '// Notificar a la UI si hubo cambios o simplemente al terminar un ciclo de sync exitoso\n            // Para evitar re-renders excesivos, idealmente solo lo llamamos si hubo pendingMessages o cloudProducts, pero por simplicidad lo llamamos siempre que termine sin error',
    'IsOffline = false;\n            // Notificar a la UI si hubo cambios o simplemente al terminar un ciclo de sync exitoso\n            // Para evitar re-renders excesivos, idealmente solo lo llamamos si hubo pendingMessages o cloudProducts, pero por simplicidad lo llamamos siempre que termine sin error'
);

syncService = syncService.replace(
    'catch (Exception ex)\n        {\n            _logger.LogError(ex, "Error al traer actualizaciones del servidor.");\n        }',
    'catch (Exception ex)\n        {\n            _logger.LogError(ex, "Error al traer actualizaciones del servidor.");\n            IsOffline = true;\n            throw;\n        }'
);

fs.writeFileSync('PosCore/Services/SyncService.cs', syncService);
