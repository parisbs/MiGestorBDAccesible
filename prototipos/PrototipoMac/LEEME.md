# Prototipo accesible para macOS

Esta prueba desechable permite evaluar MAUI Blazor Hybrid, Mac Catalyst y WKWebView con VoiceOver. No accede a bases de datos, no usa MiGestorBDAccesible.Core, no hace solicitudes de red ni recoge telemetría. No se añade a la solución Windows. Los objetos del explorador y las consultas son ejemplos; las acciones de búsqueda e historial solo anuncian su nombre. Abrir y Guardar sí trabajan con archivos reales que tú eliges.

## Compilar y abrir

Necesitas macOS, Xcode con sus herramientas de línea de comandos, .NET 10 y la carga de trabajo `maui-maccatalyst`. El entorno de referencia tiene un Mac Intel, macOS 26.7.1, Xcode 26.5 y SDK .NET 10.0.403. El proyecto fija `net10.0-maccatalyst26.0` y omite la comprobación exacta de Xcode; eso permite evaluar este entorno, pero no garantiza compatibilidad entre versiones.

Desde la raíz del repositorio:

```sh
cd prototipos/PrototipoMac
dotnet build -c Debug
open "bin/Debug/net10.0-maccatalyst26.0/maccatalyst-x64/Prototipo Mi Gestor BD Accesible.app"
```

En Apple Silicon, la carpeta de ejecución será `maccatalyst-arm64`. También puedes usar `dotnet build -t:Run -c Debug`. Cierra la aplicación al acabar la prueba.

`NuGet.Config` elimina los orígenes remotos: se usan únicamente los paquetes de tu caché local de NuGet. Si falta un paquete, detén la prueba y registra su identificador y versión. No hace falta cambiar el programa para sortear ese fallo. El primer entorno ya tenía una restauración satisfactoria en caché. Los paquetes de ejecución Release para otras arquitecturas podrían faltar.

## Preparar VoiceOver y el teclado

Activa VoiceOver con Command+F5. Usa el rotor para encontrar los encabezados y Tab para recorrer los controles. Si Tab omite botones, activa la navegación de teclado en Ajustes del Sistema, Teclado. Desactiva la navegación rápida de VoiceOver pulsando izquierda y derecha juntas al probar flechas de árbol, menú o pestañas. Interactúa con el contenido web si VoiceOver lo pide. Registra los ajustes que usaste.

Las teclas F pueden necesitar Fn según tu teclado. F10 se refiere a la tecla enviada a la aplicación, no a la función multimedia del sistema. Inicio y Fin pueden ser Fn+izquierda y Fn+derecha. Command también se llama Comando; Mayúsculas es Shift. Option sola no abre la barra.

Al abrir la aplicación, el host solicita el foco de teclado para WKWebView y el primer renderizado enfoca el nodo activo del árbol. No necesitas hacer clic en la página. Al volver a activar la ventana, el host solicita de nuevo el foco nativo sin cambiar el control web activo. Comprueba estas rutas con VoiceOver antes de darlas por válidas.

La barra ofrece Archivo, Editar, Consulta y Ayuda. F1 muestra una ayuda con foco atrapado. F6 cambia entre pestañas y editor; F8 vuelve al árbol. En los diálogos web, Tab y Mayúsculas+Tab quedan dentro del diálogo y Escape lo cancela. Los selectores de archivos usan la navegación estándar del sistema y deben devolver el foco al control de origen.

## Qué probar y registrar

Ninguna prueba manual se considera aprobada por compilar. Para cada criterio anota resultado, pasos exactos, frase que dijo VoiceOver, foco antes y después, versión del sistema y cualquier diferencia entre tecla y botón. El registro visible al final de la página conserva las últimas 50 acciones; puedes consultarlo con VoiceOver. Los anuncios tienen regiones polite y assertive separadas.

