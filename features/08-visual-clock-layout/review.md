## Críticas

### 1. Arquitectura / Layout  
**Controles de ventana con ubicación contradictoria**

- El diseño indica que el header contiene título y controles de ventana.
- B4 indica mover `CaptionButtons` al área superior derecha del contenido.
- Debe definirse una sola ubicación.

**Recomendación:** dejar el header para título y settings, y colocar los controles de ventana en un overlay superior derecho del canvas.

### 2. Terminología funcional  
**“Date-control buttons” no coincide con la aplicación**

- La aplicación no tiene botones para controlar fechas.
- Los controles existentes son minimizar, maximizar/restaurar y cerrar.

**Recomendación:** reemplazar todas las referencias por `window controls` o `caption buttons`.

### 3. Arquitectura / MVVM  
**La navegación no define un mecanismo ejecutable**

- Se propone enlazar `Select(MainNavigationPage)`.
- `Select` es un método, no un `ICommand`.
- WinUI no puede enlazar directamente ese método como comando.

**Recomendación:** definir un `RelayCommand<MainNavigationPage>` o comandos individuales para cada botón.

## Altas

### 4. Dependencias / Secuencia  
**Falta declarar dependencia de Window Modes**

La feature modifica:

- `CaptionButtons`.
- `UpdateTitleBarRegions`.
- Regiones de arrastre y passthrough.
- Posiblemente el comportamiento fullscreen.

Sin embargo, solo declara dependencia de las features 02, 03, 06 y 07.

**Recomendación:** añadir dependencia explícita de la feature 05.

### 5. UX / Layout  
**“Mismo espacio” no está garantizado**

- `AlarmView` y `TimerView` usan `StackPanel Width="360"`.
- Compartir una celda de `Grid` no garantiza que el contenido ocupe el mismo ancho y alto que Clock.
- Puede haber desplazamientos o espacios distintos entre vistas.

**Recomendación:** definir un contenedor raíz común con `Stretch`, márgenes, padding y alineación idénticos.

### 6. Arquitectura / XAML  
**`ViewHeader` no tiene implementación definida**

Se menciona un `ViewHeader` reutilizable, pero no se especifica si será:

- `UserControl`.
- `DataTemplate`.
- `ControlTemplate`.
- Estilo compartido.

Tampoco se define cómo funcionará el “optional action slot”.

**Recomendación:** elegir un tipo concreto y documentar cómo recibe el título y las acciones.

### 7. Integración / Vistas duplicadas  
**No está completamente claro qué ocurre con los botones superiores de Alarm y Timer**

El documento dice que esos botones deben desaparecer porque Alarm y Timer serán vistas de navegación, pero los pasos no indican claramente si serán:

- Eliminados.
- Reutilizados como navegación.
- Conservados como accesos alternativos.

**Recomendación:** eliminar explícitamente los botones superiores y dejar la navegación únicamente en el nav rail.

## Medias

### 8. Documentación técnica  
**La jerarquía visual descrita no coincide con la actual**

La especificación habla de `RootGrid` dentro de `NeonGlowBorder`, pero actualmente el contenido de `NeonShell` contiene un `Grid` interno y `RootGrid` pertenece a la ventana.

**Recomendación:** documentar la jerarquía final exacta, por ejemplo:

```text
Window
  NeonShell
    RootGrid
      Header
      Body
        NavRail
        ContentRegion
```

### 9. UX / Estado visual  
**No se define cómo se resaltará la página seleccionada**

Se mencionan `IsClockSelected`, `IsAlarmSelected` e `IsTimerSelected`, pero no se especifica:

- Qué estilo visual usarán.
- Qué tokens existentes se aplicarán.
- Cómo se evitará duplicar estilos.

**Recomendación:** definir un estilo común para botones seleccionados usando únicamente tokens existentes.

### 10. Rendimiento / Estado  
**No se explica el comportamiento de las vistas ocultas**

Las tres vistas podrían existir al mismo tiempo y alternarse mediante `Visibility`.

Debe aclararse que:

- No crean timers propios.
- No ejecutan trabajo periódico cuando están ocultas.
- Mantienen el estado sin duplicar ViewModels.

### 11. Terminología  
**“Clock” y “World Clock” pueden confundirse**

La navegación usa `Clock`, mientras que el ViewModel y la vista actual usan `WorldClock`.

**Recomendación:** definir que el botón visible se llama `Clock`, pero representa la vista `WorldClock`, o usar un único término.

## Bajas

### 12. Requisitos / Trazabilidad  
**Los requisitos declarados no cubren todos los componentes afectados**

La feature declara FR-01 y FR-04, pero modifica visualmente elementos relacionados con:

- FR-40: fullscreen.
- FR-42: salida de fullscreen.
- FR-50: always-on-top.
- El title bar personalizado.

**Recomendación:** mantener esos requisitos como “preservados” o incluirlos como requisitos relacionados, aunque no cambie su lógica.

## Resumen

- **Críticas:** 3
- **Altas:** 4
- **Medias:** 5
- **Bajas:** 1

La feature está **bien orientada**, pero debería aclarar primero la ubicación de los controles, el mecanismo de comandos y las reglas exactas de tamaño de las vistas.