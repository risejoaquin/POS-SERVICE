const fs = require('fs');

let content = fs.readFileSync('README.md', 'utf8');

content = content.replace(
    '*   **Punto de Venta (Ventas y Cobro)**: Interfaz intuitiva para añadir productos al carrito, modificar cantidades y completar ventas rápidamente.\n*   **Impresión de Tickets Directa (Térmica)**: Impresión nativa mediante comandos RAW (ESC/POS y `winspool.drv`) hacia impresoras térmicas en entornos Windows (por ejemplo, impresoras en puertos USB, COM, o impresoras compartidas instaladas en el sistema). Incluye un botón para probar la conectividad de la impresora.\n*   **Gestión de Inventario**: Control de existencias, umbrales de stock mínimo (`MinStockThreshold`) y alertas visuales.',
    `*   **Punto de Venta (Ventas y Cobro)**: Interfaz intuitiva para añadir productos al carrito, modificar cantidades y completar ventas rápidamente.
*   **Módulo de Pagos Avanzado**: Ventana de cobro con teclado numérico táctil (Numpad), cálculo de cambio automático, cobro exacto y simulación de programa de lealtad (búsqueda de clientes por teléfono).
*   **Suspensión y Retoma de Órdenes**: Capacidad de guardar ventas en proceso (en espera) y retomarlas más tarde, ideal para no bloquear la caja.
*   **Descuentos y Modificadores**: Permite agregar notas personalizadas por producto (ej. "sin cebolla") y aplicar descuentos directos en pesos o porcentajes al subtotal.
*   **Autorización de Gerente**: Ventanas de control de acceso por PIN para operaciones sensibles y registro de motivos en caso de anulaciones y devoluciones.
*   **Impresión de Tickets Directa (Térmica)**: Impresión nativa mediante comandos RAW (ESC/POS y \`winspool.drv\`) hacia impresoras térmicas en entornos Windows. Incluye reimpresión de tickets.
*   **Feedback de Red y Hardware**: Indicadores visuales en tiempo real del estado de conexión (Online/Offline) y banners de advertencia sobre problemas con la impresora.
*   **Gestión de Inventario**: Control de existencias, umbrales de stock mínimo (\`MinStockThreshold\`) y alertas visuales.`
);

fs.writeFileSync('README.md', content);