1. **AC-1, explorador.** Al arrancar, comprueba que el foco está en Servidor de prueba sin hacer clic. Prueba directamente flechas, Tab, F8 y F10. Cambia a otra aplicación y vuelve; repite las teclas desde el árbol y desde el editor. Debe haber una sola entrada por Tab. Recorre arriba/abajo y prueba Inicio/Fin en ambos extremos. Expande Servidor de prueba, Bases de datos, Tienda, Tablas y Clientes para llegar a las hojas Id y Nombre; prueba también Archivo y Vistas. Derecha expande y luego entra en el primer hijo; izquierda contrae y luego va al padre. Comprueba que las flechas no sacan el foco del árbol y que VoiceOver comunica nombre, nivel, posición entre hermanos, selección y expansión. Intro alterna una rama o anuncia una hoja. El marcado usa tree, treeitem, group, aria-level, aria-setsize, aria-posinset y tabindex móvil.
2. **AC-2, barra.** Coloca el foco en el editor y pulsa F10. Izquierda/derecha cambian el menú; abajo, Intro o Espacio abren sus opciones. Arriba/abajo recorren opciones; Inicio/Fin van a sus extremos. Escape cierra primero el submenú y otro Escape vuelve al editor. Prueba también F10 otra vez y Tab. Cada opción muestra el atajo en texto y lo expone con aria-keyshortcuts; comprueba cómo VoiceOver lo anuncia. El patrón es menubar/menu/menuitem. Elegir una opción devuelve el foco al contenido antes de ejecutar.
3. **AC-3, pestañas.** Hay tres consultas iniciales. Prueba Control+Tab y Control+Mayúsculas+Tab desde árbol y editor, incluyendo la vuelta del extremo al principio. El foco pasa a la pestaña activa y el anunciador dice su nombre y posición. En el tablist, izquierda/derecha, Inicio/Fin y clic cambian la pestaña. Comprueba aria-selected, posición y relación con el tabpanel. Nueva consulta y Nueva pestaña añaden una pestaña; Cerrar pestaña conserva al menos una para continuar las pruebas. El texto de cada pestaña se conserva al cambiar.
4. **AC-4, anuncios.** Activa Emitir anuncio normal, Emitir anuncio urgente y Emitir el mismo anuncio dos veces. La última acción emite dos mensajes iguales separados por 1,5 segundos. Cada emisión vacía ambas regiones, espera 150 ms y escribe en la región correspondiente. Comprueba que escuchas ambas repeticiones y que el urgente interrumpe según el comportamiento de VoiceOver. Repite varias veces.
5. **AC-5, atajos.** Prueba cada combinación de la tabla desde el árbol y desde el editor. Debes escuchar su acción y verla en el registro. Guardar y Abrir presentan selectores nativos; puedes cancelarlos para seguir. Verifica que el sistema no crea otra ventana ni captura Guardar, Buscar o Historial. Prueba Option sola y escribe un carácter con Option: no debe abrir el menú. Command+H conserva la función del sistema para ocultar la aplicación. Command+Q pasa por la confirmación descrita abajo. Las opciones nativas Archivo, Buscar y Formato (incluidos Fuente y Texto) se retiran en AppDelegate.BuildMenu para liberar también Command+B/I/U/T; se eliminan también atajos conflictivos de Editar, Ver, Ventana y Ayuda, conservando sus otras funciones. Salir se reemplaza por una orden nativa propia. Comprueba que Formato ya no aparece y que Command+B/I/U/T no activa órdenes nativas de formato; Command+I/U queda libre, sin una acción propia en el prototipo.

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
| Barra de menú | F10 |
| Salir | Command+Q |

