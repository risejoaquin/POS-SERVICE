using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PosCore.Data;
using PosCore.Models;
using PosCore.Services;

namespace PosCore.ViewModels;

public partial class ReturnsViewModel : ObservableObject
{
    private readonly PosDbContext _dbContext;
    private readonly TicketPrinterService _ticketPrinterService;

    [ObservableProperty]
    private ObservableCollection<Order> _recentOrders = new();

    public ReturnsViewModel(PosDbContext dbContext, TicketPrinterService ticketPrinterService)
    {
        _dbContext = dbContext;
        _ticketPrinterService = ticketPrinterService;
        LoadOrdersCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadOrdersAsync()
    {
        var orders = await _dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.OrderDate)
            .Take(50)
            .ToListAsync();

        RecentOrders.Clear();
        foreach (var o in orders)
        {
            RecentOrders.Add(o);
        }
    }

    [RelayCommand]
    private async Task ReturnOrderAsync(Order order)
    {
        if (order == null) return;

        if (order.IsReturned)
        {
            MessageBox.Show("Esta orden ya fue devuelta.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show($"¿Está seguro que desea devolver la orden {order.Id} por {order.TotalAmount:C}?\nEsto sumará los productos al inventario.", "Confirmar Devolución", MessageBoxButton.YesNo, MessageBoxImage.Question);
        
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                // Marcar como devuelta
                order.IsReturned = true;
                order.LastUpdated = DateTime.Now;

                // Devolver stock
                foreach (var item in order.Items)
                {
                    var product = await _dbContext.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                        product.LastUpdated = DateTime.Now;

                        // Encolar actualización de producto para sync
                        var jsonOptionsProduct = new System.Text.Json.JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };
                        var outboxMessageProduct = new OutboxMessage
                        {
                            EventType = "ProductUpdated",
                            Payload = System.Text.Json.JsonSerializer.Serialize(product, jsonOptionsProduct),
                            CreatedAt = DateTime.Now
                        };
                        _dbContext.OutboxMessages.Add(outboxMessageProduct);
                    }
                }

                // Encolar actualización de orden para sync (nota de crédito / devolución)
                var jsonOptionsOrder = new System.Text.Json.JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles };
                var outboxMessageOrder = new OutboxMessage
                {
                    EventType = "OrderReturned",
                    Payload = System.Text.Json.JsonSerializer.Serialize(order, jsonOptionsOrder),
                    CreatedAt = DateTime.Now
                };
                _dbContext.OutboxMessages.Add(outboxMessageOrder);

                await _dbContext.SaveChangesAsync();

                MessageBox.Show("Devolución procesada con éxito. El inventario ha sido restaurado.", "Devolución Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // IMPRIMIR TICKET DE NOTA DE CRÉDITO (Solo Windows)
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    try
                    {
                        _ticketPrinterService.PrintCreditNote(order, "COM1");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al intentar imprimir el ticket de devolución: {ex.Message}");
                    }
                }

                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar la devolución: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
