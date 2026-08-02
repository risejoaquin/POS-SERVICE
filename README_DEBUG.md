El problema por el cual `posbuild.exe` no se abre (se cierra inmediatamente sin mostrar nada) se debe a una **excepción de XAML al arrancar (XamlParseException)**.

### Causa:
En `MainWindow.xaml.cs`, dentro del constructor, se inicializan los `UserControl` de los pasos (`new Step1Environment()`, etc.) **antes** de que estén agregados a la ventana. 
Esos `UserControl` (como `Step1Environment.xaml`) tienen *bindings* que usan `{StaticResource StringToBoolConverter}`. Como el control se inicializa en código antes de unirse al árbol visual, no puede encontrar ese convertidor que estaba definido solo dentro de `<Window.Resources>` en el archivo `MainWindow.xaml`, y la aplicación se estrella (crashea) al instante.

### Solución aplicada:
1. **Se movieron los Convertidores a `App.xaml`**: Hemos sacado `StringToBoolConverter`, `DbTypeToVisibilityConverter` y `InverseBooleanToVisibilityConverter` de `MainWindow.xaml` y los declaramos globalmente en `<Application.Resources>` dentro de `App.xaml`. Esto garantiza que los componentes los encuentren en cualquier momento, incluso al ser instanciados desde el código.
2. **Manejador Global de Errores**: Se agregó en `App.xaml.cs` un código que atrapará cualquier excepción fatal y te mostrará un mensaje de error con los detalles (MessageBox). Así, si llega a fallar algo más en el futuro, no se cerrará de golpe de forma invisible, sino que te mostrará exactamente la línea del error.
3. **Advertencia de Eventos**: También se corrigió la advertencia de nulabilidad (`?`) del `PropertyChangedEventHandler`.

Sincroniza tus archivos o descarga los cambios, y al ejecutar `dotnet run` o abrir el `.exe`, la ventana del PosBuilder debería abrirse sin problemas.
