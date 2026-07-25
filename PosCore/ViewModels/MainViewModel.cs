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

    // Propiedades Observables
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
    }

    [RelayCommand]
    private void AddToCart(Product product)
    {
        var existingItem = Cart.FirstOrDefault(i => i.ProductId == product.Id);
        
        int currentQuantity = existingItem?.Quantity ?? 0;
        if (product.StockQuantity <= currentQuantity)
        {
            System.Windows.MessageBox.Show($"Stock insuficiente. Solo hay {product.StockQuantity} disponibles.", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                System.Windows.MessageBox.Show($"Stock insuficiente. Solo hay {item.Product.StockQuantity} disponibles.", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }
            item.Quantity++;
            UpdateTotal();
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
            System.Windows.MessageBox.Show("No hay un turno abierto. Por favor, abra un turno desde 'Arqueo / Turno' antes de cobrar.", "Turno Cerrado", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                        System.Windows.MessageBox.Show($"Stock insuficiente para {product.Name}. Compra no procesada.", "Aviso de Stock", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
            System.Windows.MessageBox.Show("Venta completada exitosamente.", "Éxito", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            
            LoadProductsCommand.Execute(null);
        }
        catch (System.Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al procesar la venta: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
            _ticketPrinterService.TestPrinter();
            System.Windows.MessageBox.Show("Se ha enviado una prueba de impresión. Verifique la impresora y los logs.", "Prueba de Impresión", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        catch (System.Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al enviar prueba de impresión: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void UpdateTotal()
    {
        Total = Cart.Sum(i => i.SubTotal);
    }
}
