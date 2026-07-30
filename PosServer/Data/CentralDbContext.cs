using Microsoft.EntityFrameworkCore;
using PosServer.Models;
using PosServer.Services;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PosServer.Data;

public class CentralDbContext : DbContext
{
    private readonly ITenantService? _tenantService;

    public string CurrentTenantId => _tenantService?.GetTenantId() ?? string.Empty;

    public CentralDbContext(DbContextOptions<CentralDbContext> options, ITenantService tenantService) : base(options)
    {
        _tenantService = tenantService;
    }

    // Constructor sin ITenantService para herramientas de diseño
    public CentralDbContext(DbContextOptions<CentralDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<License> Licenses { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Conversor para Dictionary<string, object> a JSON string (que en Postgres se mapeará a jsonb)
        var dictConverter = new ValueConverter<Dictionary<string, object>, string>(
            v => JsonSerializer.Serialize(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
            v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new Dictionary<string, object>()
        );

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
            
        modelBuilder.Entity<Product>()
            .HasIndex(p => new { p.TenantId, p.Barcode })
            .IsUnique();
            
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderDate);

        // Configuración de CustomAttributes y Filtros Globales (Global Query Filters)
        
        modelBuilder.Entity<Product>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId || CurrentTenantId == "");
            entity.Property(e => e.CustomAttributes)
                  .HasColumnType("jsonb")
                  .HasConversion(dictConverter);
        });

        modelBuilder.Entity<Order>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId || CurrentTenantId == "");
            entity.Property(e => e.CustomAttributes)
                  .HasColumnType("jsonb")
                  .HasConversion(dictConverter);
        });

        modelBuilder.Entity<OrderItem>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId || CurrentTenantId == "");
            entity.Property(e => e.CustomAttributes)
                  .HasColumnType("jsonb")
                  .HasConversion(dictConverter);
        });

        modelBuilder.Entity<User>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId || CurrentTenantId == "");
        });

        modelBuilder.Entity<License>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId || CurrentTenantId == "");
        });
    }
}
