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
    private readonly IApiService _apiService;

    // Propiedades Observables (CommunityToolkit automáticamente genera las propiedades)
    [ObservableProperty]
    private ObservableCollection<OrderItem> _cart = new();

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private AppSettings _settings;

    [ObservableProperty]
    private SolidColorBrush _primaryColorBrush = Brushes.Blue;

    private readonly SyncService _syncService;

    public MainViewModel(PosDbContext dbContext, IApiService apiService, IOptions<AppSettings> settings, SyncService syncService)
    {
        _dbContext = dbContext;
        _apiService = apiService;
        _settings = settings.Value;
        _syncService = syncService;

        _syncService.OnSyncCompleted += () => 
        {
            if (LoadProductsCommand.CanExecute(null))
            {
                LoadProductsCommand.Execute(null);
            }
        };
        
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
        // Optimización: Usar AsNoTracking para consultas de solo lectura
        var localProducts = _dbContext.Products.AsNoTracking().ToList();
        
        if (!localProducts.Any())
        {
            try 
            {
                var cloudProducts = await _apiService.GetProductsAsync();
                if (cloudProducts.Any())
                {
                    _dbContext.Products.AddRange(cloudProducts);
                    await _dbContext.SaveChangesAsync();
                    localProducts = cloudProducts;
                }
            }
            catch
            {
                // Fallback si no hay internet (o API central no existe aún)
            }

            // Datos semilla para poder probar offline de inmediato
            if (!localProducts.Any())
            {
                 localProducts = new List<Product>
                 {
                     new Product { Name = "Café Americano", Barcode = "7501001", Price = 25.50m, StockQuantity = 100 },
                     new Product { Name = "Latte Macchiato", Barcode = "7501002", Price = 35.00m, StockQuantity = 50 },
                     new Product { Name = "Croissant", Barcode = "7501003", Price = 20.00m, StockQuantity = 30 },
                     new Product { Name = "Agua Mineral", Barcode = "7501004", Price = 15.00m, StockQuantity = 40 }
                 };
                 _dbContext.Products.AddRange(localProducts);
                 await _dbContext.SaveChangesAsync();
            }
        }

        Products.Clear();
        foreach (var p in localProducts)
        {
            Products.Add(p);
        }
    }

    [RelayCommand]
    private void AddToCart(Product product)
    {
        var existingItem = Cart.FirstOrDefault(i => i.ProductId == product.Id);
        
        if (existingItem != null)
        {
            existingItem.Quantity++;
            // Forzamos actualización de la UI creando una nueva lista para disparar el evento
            // o se puede implementar INotifyPropertyChanged en OrderItem.
            var tempCart = Cart.ToList();
            Cart.Clear();
            foreach(var item in tempCart) Cart.Add(item);
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
    private void OpenReports()
    {
        var reportsWindow = App.ServiceProvider?.GetService(typeof(Views.ReportsWindow)) as System.Windows.Window;
        reportsWindow?.ShowDialog();
    }

    [RelayCommand]
    private void OpenInventory()
    {
        var inventoryWindow = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<PosCore.Views.InventoryWindow>(App.ServiceProvider!);
        inventoryWindow.ShowDialog();
        
        // Refrescar el catálogo al cerrar el inventario por si hubo cambios (agregados/eliminados/editados)
        LoadProductsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (!Cart.Any()) return;

        try
        {
            var order = new Order
            {
                OrderDate = DateTime.Now,
                TotalAmount = Total,
                Items = Cart.Select(i => new OrderItem 
                {
                    ProductId = i.ProductId,
                    ProductBarcode = i.Product?.Barcode ?? string.Empty,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            // Restar stock
            foreach (var item in Cart)
            {
                var product = await _dbContext.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                }
            }

            // 1. Guardar orden localmente
            _dbContext.Orders.Add(order);
            
            // 2. Crear mensaje de Outbox
            var jsonOptions = new System.Text.Json.JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };
            var outboxMessage = new OutboxMessage
            {
                EventType = "OrderCreated",
                Payload = System.Text.Json.JsonSerializer.Serialize(order, jsonOptions),
                CreatedAt = DateTime.Now
            };
            _dbContext.OutboxMessages.Add(outboxMessage);

            await _dbContext.SaveChangesAsync();

            // 3. Limpiar carrito
            Cart.Clear();
            UpdateTotal();
            System.Windows.MessageBox.Show("Compra completada con éxito.", "Venta", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

            // Refrescar catálogo
            LoadProductsCommand.Execute(null);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error en Checkout: {ex.Message}");
            System.Windows.MessageBox.Show($"Error al completar compra: {ex.Message}\n{ex.InnerException?.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void UpdateTotal()
    {
        Total = Cart.Sum(i => i.SubTotal);
    }
}
