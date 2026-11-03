# Prototipo accesible para macOS con AppKit

Esta variante permite evaluar MAUI Blazor Hybrid en una ventana AppKit con WKWebView y VoiceOver. Los objetos del árbol, las consultas y los resultados son ejemplos. No conecta con bases de datos, no hace solicitudes de red y no recoge telemetría. Abrir y Guardar sí leen y escriben archivos reales que tú eliges. Usa archivos de texto UTF-8 y destinos locales para las pruebas.

Existe porque, en las inspecciones de accesibilidad del 3 de noviembre de 2026, Mac Catalyst expuso los treeitem y menuitem del prototipo como AXStaticText. El esqueleto AppKit expuso el árbol como AXOutline, sus elementos como AXRow/AXOutlineRow y los elementos del menú como AXMenuItem. El usuario pudo leer título, árbol y menú con VoiceOver en ese esqueleto. Esas observaciones no aprueban la operabilidad de esta interfaz completa. El prototipo Catalyst de `../PrototipoMac` queda como evidencia, sin cambios.

El backend procede de `dotnet/maui-labs/platforms/MacOS`. Los paquetes `Microsoft.Maui.Platforms.MacOS`, `.Essentials` y `.BlazorWebView` están fijados a `0.1.0-preview.14.26531.2`: son una implementación experimental de labs, no una garantía de soporte del backend macOS en MAUI estable. Este proyecto no se añade a la solución Windows.

## Compilar y abrir

Necesitas macOS, Xcode con sus herramientas de línea de comandos, .NET 10 y la carga de trabajo `macos`. El entorno de referencia es un Mac Intel con macOS 26.7.1, Xcode 26.5 y SDK .NET 10.0.403. El proyecto fija `net10.0-macos26.0` y `ValidateXcodeVersion=false`; esa excepción permite evaluar este entorno, sin garantizar compatibilidad con otras versiones.

Desde la raíz del repositorio, en Intel:

```sh
cd prototipos/PrototipoMacAppKit
dotnet build -c Debug -r osx-x64
open "bin/Debug/net10.0-macos26.0/osx-x64/Prototipo AppKit Mi Gestor BD Accesible.app"
```

En Apple Silicon cambia `osx-x64` por `osx-arm64` en ambos comandos. También puedes usar `dotnet build -t:Run -c Debug -r osx-x64`. No abras simultáneamente la variante Catalyst si vas a registrar los resultados del prototipo AppKit.

La restauración necesita los paquetes fijados y los paquetes de ejecución correspondientes en tu caché de NuGet. No hay nuevos paquetes ni cambios de orígenes. En un entorno sin red prepara la restauración con la caché local; si falta un paquete, registra su identificador y versión y detén esa comprobación. Cuando ya exista `obj/project.assets.json` para la configuración y las arquitecturas elegidas, añade `--no-restore` a la compilación para evitar consultas de restauración. Un worktree nuevo no incluye `obj` ni los resultados de otra copia.

Para ver el diagnóstico nativo, puedes ejecutar directamente el binario del paquete desde Terminal, en vez de usar `open`:

```sh
"bin/Debug/net10.0-macos26.0/osx-x64/Prototipo AppKit Mi Gestor BD Accesible.app/Contents/MacOS/PrototipoMacAppKit"
```

La salida local muestra el contenido de `NSApplication.SharedApplication.MainMenu` después del arranque: ruta, tecla, modificadores y selector de cada opción. Las líneas `Atajo liberado para la página` identifican los conflictos neutralizados. Guarda ese registro al evaluar AC-5; la inspección del ensamblado no sustituye la inspección del menú en ejecución.

## Preparar VoiceOver y el teclado

Activa VoiceOver con Command+F5. Command también se llama Comando; Mayúsculas es Shift y Option es Alt. Si las teclas F activan funciones del sistema, añade Fn. Activa la navegación de teclado en Ajustes del Sistema, Teclado si Tab omite botones. Desactiva la navegación rápida de VoiceOver pulsando izquierda y derecha juntas para enviar las flechas a los controles. Interactúa con el contenido web si VoiceOver lo pide. Registra los ajustes usados.

