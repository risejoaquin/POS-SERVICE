using Microsoft.EntityFrameworkCore;
using PosServer.Models;

namespace PosServer.Data;

public class CentralDbContext : DbContext
{
    public CentralDbContext(DbContextOptions<CentralDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Product>()
            .HasIndex(p => new { p.TenantId, p.Barcode })
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderDate);
    }
}
