namespace PosServer.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
}
