using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PosCore.Data;
using PosCore.Models;
using System.Windows;
using System.Text.Json;
using System;

namespace PosCore.ViewModels;

public partial class InventoryViewModel : ObservableObject
{
    private readonly PosDbContext _dbContext;
    private readonly Services.SyncService _syncService;

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private Product _editingProduct = new();

    [ObservableProperty]
    private bool _isEditing;

    public InventoryViewModel(PosDbContext dbContext, Services.SyncService syncService)
    {
        _dbContext = dbContext;
        _syncService = syncService;

        _syncService.OnSyncCompleted += () => 
        {
            if (LoadProductsCommand.CanExecute(null))
            {
                LoadProductsCommand.Execute(null);
            }
        };

        LoadProductsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadProductsAsync()
    {
        var products = await _dbContext.Products.ToListAsync();
        Products.Clear();
        foreach (var p in products)
        {
            Products.Add(p);
        }
    }

    [RelayCommand]
    private void AddProduct()
    {
        EditingProduct = new Product { StockQuantity = 0, Price = 0 };
        IsEditing = true;
    }

    [RelayCommand]
    private void EditProduct()
    {
        if (SelectedProduct == null) return;
        
        EditingProduct = new Product
        {
            Id = SelectedProduct.Id,
            Name = SelectedProduct.Name,
            Barcode = SelectedProduct.Barcode,
            Price = SelectedProduct.Price,
            StockQuantity = SelectedProduct.StockQuantity,
            TenantId = SelectedProduct.TenantId,
            LastUpdated = SelectedProduct.LastUpdated
        };
        
        IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveProductAsync()
    {
        if (string.IsNullOrWhiteSpace(EditingProduct.Name) || string.IsNullOrWhiteSpace(EditingProduct.Barcode))
        {
            MessageBox.Show("El nombre y el código de barras son obligatorios.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            string eventType = "";

            if (EditingProduct.Id == 0)
            {
                _dbContext.Products.Add(EditingProduct);
                eventType = "ProductCreated";
            }
            else
            {
                var existing = await _dbContext.Products.FindAsync(EditingProduct.Id);
                if (existing != null)
                {
                    existing.Name = EditingProduct.Name;
                    existing.Barcode = EditingProduct.Barcode;
                    existing.Price = EditingProduct.Price;
                    existing.StockQuantity = EditingProduct.StockQuantity;
                    existing.LastUpdated = DateTime.UtcNow; // Set explicitly before saving to outbox
                    _dbContext.Products.Update(existing);
                    
                    // Actualizar el EditingProduct con la info más reciente para el outbox
                    EditingProduct.LastUpdated = existing.LastUpdated;
                }
                eventType = "ProductUpdated";
            }

            var jsonOptions = new JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };
            var outboxMessage = new OutboxMessage
            {
                EventType = eventType,
                Payload = JsonSerializer.Serialize(EditingProduct, jsonOptions),
                CreatedAt = DateTime.UtcNow
            };
            
            _dbContext.OutboxMessages.Add(outboxMessage);

            await _dbContext.SaveChangesAsync();
            IsEditing = false;
            await LoadProductsAsync();
            MessageBox.Show("Producto guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Error al guardar producto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
    }

    [RelayCommand]
    private async Task DeleteProductAsync()
    {
        if (SelectedProduct == null) return;

        var result = MessageBox.Show($"¿Está seguro de eliminar el producto '{SelectedProduct.Name}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                _dbContext.Products.Remove(SelectedProduct);
                await _dbContext.SaveChangesAsync();
                await LoadProductsAsync();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error al eliminar producto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
