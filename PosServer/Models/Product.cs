namespace PosServer.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public string TenantId { get; set; } = string.Empty;
}
