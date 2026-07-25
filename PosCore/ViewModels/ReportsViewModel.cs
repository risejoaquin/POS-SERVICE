using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PosCore.Data;
using PosCore.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PosCore.ViewModels;

public class DailySalesSummary
{
    public DateTime Date { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class ProductSaleSummary
{
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public partial class ReportsViewModel : ObservableObject
{
    private readonly PosDbContext _dbContext;

    [ObservableProperty]
    private ObservableCollection<DailySalesSummary> _dailySales = new();

    [ObservableProperty]
    private ObservableCollection<ProductSaleSummary> _topProducts = new();

    [ObservableProperty]
    private ObservableCollection<Product> _lowStockProducts = new();

    [ObservableProperty]
    private decimal _todayTotalRevenue;

    [ObservableProperty]
    private int _todayTotalOrders;

    public ReportsViewModel(PosDbContext dbContext)
    {
        _dbContext = dbContext;
        
        // Ensure QuestPDF community license is set
        QuestPDF.Settings.License = LicenseType.Community;
        
        LoadDataCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        var today = DateTime.Today;

        // Daily Sales
        var orders = await _dbContext.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ToListAsync();

        var validOrders = orders.Where(o => !o.IsReturned).ToList();
        var salesByDay = validOrders
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new DailySalesSummary
            {
                Date = g.Key,
                TotalOrders = g.Count(),
                TotalRevenue = g.Sum(o => o.TotalAmount)
            })
            .OrderByDescending(d => d.Date)
            .ToList();

        DailySales.Clear();
        foreach (var s in salesByDay) DailySales.Add(s);

        // Today's summary (Cierre de caja)
        var todaySales = salesByDay.FirstOrDefault(s => s.Date == today);
        if (todaySales != null)
        {
            TodayTotalRevenue = todaySales.TotalRevenue;
            TodayTotalOrders = todaySales.TotalOrders;
        }
        else
        {
            TodayTotalRevenue = 0;
            TodayTotalOrders = 0;
        }

        // Top Products
        var allItems = validOrders.SelectMany(o => o.Items).ToList();
        var topProds = allItems
            .GroupBy(i => i.ProductId)
            .Select(g => new ProductSaleSummary
            {
                ProductName = g.First().Product?.Name ?? g.First().ProductBarcode,
                QuantitySold = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.SubTotal)
            })
            .OrderByDescending(p => p.QuantitySold)
            .Take(10)
            .ToList();

        TopProducts.Clear();
        foreach (var p in topProds) TopProducts.Add(p);

        // Low stock products (e.g., stock < 10)
        var lowStock = await _dbContext.Products
            .Where(p => p.StockQuantity < p.MinStockThreshold)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync();

        LowStockProducts.Clear();
        foreach (var p in lowStock) LowStockProducts.Add(p);
    }

    [RelayCommand]
    private void ExportEndOfDayPdf()
    {
        try
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Cierre_Caja_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Text("Cierre de Caja - POS Express").SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(column => 
                    {
                        column.Spacing(20);
                        
                        column.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");
                        column.Item().Text($"Órdenes de hoy: {TodayTotalOrders}");
                        column.Item().Text($"Ingresos de hoy: {TodayTotalRevenue:C}").Bold().FontSize(16);

                        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        column.Item().Text("Productos con Bajo Stock:").SemiBold().FontSize(14);
                        if (LowStockProducts.Any())
                        {
                            foreach (var prod in LowStockProducts)
                            {
                                column.Item().Text($"- {prod.Name} ({prod.Barcode}): {prod.StockQuantity} unidades").FontColor(Colors.Red.Medium);
                            }
                        }
                        else
                        {
                            column.Item().Text("No hay productos con bajo stock.");
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf(filePath);

            MessageBox.Show($"Reporte exportado correctamente a:\n{filePath}", "Exportar PDF", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al exportar PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
