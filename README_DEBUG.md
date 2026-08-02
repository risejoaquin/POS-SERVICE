He solucionado los errores de compilación del proyecto `PosCore`. 

El problema radicaba en que en `MainWindow.xaml.cs` se intentaba asignar valores a propiedades de solo lectura (como `SubTotal`) y se estaban usando nombres de propiedades incorrectos que no existen en el modelo (como usar `Price` y `Name` cuando el modelo `OrderItem` realmente tiene `UnitPrice` y `Product`). Además, faltaba la implementación del comando `ApplyDiscount` en el `MainViewModel`.

### Pasos para probar la solución:

1. **Sincroniza/copia** los siguientes archivos que acabo de reparar a tu equipo local (en la carpeta `PosCore`):
   - `PosCore/Views/MainWindow.xaml.cs`
   - `PosCore/ViewModels/MainViewModel.cs`

2. Vuelve a ejecutar la compilación desde tu terminal:
   ```powershell
   dotnet build
   ```

Los 7 errores deben haber desaparecido completamente (las únicas cosas que verás ahora son advertencias o "warnings", las cuales no impiden que se compile ni que funcione la aplicación).

¡Avísame si al compilar surge algún otro detalle para resolverlo al instante!