6. **AC-6, cierre.** Marca Simular cambios pendientes. Prueba por separado Cerrar ventana con comprobación, Salir y Command+Q. Debe aparecer un diálogo con Guardar, No guardar y Cancelar; el foco inicial está en Cancelar. Cancelar o Escape mantiene la aplicación abierta y devuelve el foco. Guardar exporta el texto de la pestaña activa con el selector del sistema; cancelar ese selector o un error mantiene abierta la aplicación y la confirmación. No guardar registra el descarte simulado. Tras guardar o descartar se anuncia el cierre y se pide a MAUI cerrar la ventana o salir. Sin la casilla, esas rutas cierran directamente. Reabre la aplicación entre intentos. **Limitación conocida: el botón rojo no se puede vetar con las API públicas de UIKit inspeccionadas y cierra sin confirmación.** Command+W pertenece al contenido y cierra pestañas; la opción nativa Cerrar ventana se retira para evitar conflicto. No se implementan ganchos privados de AppKit ni se suplanta el delegado de NSWindow. La salida forzada, el cierre de sesión y otras terminaciones del sistema tampoco se interceptan. Esta limitación impide considerar AC-6 completo.
7. **AC-7, archivos.** Escribe una frase en el editor. Usa Guardar como, elige un nombre distinto a Consulta.txt y una carpeta local y confirma. Repite desde el menú y con Command+Mayúsculas+S. Abre ese archivo con Abrir archivo o Command+O; comprueba el nombre anunciado, el texto anunciado completo y el contenido del editor. Prueba Cancelar en ambos selectores y verifica que recuperas el foco. Guardar también solicita nombre y carpeta: el prototipo no conserva permisos ni una ruta de guardado entre llamadas. El selector de apertura admite archivos de datos; elige texto UTF-8 para esta prueba. Son UIDocumentPickerViewController de apertura y exportación, con acceso de seguridad delimitado; no son inputs HTML. Prueba también una ruta con espacios y caracteres acentuados. No elijas destinos de nube si necesitas garantizar una prueba completamente local.
8. **AC-8, paquete.** Ejecuta el script de abajo. Comprueba firma y existencia del zip, descomprímelo en otra carpeta y repite el arranque y los criterios anteriores en la aplicación extraída. Registra arquitectura y macOS del equipo receptor. Esta firma ad hoc no es una firma de Developer ID ni incluye notarización; la política de Gatekeeper del receptor puede impedir abrir un zip descargado. La prueba no modifica esa política.

## Problemas conocidos

El 3 de noviembre de 2026, AXPress (la acción que usa VoiceOver con VO+Espacio) hizo caer WebContent dos veces al pulsar un grupo y un texto del árbol. Los informes situaron el fallo en `_synthesizeMouseClick:` de `AXPMacPlatformElement`, dentro de la traducción de accesibilidad de Mac Catalyst de Apple, con una aserción de Swift y SIGILL. El host ahora registra la terminación en la consola local y solicita recargar la página para reiniciar Blazor, en lugar de dejar la ventana en blanco. El primer renderizado vuelve a enfocar el árbol. La recarga pierde el texto que no hayas guardado y reinicia las pestañas; no corrige el fallo de Apple. Falta validar con VoiceOver que, tras una caída, reaparece el contenido y funcionan Tab, F8 y F10 sin hacer clic.

## Empaquetar

```sh
cd prototipos/PrototipoMac
./empaquetar.sh
```

Ejecuta los comandos desde la raíz del repositorio o, si ya estás en esta carpeta, ejecuta solo `./empaquetar.sh`. El script compila Release con las arquitecturas predeterminadas de Mac Catalyst y sin firma de desarrollador, localiza la `.app`, la firma con `codesign -s -`, comprueba la firma y usa `ditto -c -k --keepParent` para crear `salida/PrototipoMac-fecha-hora.zip`. Se puede subir ese archivo a GitHub manualmente. El script no sube archivos ni usa GitHub Actions. No requiere cuenta de desarrollador.

Si la compilación Release necesita paquetes no disponibles en caché, falla sin conectarse a NuGet y no genera un paquete nuevo. No distribuyas la compilación Debug como si fuera Release. Si hay más de una `.app` en las carpetas Release, el script pide revisar los resultados en lugar de escoger una arbitrariamente.

