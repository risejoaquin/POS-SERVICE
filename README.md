# Super POS Express

Super POS Express es un sistema de Punto de Venta (POS) moderno, inteligente y diseñado con una arquitectura **Offline-First**. Esto significa que las sucursales pueden seguir operando y realizando ventas incluso si pierden su conexión a internet, sincronizando los datos automáticamente una vez que la conexión se restablece.

## 🏗️ Arquitectura del Proyecto

La solución está dividida en tres proyectos principales:

1. **`PosCore`**: La aplicación de escritorio (cliente). Construida con WPF y .NET 8. Utiliza una base de datos SQLite local para garantizar el funcionamiento offline.
2. **`PosServer`**: El servidor central (API). Construido con ASP.NET Core 8. Se encarga de centralizar las ventas, manejar el catálogo global de productos y la autenticación. Utiliza PostgreSQL como base de datos principal.
3. **`PosCore.Tests`**: Proyecto de pruebas unitarias para asegurar la calidad y estabilidad de la lógica de negocio y la sincronización.

## 🚀 Características Principales

*   **Offline-First**: Operación ininterrumpida sin depender de conexión a internet.
*   **Sincronización Automática**: El servicio de sincronización en segundo plano (`SyncService`) envía las ventas acumuladas al servidor cuando hay red disponible.
*   **Actualizaciones Silenciosas**: Integración con Squirrel para distribuir y aplicar actualizaciones automáticamente.
*   **Marca Blanca (White-Label)**: Fácil personalización de colores, logos y nombre de empresa a través del archivo de configuración local.
*   **Gestión Multi-Tenant**: Soporte para múltiples sucursales con identificadores únicos.

## 🛠️ Requisitos Previos

Para compilar y ejecutar este proyecto en tu entorno de desarrollo, necesitas:

*   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [Visual Studio 2022](https://visualstudio.microsoft.com/) o IDE compatible (como Rider o VS Code con la extensión de C#).
*   PostgreSQL (para el servidor) o una cuenta en Supabase.

## 📚 Documentación

Para obtener instrucciones detalladas sobre instalación, despliegue y uso, consulta la documentación incluida en la carpeta `PosCore/Docs`:

*   [Manual de Usuario](./PosCore/Docs/User_Manual.md)
*   [Guía de Instalación](./PosCore/Docs/Installation_Guide.md)
*   [Guía de Personalización](./PosCore/Docs/Customization_Guide.md)
*   [Guía de Despliegue del Servidor](./PosCore/Docs/Deployment_Guide.md)
*   [Guía de Pruebas y CI/CD](./INSTALLER_TESTING_GUIDE.md)

## 💻 Cómo Ejecutar en Desarrollo

### 1. Levantar el Servidor (PosServer)

1. Navega a la carpeta del servidor: `cd PosServer`
2. Configura tu cadena de conexión a PostgreSQL en `appsettings.json` o `appsettings.Development.json`.
3. Aplica las migraciones (la base de datos se creará automáticamente si usas `EnsureCreated` o puedes correr `dotnet ef database update`).
4. Ejecuta el servidor: `dotnet run`
   * El servidor iniciará, por ejemplo, en `http://localhost:5000` o `https://localhost:5001`.

### 2. Levantar el Cliente de Escritorio (PosCore)

1. Navega a la carpeta del cliente: `cd PosCore`
2. Abre `appsettings.json` y asegúrate de que `ApiSettings:BaseUrl` apunte a la URL de tu servidor local (ej. `http://localhost:5000/api/`).
3. Ejecuta la aplicación: `dotnet run`
   * Se abrirá la interfaz gráfica de WPF. Al iniciar sesión, se creará la base de datos local `pos_local.db` y se descargarán los productos del servidor.

## 📦 Empaquetado y Producción

El proyecto incluye un script en PowerShell (`PosCore/build_and_package.ps1`) y un flujo de GitHub Actions (`.github/workflows/build-release.yml`) para compilar un único ejecutable (Self-Contained) y generar un instalador `.exe` utilizando Inno Setup y Squirrel.

Consulta la [Guía de Pruebas e Instalador](./INSTALLER_TESTING_GUIDE.md) para más detalles.
