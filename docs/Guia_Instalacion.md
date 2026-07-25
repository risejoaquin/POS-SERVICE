# Guía de Instalación y Configuración

El sistema consta de dos partes: 
1. **PosServer**: El servidor en la nube (API) que centraliza la información.
2. **PosCore**: El cliente de escritorio (WPF) diseñado para puntos de venta, con soporte offline.

## 1. Requisitos Previos
- **Para el Cliente (PosCore)**: Windows 10 o superior, con **.NET 8 Desktop Runtime** instalado.
- **Para el Servidor (PosServer)**: Entorno compatible con contenedores Docker o servidor con .NET 8 SDK. Base de datos PostgreSQL.
- **Impresora térmica**: Configurada y mapeada a un puerto COM (por ejemplo `COM1`).

## 2. Despliegue del Servidor Central (PosServer)
El servidor utiliza una base de datos PostgreSQL para almacenar todos los datos de forma centralizada (Multitenant).

1. Abra una terminal en la carpeta `/PosServer`.
2. Configure la variable de entorno de conexión a base de datos.
   ```bash
   export DATABASE_URL="postgres://usuario:password@host:5432/posdb"
   ```
3. Ejecute la aplicación o constrúyala para despliegue:
   ```bash
   dotnet run
   ```
   *Nota: Al iniciar, el servidor ejecutará automáticamente las migraciones para crear la estructura de tablas y un usuario administrador por defecto (admin / admin123).*

## 3. Instalación del Cliente de Escritorio (PosCore)
La aplicación cliente se distribuye idealmente mediante **Squirrel** (para actualizaciones automáticas). Para entornos de desarrollo o despliegues manuales:

1. Compile la aplicación WPF:
   ```bash
   cd PosCore
   dotnet build -c Release
   ```
2. Distribuya la carpeta binaria resultante (`bin/Release/net8.0-windows/`) a la máquina del cliente.

## 4. Configuración del Cliente
La base de datos local y las configuraciones de seguridad se auto-administran. 
- **Base de Datos Local**: PosCore utiliza una base de datos local SQLite (`pos_local.db`). No requiere instalación adicional. Al primer inicio, el sistema aplicará las migraciones y creará el archivo.
- **Backups Automáticos**: En cada inicio de sesión, el sistema hace una copia de seguridad (`pos_local.db.bak`). Si detecta corrupción de archivos, le ofrecerá restaurar el último respaldo de forma automática.
- **Protección de Datos (DPAPI)**: Para proteger la cadena de conexión local y la URL del servidor, la primera vez que inicia la app, los datos sensibles en `appsettings.json` se encriptarán usando la API de Protección de Datos de Windows (DPAPI) vinculada a la cuenta del usuario de Windows.