## Procedencia y límites

El archivo `MiGestorBDAccesible/wwwroot/js/app.js` se enlaza desde el proyecto original, se carga desde index.html y no se modifica. `wwwroot/js/prototipo-mac.js` adapta Command y evita el disparo de Alt sola desde captura sin cancelar su escritura predeterminada. Los componentes registran las referencias .NET y los métodos JSInvokable usados por el script real.

Explorador reproduce el marcado recursivo y el recorrido por nodos visibles de Explorador.razor. BarraMenus, Anunciador y AnunciadorService parten de los originales; la barra añade los atajos macOS y aria-keyshortcuts. Las pestañas siguen EspacioTrabajo.razor e incorporan posición y tamaño del conjunto. No se copian lógica de bases de datos ni componentes Windows.

La limitación del botón rojo se basa en los encabezados de UIKit del Xcode instalado: `UIScene.h` declara `sceneDidDisconnect:` sin resultado ni respuesta de cancelación; `UIApplication.h` ofrece `requestSceneSessionDestruction:options:errorHandler:` para solicitar el cierre y `applicationWillTerminate:` como notificación, sin una consulta equivalente a `windowShouldClose:`/`applicationShouldTerminate:`. `UIWindowScene.h` tampoco proporciona ese veto. Esto describe las API inspeccionadas, no una prueba manual de todas las rutas de macOS. Valida la decisión de host antes de portar la aplicación real.

Puedes comprobar las rutas del script sin instalar dependencias adicionales, si tienes Node.js:

```sh
node verificar-teclado.cjs
```

Esta comprobación carga el app.js original y la adaptación en un entorno simulado. Verifica el despacho de atajos; no comprueba VoiceOver, WKWebView, los menús de macOS ni los selectores nativos.

## Verificación automatizada de la base

La comprobación `node verificar-teclado.cjs` pasó sus 16 rutas de teclado, el bloqueo de acciones dentro de diálogos y Option sin capturar. También pasaron `bash -n empaquetar.sh`, la validación del plist de permisos y `git diff --check`. Estas comprobaciones no aprueban ningún criterio manual con VoiceOver.

En el entorno restringido de ejecución, `dotnet build -c Debug` generó el ensamblado de la aplicación, pero la fase posterior falló con MSB4216 al crear o conectar el host de tareas NET/x64 para `ComputeManagedAssemblies`, seguido de MSB4027. Una prueba independiente de socket Unix local devolvió `Operation not permitted`, coherente con el bloqueo de IPC de este entorno. No se considera una compilación Debug completa ni se ha abierto la aplicación.

La restauración Release se comprobó por separado, usando una carpeta temporal para no cambiar los resultados Debug. Falló con NU1101 porque falta `Microsoft.MacCatalyst.Runtime.maccatalyst-arm64.net10.0_26.0` en la caché. Por ello no se ejecutó el empaquetador ni se generó un zip de distribución.

## Verificación de la corrección del host

Se han contrastado las firmas con los ensamblados y la documentación XML locales de MAUI 10.0.110 y Mac Catalyst 26.0, y se ha inspeccionado el orden de inicialización de Blazor y sus callbacks de navegación. El encadenamiento conserva los siete callbacks de su delegado en MAUI 10.0.110; revísalo si cambias de versión. El binding de `webViewWebContentProcessDidTerminate:` se llama `ContentProcessDidTerminate`. Han pasado de nuevo las 16 rutas de `node verificar-teclado.cjs`, la comprobación de XML y controladores de eventos del XAML y `git diff --check`. El SHA-256 de `app.js` coincide con HEAD. Esto no es una compilación ni una prueba de ejecución del host. No se ejecuta MSBuild en este entorno porque su IPC local está bloqueado. Ejecuta `dotnet build -c Debug` fuera del entorno restringido y valida manualmente arranque, reactivación de ventana, menús, Command+Q y recuperación del proceso con VoiceOver.
