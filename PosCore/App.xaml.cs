using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using PosCore.Data;
using PosCore.Models;
using PosCore.Services;
using PosCore.ViewModels;
using PosCore.Views;
using Squirrel;
using System.Threading.Tasks;
using Serilog;
using System;
using System.Linq;

namespace PosCore;

public partial class App : Application
{
    private async Task CheckForUpdatesAsync()
    {
        try
        {
            using (var mgr = new UpdateManager("https://api.tu-pos-central.com/releases"))
            {
                await mgr.UpdateApp();
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error al actualizar la aplicación.");
        }
    }
    public static IServiceProvider? ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Global Exception Handlers
        this.DispatcherUnhandledException += (s, args) =>
        {
            Log.Fatal(args.Exception, "Unhandled UI exception");
            MessageBox.Show($"Ha ocurrido un error inesperado: {args.Exception.Message}\n\nRevisa el archivo de logs para más detalles.", "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            Log.Fatal(args.ExceptionObject as Exception, "Unhandled Domain exception");
            MessageBox.Show($"Ha ocurrido un error fatal: {(args.ExceptionObject as Exception)?.Message}\n\nRevisa el archivo de logs para más detalles.", "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
        };

        // Configuración de Logging (Serilog)
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File("logs/pos-log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Information("Iniciando aplicación Super POS Express...");

#if !DEBUG
        // 1. Manejar eventos de Squirrel (accesos directos al instalar/desinstalar)
        try 
        {
            using (var mgr = new UpdateManager("https://api.tu-pos-central.com/releases"))
            {
                SquirrelAwareApp.HandleEvents(
                    onInitialInstall: (v, t) => mgr.CreateShortcutForThisExe(),
                    onAppUpdate: (v, t) => mgr.CreateShortcutForThisExe(),
                    onAppUninstall: (v, t) => mgr.RemoveShortcutForThisExe()
                    );
            }
            
            // 2. Comprobar actualizaciones en segundo plano
            Task.Run(async () => await CheckForUpdatesAsync());
        } 
        catch 
        {
            // Ignorar errores si Squirrel falla o no hay conexión
        }
#endif

        string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        var secureSettings = SecureConfigManager.LoadAndSecureConfig(configPath);
        
        var services = new ServiceCollection();

        // 0. Configuración de Logging
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddSerilog(dispose: true);
        });

        // 0. Configuración (Opciones en memoria a partir de los datos seguros)
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(secureSettings));

        // 1. Inyección del DbContext (EF Core SQLite)
        services.AddDbContext<PosDbContext>(options =>
            options.UseSqlite(secureSettings.DatabaseSettings.ConnectionString));

        // 2. Inyección de HttpClient, Handler de Auth y Servicios
        services.AddSingleton<SessionManager>();
        services.AddTransient<AuthDelegatingHandler>();
        services.AddHttpClient<LicenseService>();
        services.AddHttpClient<IApiService, ApiService>()
            .AddHttpMessageHandler<AuthDelegatingHandler>();

        // 3. Inyección de ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<InventoryViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<ReturnsViewModel>();
        services.AddTransient<ShiftViewModel>();
        services.AddTransient<ShiftWindow>();
        services.AddTransient<LogViewerViewModel>();
        services.AddTransient<LogViewerWindow>();

        // 4. Inyección del servicio de sincronización (Singleton)
        services.AddSingleton<SyncService>();
        services.AddSingleton<TicketPrinterService>();

        // 5. Inyección de Views
        services.AddTransient<MainWindow>();
        services.AddTransient<InventoryWindow>();
        services.AddTransient<LoginWindow>();
        services.AddTransient<ReportsWindow>();
        services.AddTransient<ReturnsWindow>();

        ServiceProvider = services.BuildServiceProvider();


        // Aplicar migraciones y Backup
        using (var scope = ServiceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PosDbContext>();
            var connStr = secureSettings.DatabaseSettings.ConnectionString;
            
            DatabaseBackupService.ManageDatabaseBackup(connStr);

            try 
            {
                dbContext.Database.Migrate();
            } 
            catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 11 || ex.SqliteErrorCode == 26 || ex.Message.Contains("malformed"))
            {
                // 11 = SQLITE_CORRUPT, 26 = SQLITE_NOTADB
                Log.Error(ex, "Base de datos corrupta detectada.");
                if (DatabaseBackupService.TryRestoreFromBackup(connStr))
                {
                    Application.Current.Shutdown();
                    return;
                }
                else 
                {
                    MessageBox.Show("No se pudo reparar la base de datos. Póngase en contacto con el soporte.", "Error fatal", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al aplicar migraciones de base de datos.");
            }
        }

        Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var sessionManager = ServiceProvider.GetRequiredService<SessionManager>();
        bool isLoggedIn = sessionManager.LoadSession();

        if (!isLoggedIn)
        {
            var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
            isLoggedIn = loginWindow.ShowDialog() == true;
        }

        if (isLoggedIn)
        {
            var licenseService = ServiceProvider.GetRequiredService<LicenseService>();
            bool isLicenseValid = licenseService.ValidateLicenseAsync().GetAwaiter().GetResult();
            if (!isLicenseValid)
            {
                Application.Current.Shutdown();
                return;
            }

            var syncService = ServiceProvider.GetRequiredService<SyncService>();
            syncService.Start();
            
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            Application.Current.MainWindow = mainWindow;
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            mainWindow.Show();
        }
        else
        {
            Application.Current.Shutdown();
        }
    }
}