Al arrancar, el host pide `NSWindow.MakeFirstResponder` para WKWebView; el primer renderizado del árbol enfoca Servidor de prueba. Al volver a activar la ventana se pide otra vez el foco nativo sin cambiar el control web activo. El regreso de un selector de archivos también recupera el foco nativo y el foco web. Estas rutas deben comprobarse sin hacer clic en la página.

F10 activa la barra web; Option sola permite escribir caracteres y no abre el menú. F8 vuelve al árbol, F6 alterna pestañas y editor, y F1 abre la ayuda. Los diálogos web usan `role=dialog`, `aria-modal`, contenido de fondo inerte y foco atrapado con Tab y Mayúsculas+Tab. Escape cancela. Los selectores de archivos son hojas nativas de AppKit y usan la navegación del sistema.

## Qué probar por criterio

Todos los criterios siguientes están pendientes de validación manual en esta versión. No los des por aprobados por compilar ni por superar la comprobación de JavaScript. Para cada uno registra pasos, resultado, frase exacta de VoiceOver, foco antes y después, arquitectura, macOS y diferencias entre teclado y activación con VO+Espacio. La página conserva las últimas 50 acciones en Registro de acciones.

1. **AC-1, explorador.** Arranca y prueba flechas, Tab, F8 y F10 sin hacer clic. Cambia a otra aplicación y vuelve, tanto desde el árbol como desde el editor. Debe haber una sola entrada del árbol por Tab. Arriba/abajo recorren los nodos visibles; Inicio/Fin van a sus extremos. Derecha expande y después entra en el primer hijo; izquierda contrae y después vuelve al padre. Expande Servidor de prueba, Bases de datos, Tienda, Tablas y Clientes hasta las hojas Id y Nombre; prueba también Archivo y Vistas. Intro alterna ramas o anuncia una hoja. Comprueba nombre, nivel, posición, selección y expansión. Comprueba también el árbol de accesibilidad de esta interfaz completa: `tree`, `treeitem` y `group` deben conservar la exposición nativa observada en el esqueleto. Prueba VO+Espacio y registra cualquier caída; todavía no se ha establecido si el fallo de AXPress de Catalyst afecta a AppKit.
2. **AC-2, menús.** Desde el editor pulsa F10. Izquierda/derecha cambian de menú. Abajo, Intro y Espacio abren opciones; arriba/abajo, Inicio y Fin las recorren. Escape cierra primero el submenú y luego devuelve el foco; prueba también F10 otra vez y Tab. Comprueba `menubar`, `menu`, `menuitem`, expansión, opciones no disponibles y atajos anunciados. Elegir una opción debe devolver el foco al contenido y ejecutar su acción. Prueba las acciones con VO+Espacio además del teclado.
3. **AC-3, pestañas.** Desde árbol y editor prueba Control+Tab y Control+Mayúsculas+Tab, incluida la vuelta entre extremos. El foco y el anuncio deben identificar la pestaña activa y su posición. En la lista de pestañas prueba izquierda/derecha, Inicio y Fin. Comprueba selección y relación con el panel de consulta. Nueva consulta y Nueva pestaña añaden consultas; Cerrar pestaña conserva al menos una. Escribe textos distintos y comprueba que se conservan al cambiar.
4. **AC-4, anuncios.** Activa Emitir anuncio normal, Emitir anuncio urgente y Emitir el mismo anuncio dos veces. Cada emisión vacía las dos regiones, espera 150 ms y escribe en la región normal o urgente. El botón de repetición emite el mismo texto dos veces con 1,5 segundos de separación. Comprueba que escuchas las dos emisiones y registra el comportamiento del urgente. Los avisos transitorios pasan por `AnunciadorService` y su componente.
5. **AC-5, atajos.** Prueba todas las combinaciones de la tabla desde árbol y editor. Debes escuchar y ver la acción en el registro; puedes cancelar los selectores para continuar. Repite con un diálogo web abierto: los atajos de trabajo no deben ejecutar acciones de fondo. Option sola no activa el menú; prueba escribir un carácter con Option. Command+H sigue ocultando la aplicación. Revisa también el registro nativo del menú y confirma que ningún atajo de la tabla termina en una opción nativa distinta. El código revisa el menú después de `base.DidFinishLaunching` y al recuperar la ventana; vacía los equivalentes conflictivos, sin retirar las funciones de edición estándar. Command+Q conserva la orden nativa de salir, que consulta el delegado de aplicación antes de terminar.

