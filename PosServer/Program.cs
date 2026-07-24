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
builder.Services.AddDbContext<CentralDbContext>(options =>
    options.UseNpgsql(connString));

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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure Database exists and is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CentralDbContext>();
    
    // Ensure the database is created first
    dbContext.Database.EnsureCreated();
    
    try 
    {
        // Force create tables just in case EnsureCreated skips them (like on Supabase)
        var creator = dbContext.Database.GetService<IRelationalDatabaseCreator>();
        creator.CreateTables();
    } 
    catch 
    { 
        // Ignore exception if tables already exist
    }
    
    // Seed admin user if it doesn't exist
    if (!dbContext.Users.Any(u => u.Username == "admin"))
    {
        dbContext.Users.Add(new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            TenantId = "TENANT_001"
        });
        dbContext.SaveChanges();
    }
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
