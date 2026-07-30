using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PosServer.Data;
using PosServer.Models;
using BCrypt.Net;
using System.Net;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Database
var connString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
var envDbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(envDbUrl)) {
    connString = envDbUrl;
}

if (connString.StartsWith("\"") && connString.EndsWith("\"")) {
    connString = connString.Trim('"');
}

if (connString.StartsWith("postgres://") || connString.StartsWith("postgresql://")) {
    var uri = new Uri(connString);
    var userInfo = uri.UserInfo.Split(':', 2); // Limit split to 2 in case password has colon
    var username = WebUtility.UrlDecode(userInfo[0]);
    var password = userInfo.Length > 1 ? WebUtility.UrlDecode(userInfo[1]) : "";
    connString = $"Host={uri.Host};Port={(uri.IsDefaultPort ? 5432 : uri.Port)};Database={uri.LocalPath.TrimStart('/')};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=True";
}

if (connString.Contains("supabase.com") || connString.Contains("pooler"))
{
    // Fix for Supabase Transaction Pooler (pgbouncer) which breaks EF Core Prepared Statements
    if (!connString.Contains("Max Auto Prepare"))
    {
        connString += ";Max Auto Prepare=0;Pooling=false;";
    }
}

builder.Services.AddDbContext<CentralDbContext>(options =>
    options.UseNpgsql(connString, o => o.CommandTimeout(120)));

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/", () => "POS Server is running!");

// Servir la carpeta releases estáticamente para Squirrel
var releasesPath = Path.Combine(builder.Environment.ContentRootPath, "releases");
if (!Directory.Exists(releasesPath))
{
    Directory.CreateDirectory(releasesPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(releasesPath),
    RequestPath = "/releases",
    ServeUnknownFileTypes = true // Importante para .nupkg y RELEASES
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Init Database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CentralDbContext>();
    
    // Attempt to create tables if they don't exist
    try
    {
        var creator = dbContext.Database.GetService<IRelationalDatabaseCreator>();
        if (!creator.Exists())
        {
            creator.Create();
        }
        creator.CreateTables();
    }
    catch
    {
        // Tables already exist, try to add new columns for updates
        try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"Orders\" ADD COLUMN \"ReturnReason\" text DEFAULT '';"); } catch { }
        try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"Orders\" ADD COLUMN \"AuthorizedBy\" text DEFAULT '';"); } catch { }
        try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"Orders\" ADD COLUMN \"PaymentDetails\" text DEFAULT '';"); } catch { }
        try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"Orders\" ADD COLUMN \"SubTotal\" numeric DEFAULT 0;"); } catch { }
        try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"Orders\" ADD COLUMN \"TaxAmount\" numeric DEFAULT 0;"); } catch { }
        try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"Users\" ADD COLUMN \"Role\" text DEFAULT 'Admin';"); } catch { }
        try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"OrderItem\" ADD COLUMN \"Notes\" text DEFAULT '';"); } catch { }
        try { dbContext.Database.ExecuteSqlRaw("ALTER TABLE \"OrderItem\" ADD COLUMN \"Discount\" numeric DEFAULT 0;"); } catch { }
    }
}

// Seed Database in a separate scope to ensure clean connection
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CentralDbContext>();
    try 
    {
        if (!dbContext.Users.Any(u => u.Username == "admin"))
        {
            dbContext.Users.Add(new User
            {
                Username = "admin",
                Pin = "admin123",
                TenantId = "TENANT_001",
                Role = "Admin"
            });
            dbContext.Users.Add(new User
            {
                Username = "cajero",
                Pin = "cajero123",
                TenantId = "TENANT_001",
                Role = "Cajero"
            });
            dbContext.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding database: {ex.Message}");
    }
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
