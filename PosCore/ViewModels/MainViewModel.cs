using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PosCore.Data;
using PosCore.Models;
using PosCore.Services;

namespace PosCore.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly PosDbContext _dbContext;

    public static ObservableCollection<ObservableCollection<OrderItem>> SuspendedOrders { get; set; } = new();

    [ObservableProperty]
    private decimal _discountAmount = 0m;

    [ObservableProperty]
    private bool _isDiscountApplied = false;

    [ObservableProperty]
    private decimal _subTotal = 0m;
    private readonly IApiService _apiService;

    // Propiedades Observables
    [ObservableProperty]
    private ObservableCollection<OrderItem> _cart = new();

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private ObservableCollection<Product> _filteredProducts = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _notificationMessage = string.Empty;

    [ObservableProperty]
    private bool _isNotificationVisible = false;

    [ObservableProperty]
    private SolidColorBrush _notificationColor = Brushes.Green;

    private async Task ShowNotification(string message, bool isError = false)
    {
        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            NotificationMessage = message;
            NotificationColor = isError ? Brushes.Red : Brushes.Green;
            IsNotificationVisible = true;
        });

        await Task.Delay(3000);

        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            IsNotificationVisible = false;
        });
    }

    partial void OnSearchQueryChanged(string value)
    {
        ApplySearchFilter();
    }

    private void ApplySearchFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            FilteredProducts = new ObservableCollection<Product>(Products);
            return;
        }

        var query = SearchQuery.ToLower();
        var exactBarcodeMatch = Products.FirstOrDefault(p => p.Barcode == SearchQuery);
        
        // Comportamiento escáner: coincidencia exacta de código de barras
        if (exactBarcodeMatch != null)
        {
            AddToCart(exactBarcodeMatch);
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() => 
            {
                SearchQuery = string.Empty;
            });
            return;
        }

        var matches = Products.Where(p => 
            p.Name.ToLower().Contains(query) || 
            p.Barcode.Contains(query)
        ).ToList();

        FilteredProducts = new ObservableCollection<Product>(matches);
    }

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private AppSettings _settings;

    [ObservableProperty]
    private SolidColorBrush _primaryColorBrush = Brushes.Blue;

    private readonly SyncService _syncService;
    private readonly TicketPrinterService _ticketPrinterService;

    public MainViewModel(PosDbContext dbContext, IApiService apiService, IOptions<AppSettings> settings, SyncService syncService, TicketPrinterService ticketPrinterService)
    {
        _dbContext = dbContext;
        _apiService = apiService;
        _settings = settings.Value;
        _syncService = syncService;
        _ticketPrinterService = ticketPrinterService;

        _syncService.OnSyncCompleted += () => 
        {
            if (LoadProductsCommand.CanExecute(null))
            {
                LoadProductsCommand.Execute(null);
            }
        };
        _syncService.OnNetworkStatusChanged += (isOffline) =>
        {
            IsOffline = isOffline;
            SyncStatusMessage = isOffline ? "Modo Offline (Reintentando...)" : "Sincronizado";
            SyncStatusColor = isOffline ? Brushes.Orange : Brushes.Green;
        };
        IsOffline = _syncService.IsOffline;
        SyncStatusMessage = IsOffline ? "Modo Offline (Reintentando...)" : "Sincronizado";
        SyncStatusColor = IsOffline ? Brushes.Orange : Brushes.Green;
        
        try {
            var color = (Color)ColorConverter.ConvertFromString(_settings.WhiteLabel.PrimaryColor);
            PrimaryColorBrush = new SolidColorBrush(color);
        } catch {
            // fallback if color is invalid
        }
        
        // Cargar productos iniciales
        LoadProductsCommand.Execute(null);
    }


    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        var localProducts = await _dbContext.Products.AsNoTracking().ToListAsync();
        
        if (!localProducts.Any())
        {
            // If empty, try to sync from API
            await _syncService.SyncDataAsync();
            localProducts = await _dbContext.Products.AsNoTracking().ToListAsync();
            
            // Seed default products if still empty
            if (!localProducts.Any())
            {
                var dummyProducts = new System.Collections.Generic.List<PosCore.Models.Product>
                {
                    new PosCore.Models.Product { Name = "Coca Cola 600ml", Price = 1.50m, Barcode = "7501055300075", StockQuantity = 100 },
                    new PosCore.Models.Product { Name = "Gansito Marinela", Price = 1.20m, Barcode = "7501000142200", StockQuantity = 50 },
                    new PosCore.Models.Product { Name = "Sabritas Sal 40g", Price = 1.00m, Barcode = "7501011115545", StockQuantity = 75 },
                    new PosCore.Models.Product { Name = "Agua Ciel 1L", Price = 0.90m, Barcode = "7501055310883", StockQuantity = 120 }
                };
                
                _dbContext.Products.AddRange(dummyProducts);
                await _dbContext.SaveChangesAsync();
                
                localProducts = await _dbContext.Products.AsNoTracking().ToListAsync();
            }
        }

        Products.Clear();
        foreach (var p in localProducts)
        {
            Products.Add(p);
        }
        ApplySearchFilter();
    }

    [RelayCommand]
    private void AddToCart(Product product)
    {
        var existingItem = Cart.FirstOrDefault(i => i.ProductId == product.Id);
        
        int currentQuantity = existingItem?.Quantity ?? 0;
        if (product.StockQuantity <= currentQuantity)
        {
            _ = ShowNotification($"Stock insuficiente. Solo hay {product.StockQuantity} disponibles.", true);
            return;
        }

        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            Cart.Add(new OrderItem
            {
                ProductId = product.Id,
                Product = product,
                Quantity = 1,
                UnitPrice = product.Price
            });
        }
        
        UpdateTotal();
    }

    [RelayCommand]
    private void RemoveFromCart(OrderItem item)
    {
        if (item != null)
        {
            Cart.Remove(item);
            UpdateTotal();
        }
    }

    [RelayCommand]
    private void IncreaseQuantity(OrderItem item)
    {
        if (item != null)
        {
            if (item.Product != null && item.Quantity >= item.Product.StockQuantity)
            {
                _ = ShowNotification($"Stock insuficiente. Solo hay {item.Product.StockQuantity} disponibles.", true);
                return;
            }
            item.Quantity++;
            UpdateTotal();
        }
    }

    [RelayCommand]
    [RelayCommand]
    [RelayCommand]
    private void ModifyItem(OrderItem item)
    {
        if (item != null)
        {
            var modifierWindow = new PosCore.Views.ItemModifierWindow(item);
            if (modifierWindow.ShowDialog() == true)
            {
                // Force UI update for the cart by replacing the item to trigger property changed
                var index = Cart.IndexOf(item);
                if (index >= 0) {
                    Cart.RemoveAt(index);
                    Cart.Insert(index, item);
                }
                UpdateTotal();
            }
        }
    }

    [RelayCommand]
    private void DecreaseQuantity(OrderItem item)
    {
        if (item != null)
        {
            if (item.Quantity > 1)
            {
                item.Quantity--;
            }
            else
            {
                Cart.Remove(item);
            }
        }
        
        UpdateTotal();
    }

    [RelayCommand]
    private void OpenShift()
    {
        var shiftWindow = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<PosCore.Views.ShiftWindow>(App.ServiceProvider!);
        shiftWindow.ShowDialog();
    }

    [RelayCommand]
    private void OpenReports()
    {
        var reportsWindow = App.ServiceProvider?.GetService(typeof(Views.ReportsWindow)) as System.Windows.Window;
        reportsWindow?.ShowDialog();
    }

    [RelayCommand]
    private void OpenReturns()
    {
        var returnsWindow = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<PosCore.Views.ReturnsWindow>(App.ServiceProvider!);
        returnsWindow.ShowDialog();
        
        LoadProductsCommand.Execute(null);
    }

    [RelayCommand]
    private void OpenInventory()
    {
        var inventoryWindow = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<PosCore.Views.InventoryWindow>(App.ServiceProvider!);
        inventoryWindow.ShowDialog();
        
        LoadProductsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (!Cart.Any()) return;
        
        var activeShift = await _dbContext.CashRegisterShifts.FirstOrDefaultAsync(s => !s.IsClosed);
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
            // Validar stock antes de continuar
            foreach (var item in Cart)
            {
                var product = await _dbContext.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    if (product.StockQuantity < item.Quantity)
                    {
                        _ = ShowNotification($"Stock insuficiente para {product.Name}.", true);
                        return; // Cancela el proceso
                    }
                    product.StockQuantity -= item.Quantity;
                    item.Product = product;
                }
            }

            var order = new Order
            {
                OrderDate = System.DateTime.Now,
                TotalAmount = Total,
                Items = Cart.ToList(),
                IsReturned = false
            };
            
            _dbContext.Orders.Add(order);
            
            var jsonOptions = new JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };
            var payload = JsonSerializer.Serialize(order, jsonOptions);
            _dbContext.OutboxMessages.Add(new OutboxMessage
            {
                EventType = "OrderCreated",
                Payload = payload,
                CreatedAt = System.DateTime.Now
            });

            await _dbContext.SaveChangesAsync();
            
            _ticketPrinterService.PrintTicket(order);
            
            Cart.Clear();
            UpdateTotal();
            _ = ShowNotification("Venta completada exitosamente.", false);
            
            LoadProductsCommand.Execute(null);
        }
        catch (System.Exception ex)
        {
            _ = ShowNotification($"Error: {ex.Message}", true);
        }
    }


    [RelayCommand]
    private void SuspendOrder()
    {
        if (!Cart.Any()) return;
        
        // Add current cart to suspended
        var suspendedCart = new ObservableCollection<OrderItem>(Cart);
        SuspendedOrders.Add(suspendedCart);
        
        Cart.Clear();
        UpdateTotal();
        _ = ShowNotification("Orden suspendida exitosamente.", false);
    }

    [RelayCommand]
    private void ResumeOrder()
    {
        if (Cart.Any())
        {
            _ = ShowNotification("Hay una orden en curso. Ciérrela o suspéndala antes de retomar otra.", true);
            return;
        }

        var resumeWindow = new PosCore.Views.SuspendedOrdersWindow(SuspendedOrders);
        if (resumeWindow.ShowDialog() == true && resumeWindow.SelectedOrder != null)
        {
            foreach (var item in resumeWindow.SelectedOrder)
            {
                Cart.Add(item);
            }
            SuspendedOrders.Remove(resumeWindow.SelectedOrder);
            UpdateTotal();
            _ = ShowNotification("Orden retomada.", false);
        }
    }

    [RelayCommand]
    private void OpenLogs()
    {
        var logsWindow = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<PosCore.Views.LogViewerWindow>(App.ServiceProvider!);
        logsWindow.ShowDialog();
    }

    [RelayCommand]
    private void TestPrinter()
    {
        try
        {
            bool success = _ticketPrinterService.TestPrinter();
            if (!success)
            {
                IsHardwareError = true;
                HardwareErrorMessage = "Error de impresora detectado durante prueba.";
                _ = ShowNotification("Fallo al imprimir", true);
            }
            else
            {
                IsHardwareError = false;
                _ = ShowNotification("Prueba de impresión enviada.", false);
            }
        }
        catch (System.Exception ex)
        {
            _ = ShowNotification($"Error de impresión: {ex.Message}", true);
        }
    }

    private void UpdateTotal()
    {
        SubTotal = Cart.Sum(i => i.SubTotal);
        // Simulate auto discount evaluation (e.g. 10% off for combo if more than 2 items)
        if (Cart.Count >= 2)
        {
            DiscountAmount = SubTotal * 0.10m;
            IsDiscountApplied = true;
        }
        else
        {
            DiscountAmount = 0;
            IsDiscountApplied = false;
        }
        Total = SubTotal - DiscountAmount;
    }
}
