using System.Security.Cryptography;
using System.Text;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

    public string CurrentTenantId 
    { 
        get 
        {
            var id = _tenantService?.GetTenantId() ?? string.Empty;
            if (string.IsNullOrEmpty(id))
            {
                throw new InvalidOperationException("CurrentTenantId is not set. Check TenantMiddleware configuration.");
            }
            return id;
        }
    }

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
    public DbSet<ProductModifier> ProductModifiers { get; set; } = null!;
    public DbSet<ModifierOption> ModifierOptions { get; set; } = null!;
    public DbSet<ProductModifierLink> ProductModifierLinks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Conversor para Dictionary<string, object> a JSON string (que en Postgres se mapeará a jsonb)
        var dictConverter = new ValueConverter<Dictionary<string, object>, string>(
            v => JsonSerializer.Serialize(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
            v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? new Dictionary<string, object>()
        );
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        // Optimización de rendimiento: precalcular hashes de diccionarios serializados usando SHA256 para evitar evaluación O(n^2)
        var dictComparer = new ValueComparer<Dictionary<string, object>>(
            (c1, c2) => DictionaryHashCache.GetHash(c1) == DictionaryHashCache.GetHash(c2),
            c => DictionaryHashCache.GetHash(c).GetHashCode(),
            c => c == null ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(c, jsonOptions), jsonOptions)!
        );

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.TenantId);
        
        modelBuilder.Entity<Product>()
            .HasIndex(p => new { p.TenantId, p.Barcode })
            .IsUnique();
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.TenantId);
        
        modelBuilder.Entity<Order>()
            .HasIndex(o => new { o.TenantId, o.OrderDate })
            .IsDescending(false, true);
        
        modelBuilder.Entity<OrderItem>()
            .HasIndex(oi => oi.OrderId);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configuración de CustomAttributes y Filtros Globales (Global Query Filters)
        
        modelBuilder.Entity<Product>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
            entity.Property(e => e.CustomAttributes)
                  
                  .HasConversion(dictConverter, dictComparer);
        });

        modelBuilder.Entity<Order>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
            entity.Property(e => e.CustomAttributes)
                  
                  .HasConversion(dictConverter, dictComparer);
        });

        modelBuilder.Entity<OrderItem>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
            entity.Property(e => e.CustomAttributes)
                  
                  .HasConversion(dictConverter, dictComparer);
        });

        modelBuilder.Entity<User>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        modelBuilder.Entity<License>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        modelBuilder.Entity<ProductModifier>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });
        
        modelBuilder.Entity<ModifierOption>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });

        modelBuilder.Entity<ProductModifierLink>(entity => {
            entity.HasQueryFilter(e => e.TenantId == CurrentTenantId);
        });
    }
}

public static class DictionaryHashCache
{
    private static readonly ConcurrentDictionary<string, string> _hashCache = new ConcurrentDictionary<string, string>();
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static string GetHash(Dictionary<string, object>? dict)
    {
        if (dict == null || dict.Count == 0) return string.Empty;
        
        // Serializamos para obtener una representación canónica
        string json = JsonSerializer.Serialize(dict, _jsonOptions);
        
        // Usamos cache para no recalcular el SHA256 de un mismo JSON
        return _hashCache.GetOrAdd(json, ComputeSha256);
    }

    private static string ComputeSha256(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
