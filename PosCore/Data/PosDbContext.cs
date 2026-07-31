using System.Text.Json;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PosCore.Models;
using PosCore.Services;

namespace PosCore.Data;

public class PosDbContext : DbContext
{
    private readonly SessionManager _sessionManager;

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<CashRegisterShift> CashRegisterShifts { get; set; }
    public DbSet<CashMovement> CashMovements { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ProductModifier> ProductModifiers { get; set; }
    public DbSet<ModifierOption> ModifierOptions { get; set; }
    public DbSet<ProductModifierLink> ProductModifierLinks { get; set; }

    public PosDbContext(DbContextOptions<PosDbContext> options, SessionManager sessionManager) : base(options)
    {
        _sessionManager = sessionManager;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Optimización SQLite: Índices
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Barcode)
            .IsUnique();
        
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderDate);

        var dictConverter = new ValueConverter<Dictionary<string, object>, string>(
            v => JsonSerializer.Serialize(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
            v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new Dictionary<string, object>()
        );

        modelBuilder.Entity<Product>().Property(e => e.CustomAttributes).HasConversion(dictConverter);
        modelBuilder.Entity<Order>().Property(e => e.CustomAttributes).HasConversion(dictConverter);
        modelBuilder.Entity<OrderItem>().Property(e => e.CustomAttributes).HasConversion(dictConverter);

            
        // Multi-Tenant: Filtro Global
        modelBuilder.Entity<Product>().HasQueryFilter(e => e.TenantId == _sessionManager.CurrentTenantId);
        modelBuilder.Entity<Order>().HasQueryFilter(e => e.TenantId == _sessionManager.CurrentTenantId);
        modelBuilder.Entity<OrderItem>().HasQueryFilter(e => e.TenantId == _sessionManager.CurrentTenantId);
        modelBuilder.Entity<OutboxMessage>().HasQueryFilter(e => e.TenantId == _sessionManager.CurrentTenantId);
        modelBuilder.Entity<CashRegisterShift>().HasQueryFilter(e => e.TenantId == _sessionManager.CurrentTenantId);
        modelBuilder.Entity<CashMovement>().HasQueryFilter(e => e.TenantId == _sessionManager.CurrentTenantId);
        modelBuilder.Entity<User>().HasQueryFilter(e => e.TenantId == _sessionManager.CurrentTenantId);
        modelBuilder.Entity<ProductModifier>().HasQueryFilter(e => e.TenantId == _sessionManager.CurrentTenantId);
        modelBuilder.Entity<ModifierOption>().HasQueryFilter(e => e.TenantId == _sessionManager.CurrentTenantId);
        modelBuilder.Entity<ProductModifierLink>().HasQueryFilter(e => e.TenantId == _sessionManager.CurrentTenantId);
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AssignTenantIdToAddedEntities();
        UpdateLastUpdatedField();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    public override int SaveChanges()
    {
        AssignTenantIdToAddedEntities();
        UpdateLastUpdatedField();
        return base.SaveChanges();
    }

    private void AssignTenantIdToAddedEntities()
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
        {
            var tenantProperty = entry.Entity.GetType().GetProperty("TenantId");
            if (tenantProperty != null)
            {
                var currentValue = tenantProperty.GetValue(entry.Entity) as string;
                if (string.IsNullOrEmpty(currentValue))
                {
                    tenantProperty.SetValue(entry.Entity, _sessionManager.CurrentTenantId);
                }
            }
        }
    }

    private void UpdateLastUpdatedField()
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
        {
            var lastUpdatedProperty = entry.Entity.GetType().GetProperty("LastUpdated");
            if (lastUpdatedProperty != null)
            {
                lastUpdatedProperty.SetValue(entry.Entity, DateTime.Now);
            }
        }
    }
}