| Acción | Atajo macOS |
| --- | --- |
| Nueva consulta | Command+N |
| Nueva pestaña | Command+T |
| Cerrar pestaña | Command+W |
| Guardar | Command+S |
| Guardar como | Command+Mayúsculas+S |
| Abrir archivo | Command+O |
| Buscar y reemplazar | Command+F |
| Buscar objeto | Command+B |
| Historial | Command+Y |
| Buscar siguiente | Command+G o F3 |
| Ejecutar | Command+Return o F5 |
| Cambiar pestaña | Control+Tab / Control+Mayúsculas+Tab |
| Barra web | F10 |
| Salir | Command+Q |

6. **AC-6, cambios pendientes y cierre.** Marca Simular cambios pendientes: es una simulación explícita, no una detección automática de edición. Prueba por separado el botón rojo de la ventana, Cerrar ventana con comprobación, Salir, Command+Q y la opción de salir del menú nativo. Debe aparecer Guardar / No guardar / Cancelar, con foco inicial en Cancelar. Tab y Mayúsculas+Tab deben permanecer dentro; Escape y Cancelar mantienen abierta la app y devuelven el foco. Guardar abre NSSavePanel para el texto de la pestaña activa; cancelar ese panel o un error mantiene abierta la confirmación. Tras guardar o descartar, debe anunciarse la acción y cerrarse la ventana o terminar la app. Reabre la app entre intentos. Sin cambios pendientes esas rutas cierran directamente, tras anunciarlo. Command+W siempre cierra una pestaña, nunca la ventana. Al cerrar la última ventana, el proceso puede seguir activo conforme al comportamiento del backend; Command+Q permite salir sin una página abierta. La salida forzada no permite este diálogo.
7. **AC-7, archivos nativos.** Escribe una frase y usa Guardar como con botón, menú y Command+Mayúsculas+S. Elige nombre y carpeta, incluyendo espacios y acentos. Abre el archivo con botón, menú y Command+O. Comprueba nombre y contenido anunciados, texto del editor y contenido real del archivo. Prueba Cancelar y recuperación del foco en ambos paneles, además de un error de lectura o escritura. Se usan `NSSavePanel` y `NSOpenPanel` como hojas de la ventana, con lectura y escritura UTF-8. Guardar también pide nombre y carpeta; no se conserva una ruta entre llamadas. No se habilita App Sandbox ni se requieren permisos de red.
8. **AC-8, distribución.** Prepara la restauración Release para `osx-x64` y `osx-arm64` con los paquetes locales, ejecuta el empaquetador y conserva su salida. Debe verificar ambas arquitecturas, firmar ad hoc, verificar la firma y crear un zip en `salida/`. Extrae el zip en otra carpeta y repite arranque, cierre, selectores y criterios anteriores. Registra arquitectura y macOS del receptor. La firma ad hoc no incluye Developer ID ni notarización; la política de Gatekeeper del receptor puede impedir abrir un zip descargado.

Buscar, historial y ejecución solo anuncian acciones de prueba. No realizan operaciones SQL.

## Empaquetar

Desde esta carpeta, tras preparar los activos de restauración Release para las dos arquitecturas:

```sh
./empaquetar.sh
```

El script ejecuta `dotnet build -c Release`, que restaura las dos arquitecturas, sin firma de desarrollador ni creación de instalador. Espera la aplicación universal en `bin/Release/net10.0-macos26.0/Prototipo AppKit Mi Gestor BD Accesible.app`, lee el nombre de su ejecutable del Info.plist y comprueba `x86_64` y `arm64` con `lipo`. Si solo hay una arquitectura o faltan activos de restauración, falla antes de empaquetar.

