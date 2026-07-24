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

namespace PosCore;

public partial class App : Application
{
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
                    onInitialInstall: v => mgr.CreateShortcutForThisExe(),
                    onAppUpdate: v => mgr.CreateShortcutForThisExe(),
                    onAppUninstall: v => mgr.RemoveShortcutForThisExe()
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

        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        IConfiguration configuration = builder.Build();

        var services = new ServiceCollection();

        // 0. Configuración de Logging
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddSerilog(dispose: true);
        });

        // 0. Configuración
        services.Configure<AppSettings>(configuration);

        // 1. Inyección del DbContext (EF Core SQLite)
        services.AddDbContext<PosDbContext>(options =>
            options.UseSqlite(configuration.GetSection("DatabaseSettings")["ConnectionString"]));

        // 2. Inyección de HttpClient, Handler de Auth y Servicios
        services.AddSingleton<SessionManager>();
        services.AddTransient<AuthDelegatingHandler>();
        services.AddHttpClient<IApiService, ApiService>()
            .AddHttpMessageHandler<AuthDelegatingHandler>();

        // 3. Inyección de ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<InventoryViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<ReportsViewModel>();

        // 4. Inyección del servicio de sincronización (Singleton)
        services.AddSingleton<SyncService>();

        // 5. Inyección de Views
        services.AddTransient<MainWindow>();
        services.AddTransient<InventoryWindow>();
        services.AddTransient<LoginWindow>();
        services.AddTransient<ReportsWindow>();

        ServiceProvider = services.BuildServiceProvider();

        // Aplicar migraciones al inicio (Reemplaza a EnsureCreated)
        using (var scope = ServiceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PosDbContext>();
            dbContext.Database.Migrate();
        }

        var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
        if (loginWindow.ShowDialog() == true)
        {
            // Iniciar servicio de sincronización en segundo plano
            var syncService = ServiceProvider.GetRequiredService<SyncService>();
            syncService.Start();

            // Resolvemos la ventana principal desde el contenedor de dependencias
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        else
        {
            Application.Current.Shutdown();
        }
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            // Busca actualizaciones en la URL configurada
            using var mgr = new UpdateManager("https://api.tu-pos-central.com/releases");
            var updateInfo = await mgr.CheckForUpdate();
            
            if (updateInfo.ReleasesToApply.Any())
            {
                await mgr.UpdateApp();
                // Opcional: Mostrar mensaje al usuario para reiniciar la app
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fallo al comprobar actualizaciones mediante Squirrel.");
            // Mensaje al usuario para descarga manual si la actualización falla crítica (simulado con Dispatcher)
            Application.Current.Dispatcher.Invoke(() => {
                // MessageBox.Show("No se pudo actualizar. Descarga manualmente desde: https://misitio.com/descargas", "Aviso de Actualización", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Cerrando aplicación...");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
