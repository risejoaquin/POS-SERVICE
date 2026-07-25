# Manual de Usuario - Super POS Express

Bienvenido al manual de usuario de Super POS Express, su sistema de punto de venta rápido y confiable, diseñado para funcionar con o sin conexión a internet.

## 1. Inicio de Sesión
Al iniciar la aplicación, se le solicitará su **Usuario** y **Contraseña**.
- Ingrese sus credenciales.
- Si no hay conexión al servidor pero usted ha iniciado sesión antes, el sistema le permitirá ingresar en modo fuera de línea gracias a la persistencia segura local.

## 2. Pantalla Principal (Ventas)
La pantalla principal es donde ocurre la operación diaria de ventas.

- **Búsqueda de Productos**: Use la barra superior para escanear códigos de barras o buscar productos por nombre. Al presionar "Buscar", los resultados aparecerán en la cuadrícula inferior.
- **Agregar al Carrito**: Puede hacer clic en "Agregar" en la tarjeta de un producto para añadirlo al ticket actual.
- **Modificar Carrito**: En el panel lateral derecho, verá los productos agregados. Puede aumentar (+), disminuir (-) la cantidad, o eliminar (Eliminar) el producto del carrito.
- **Cobrar**: Una vez que el carrito esté listo, haga clic en el botón azul de **Cobrar**. El sistema validará el stock, registrará la venta, generará el recibo y lo enviará a la impresora configurada.

## 3. Arqueo y Turnos
Antes de poder realizar ventas, es necesario abrir un turno de caja.
- Haga clic en el botón **"Arqueo / Turno"** en la barra superior.
- **Abrir Turno**: Si no hay un turno activo, indique el "Fondo Inicial" (efectivo con el que inicia el día) y haga clic en **Abrir Turno**.
- **Cerrar Turno**: Al finalizar su jornada, vuelva a ingresar a esta pantalla. Podrá ver el resumen de ventas. Indique el monto real en caja en "Efectivo Final" para ver la diferencia (faltante/sobrante) y haga clic en **Cerrar Turno**.

## 4. Gestión de Inventario
- Haga clic en el botón **"Inventario"**.
- Aquí podrá ver el listado completo de productos locales.
- Puede **Editar** el precio o el stock manualmente. 
- Los cambios realizados aquí se sincronizarán automáticamente con la base de datos central cuando haya conexión a internet.

## 5. Devoluciones y Notas de Crédito
- Haga clic en el botón **"Devoluciones"**.
- Ingrese el número de Ticket (Order ID) de la venta que desea devolver.
- El sistema cargará los detalles de la venta original. Confirme la operación para realizar la devolución, regresar el stock al inventario y generar un comprobante de Nota de Crédito en la ticketera.

## 6. Sincronización Automática
El sistema trabaja sin interrupciones aunque se caiga el internet. 
- Cualquier venta o actualización de producto se guarda localmente en su base de datos SQLite.
- Cuando el internet regresa, el proceso en segundo plano (SyncService) se encarga de enviar las ventas al servidor central y descargar nuevos productos o cambios de precio.

## 7. Soporte Técnico y Logs
Si experimenta un error, puede ayudar al equipo técnico enviando un reporte.
- Haga clic en **"Logs"** en el panel superior.
- Se abrirá una ventana donde podrá visualizar los registros técnicos del sistema.
- Haga clic en **"Enviar a Soporte"** para empaquetar de forma segura los archivos de diagnóstico y enviarlos automáticamente al equipo de desarrollo.
