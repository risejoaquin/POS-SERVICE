using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PosCore.Models;

namespace PosCore.Data;

public class PosDbContextFactory : IDesignTimeDbContextFactory<PosDbContext>
{
    public PosDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var builder = new DbContextOptionsBuilder<PosDbContext>();
        var connectionString = configuration.GetSection("DatabaseSettings")["ConnectionString"];
        builder.UseSqlite(connectionString);

        var appSettings = new AppSettings();
        configuration.Bind(appSettings);
        var options = Options.Create(appSettings);

        return new PosDbContext(builder.Options, options);
    }
}
