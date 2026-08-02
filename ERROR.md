# Errores y Tareas Pendientes del Proyecto (Realidad del Código)

Este documento ha sido actualizado para reflejar estrictamente el estado actual del código del proyecto (sin listados hipotéticos). Actualmente, **el proyecto compila correctamente al 100%** (PosCore, PosServer y PosBuilder).

## 1. Excepciones `NotImplementedException` (Menores)
Existen algunas excepciones por implementar en los convertidores de UI (normalmente en el método `ConvertBack` que rara vez se usa, pero que es buena práctica tener en cuenta):
- `PosCore/Converters/InverseBooleanToVisibilityConverter.cs` (ConvertBack no implementado).
- `PosCore/Converters/LessThanZeroConverter.cs` (ConvertBack no implementado).
- `PosBuilder/Converters.cs` (ConvertBack no implementado).
- `PosBuilder/InverseBooleanToVisibilityConverter.cs` (ConvertBack no implementado).

## 2. Advertencias de Compilación (Warnings)
Durante la compilación se detectan advertencias sobre `Nullable Reference Types` (CS8600, CS8603, CS8618) y algunos casteos o asignaciones posiblemente nulas. Aunque no impiden que el software funcione ni compile, son deudas técnicas menores a limpiar en el futuro para mayor seguridad de tipos.
- Ejemplos en: `test_comparer.cs`, `ShortcutManager.cs`, `SettingsWindow.xaml.cs`, etc.

## 3. Características Core a Desarrollar (Próximos Pasos)
Tal como se definió, el objetivo a corto plazo es que el sistema sea capaz de:
- **Cobrar** (flujo de checkout en caja).
- **Controlar Inventario** (descuento de stock en tiempo real).
- **Sincronización Local/Nube** (ya existe la base para la sincronización con supabase y posServer).

El código fuente base es estable y compila.