Firma con `codesign --force --deep -s - --entitlements Entitlements.plist`, verifica con `codesign --verify --deep --strict` y comprime con `ditto -c -k --sequesterRsrc --keepParent`. `Entitlements.plist` contiene un diccionario vacío: no solicita red ni App Sandbox. El zip queda en `salida/PrototipoMacAppKit-fecha-hora.zip`; `salida/`, `bin/` y `obj/` están ignorados por git. No sube archivos ni usa GitHub Actions. No necesitas una cuenta de desarrollador para esa firma.

## Recuperación de WebContent

El controlador conserva el delegado de navegación que encuentre y añade el callback `webViewWebContentProcessDidTerminate:`. Cuando termina WebContent registra el suceso en la consola local y solicita recargar la página, conservando el gestor y el canal Blazor del backend. El árbol vuelve a solicitar foco en su primer renderizado. Se reinician las pestañas y se pierde el texto que no hayas guardado: es recuperación visual y de teclado, no recuperación de documentos.

Para comprobarlo, identifica en Monitor de Actividad el proceso WebContent de esta aplicación y termina ese proceso. Verifica que reaparece la interfaz y funcionan Tab, flechas, F8, F10 y Command sin clic. Comprueba que el registro nativo muestra la terminación. La existencia del callback no demuestra que esta recuperación funcione; queda pendiente de prueba en ejecución con VoiceOver.

## Integración y comprobaciones

`MainPage` aloja `Components.Routes` en `MacOSBlazorWebView`. La interfaz, estilos y adaptación Command proceden del prototipo Catalyst. `MiGestorBDAccesible/wwwroot/js/app.js` sigue enlazado como BundleResource, se carga desde index.html y permanece sin modificar. `Routes` no usa `FocusOnNavigate` para evitar que su enfoque del encabezado desplace el foco inicial del árbol.

La inspección estática de los ensamblados restaurados de preview.14 mostró que `MacOSMauiApplication.CreatePlatformWindow` instancia directamente `WindowHandler`. Por eso se envuelve el delegado de la ventana real después del arranque del backend. Su delegado `MacOSWindowDelegate` implementa `WindowShouldClose` y `WillClose`; se conservan ambos y se restaura el original antes de notificar el cierre para mantener su limpieza. La aplicación deriva de `MacOSMauiApplication`, llama a su arranque y conserva sus callbacks de ciclo de vida; añade `ApplicationShouldTerminate` para vetar la salida hasta confirmar en la página. La autorización de salida evita una segunda confirmación al llamar a `Terminate`.

El cierre confirmado y los paneles se despachan al hilo principal mediante `NSApplication.BeginInvokeOnMainThread`. No dependen del `MainThread` estático de MAUI: los paquetes AppKit inspeccionados no registran su implementación personalizada.

`BlazorWebViewHandler` se deriva para conectar y liberar el controlador de WKWebView. Este backend inspeccionado no instala un `WKNavigationDelegate`; el proxy conserva cualquier delegado previo y reenvía sus selectores opcionales. El foco se solicita al montar la vista, al terminar la navegación, al recuperar la ventana y al regresar de un panel. Revisa estos puntos si cambias el backend.

Puedes ejecutar las comprobaciones sin red ni dependencias adicionales, si tienes Node.js:

```sh
node verificar-teclado.cjs
bash -n empaquetar.sh
plutil -lint Entitlements.plist
```

La comprobación JavaScript carga el app.js real y la adaptación Command: verifica 16 rutas, bloqueo por diálogo y Option sin capturar. No prueba WKWebView, menús nativos, selectores, cierre ni VoiceOver.

Durante este port se comprobaron C# y Razor con el compilador Roslyn y el generador Razor del SDK, usando referencias locales, sin MSBuild. No se ejecutaron el registrador de Objective-C, el enlazador, la compilación de la `.app`, la firma ni el empaquetador. MSBuild tiene bloqueado el IPC local en el entorno restringido de trabajo. La siguiente comprobación del orquestador es `dotnet build -c Debug` fuera de ese entorno; después hacen falta las pruebas manuales anteriores. No hay criterios manuales aprobados por este port.
