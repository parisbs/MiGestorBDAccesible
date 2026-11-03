# Mi Gestor BD Accesible

Gestor de bases de datos de escritorio para SQL Server, Oracle y PostgreSQL,
hecho para usarse por completo con el teclado y un lector de pantalla (NVDA en
Windows).

## Comandos

- Verify: `dotnet build MiGestorBDAccesible.sln && dotnet test MiGestorBDAccesible.sln --no-build`
- La aplicación de escritorio necesita Windows y el SDK de .NET 10
  (net10.0-windows, WPF). El núcleo, las pruebas y las herramientas son
  net10.0 y compilan en cualquier sistema. Fuera de Windows, Verify completo no
  se puede ejecutar: se compilan y prueban `MiGestorBDAccesible.Pruebas` y
  `MiGestorBDAccesible.Herramientas` como el trabajo de CI `Compilar y probar
  en macOS`, el resto se informa como no ejecutado y la referencia es el
  trabajo `Compilar y probar`, que ejecuta Verify en `windows-latest`.
- `crear-instalador.ps1` genera el instalador NSIS y el manual en HTML en
  `salida/`; necesita NSIS (`winget install NSIS.NSIS`).

## Invariantes

- La accesibilidad es el producto. Todo cambio visible se tiene que poder usar
  por completo con el teclado y el lector de pantalla tiene que anunciarlo
  bien, con los patrones ARIA estándar que ya se usan (árbol, barra de menús,
  menú, pestañas, cuadro de lista, diálogo con el foco atrapado). El autor
  aprueba a mano con NVDA cada función, tecla y mensaje; las pruebas
  automáticas no lo demuestran, así que un cambio visible no está terminado
  hasta que se comprueba con un lector de pantalla, y el pull request dice si
  se hizo.
- Los anuncios pasajeros van por `AnunciadorService`. El componente
  `Anunciador` vacía su región en vivo y escribe el mensaje tras una breve
  espera para que un mensaje repetido se vuelva a leer.
- Los cambios en la base de datos muestran antes el SQL y piden confirmación:
  los proveedores generan scripts (métodos `Script...`) que la interfaz solo
  ejecuta cuando el usuario confirma. La edición de filas es la excepción
  deliberada: corre en su propia transacción, que el usuario confirma o
  deshace, y al cerrar la ventana se le pregunta qué hacer. No se añaden
  caminos que cambien datos sin una de estas dos protecciones.
- La misma acción tiene el mismo nombre, la misma entrada de menú y la misma
  tecla en todos los motores. Las diferencias entre motores van por
  `CapacidadesMotorModelo` y `TerminosMotorModelo` de cada proveedor, nunca
  por comprobaciones del motor en la interfaz.
- Sin uso de red ni telemetría, salvo una excepción explícita del autor desde
  la versión 1.1: `ActualizacionesService` consulta en GitHub si hay una
  versión nueva (como mucho una vez al día, desactivable desde «Buscar
  actualizaciones al iniciar») y descarga el instalador cuando el usuario lo
  pide. No se añade otra salida a la red ni se envían datos del usuario. Las
  conexiones, el historial y las pestañas se
  quedan en la carpeta de datos de la aplicación del usuario
  (`%APPDATA%\MiGestorBDAccesible` en Windows). Las contraseñas se cifran
  para el usuario actual: con DPAPI en Windows y con Data Protection, con las
  claves en la carpeta `claves` junto a `conexiones.json`, en los demás
  sistemas.
- `MiGestorBDAccesible.Core` no depende de WPF ni de Blazor; la lógica que se
  puede probar va ahí, no en los componentes Razor.
- Licencia: GPL 3 o posterior con una excepción de la sección 7 que solo
  cubre el controlador de Oracle (`EXCEPCION-ORACLE.md`). No se añade otra
  dependencia que no sea libre sin que el autor conceda una excepción
  equivalente.

## Reglas del stack

- C# con .NET 10, tipos de referencia anulables y usings implícitos. La
  interfaz es Blazor Hybrid: componentes Razor en
  `MiGestorBDAccesible/Componentes` dentro de un `BlazorWebView` de WPF
  (WebView2), más `wwwroot/js/app.js` para el foco y el teclado.
- Cada motor es una implementación de `IProveedorBaseDatos` dividida en clases
  parciales por tema; una función que toca los proveedores se implementa en
  los tres motores o se marca como no disponible mediante las capacidades.
- Las pruebas son xUnit en `MiGestorBDAccesible.Pruebas`, solo unitarias sobre
  el núcleo; no hay pruebas de integración contra servidores reales.
- Los archivos `.cs` se guardan con LF (`.gitattributes`): varias pruebas
  comparan scripts generados con cadenas literales de varias líneas.
- `MANUAL.md` va incrustado en el ensamblado del núcleo y
  `MiGestorBDAccesible.Herramientas` lo exporta como manual HTML; la ayuda de
  F1 sale de los `fichas.json` de cada motor en `MiGestorBDAccesible.Core/Ayuda/`.

## Convenciones

- Idioma: todo en español: identificadores, comentarios, textos de la
  interfaz, documentación, este archivo, issues, commits y pull requests. Los
  textos de la interfaz tutean al usuario. Se mantienen en inglés solo los
  términos estándar, como los tipos de Conventional Commits y los
  identificadores `AC-n`.
- Git: nombres de rama de gitflow (`feature/`, `fix/`, `hotfix/`, `release/`,
  `chore/`, `docs/`, `refactor/`, `test/`) con la descripción en español y sin
  tildes; las versiones son etiquetas anotadas `vX.Y.Z`.
- Commits y títulos de pull request: Conventional Commits en una línea, con
  el tipo estándar y la descripción en español en infinitivo, por ejemplo
  `feat(explorador): agregar búsqueda por esquema`. El ámbito va sin tildes ni
  eñes.
- Un cambio de comportamiento visible o de una tecla actualiza `MANUAL.md` en
  el mismo cambio, y las fichas de F1 cuando toca la ayuda de SQL.
- Los diagramas de la documentación usan Mermaid y llevan una breve
  descripción en prosa.

## Flujo de trabajo

- Los issues llevan criterios de aceptación como `CUANDO ... ENTONCES ...`,
  numerados `AC-n`. Las pruebas que demuestran un criterio llevan su
  identificador en el nombre. Los criterios que solo se pueden comprobar con
  un lector de pantalla se marcan `(manual, NVDA)`.
- Human merge: `**/*.csproj`, `*.sln`, `**/Directory.Build.props`, `instalador/*.nsi`, `*.ps1`
