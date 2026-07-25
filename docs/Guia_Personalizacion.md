# Guía de Personalización y Configuración

El cliente de escritorio (PosCore) es altamente configurable a través de su archivo `appsettings.json`, que se encuentra en el directorio raíz donde se ejecuta la aplicación.

A continuación, detallamos la estructura y las opciones disponibles para personalizar su Punto de Venta.

## Archivo `appsettings.json`

Al abrir el archivo con un editor de texto (como Notepad o VS Code), verá una estructura similar a esta:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://pos-service-production.up.railway.app/"
  },
  "DatabaseSettings": {
    "ConnectionString": "Data Source=pos_local.db"
  },
  "WhiteLabel": {
    "CompanyName": "Mi Empresa POS",
    "PrimaryColor": "#6366f1",
    "LogoPath": "Assets/logo.png"
  },
  "Modules": {
    "EnableTableManagement": false,
    "EnableInventoryControl": true
  },
  "Tenant": {
    "CurrentTenantId": "TENANT_001"
  },
  "Printer": {
    "PortName": "COM1"
  },
  "License": {
    "LicenseKey": "VAL-MI-LICENCIA",
    "LastValidationDate": null
  }
}
```

### 1. Configuración de Marca Blanca (WhiteLabel)
Personalice el aspecto del sistema para adaptarlo a su empresa.

- **`CompanyName`**: El nombre comercial de su empresa. Este nombre aparecerá en la cabecera de los tickets impresos, así como en las pantallas de la aplicación.
- **`PrimaryColor`**: El color distintivo de su marca en formato Hexadecimal (por ejemplo, `#FF0000` para rojo, `#6366f1` para índigo). Este color se aplicará dinámicamente a la interfaz (botones, cabeceras).
- **`LogoPath`**: Ruta relativa a la imagen de su logotipo. (Nota: En esta versión inicial, el soporte gráfico principal es a través de colores corporativos).

### 2. Configuración de Hardware
- **Impresora (`Printer.PortName`)**: Define el puerto serie virtual donde está conectada su impresora de tickets térmica ESC/POS. Los valores comunes son `COM1`, `COM2`, `COM3`, etc. Asegúrese de que coincida con la configuración en su Administrador de Dispositivos de Windows.

### 3. Conectividad y Licenciamiento
- **`ApiSettings.BaseUrl`**: La URL de su servidor central PosServer. (Nota: La aplicación encriptará este valor al ejecutarse por primera vez para mayor seguridad).
- **`License.LicenseKey`**: Su clave de licencia de software comercial. La licencia se valida periódicamente contra el servidor central para permitir el uso en modo offline hasta por 7 días.

### 4. Módulos y Multitenancy
- **`Modules`**: Permite habilitar o deshabilitar ciertas funcionalidades de interfaz.
  - `EnableInventoryControl`: Muestra u oculta la funcionalidad de ajuste manual de stock.
- **`Tenant.CurrentTenantId`**: Identificador único de su sucursal o franquicia dentro de la base de datos central. Todas sus operaciones (ventas, stock) quedarán asociadas criptográficamente a este identificador.

## Reinicio Necesario
Tenga en cuenta que cualquier modificación al archivo `appsettings.json` requiere que reinicie la aplicación de escritorio para que los cambios de personalización surtan efecto (lectura en el arranque).
