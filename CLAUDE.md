# Mi Gestor BD Accesible

Gestor de bases de datos de escritorio para SQL Server, Oracle y PostgreSQL,
hecho para usarse por completo con el teclado y un lector de pantalla (NVDA en
Windows). Este checkout es el fork `parisbs/MiGestorBDAccesible` de
`WilmerRv1989/MiGestorBDAccesible`. El fork existe solo para contribuir: el
producto y sus decisiones son del autor original.

## Comandos

- Verify: `dotnet build MiGestorBDAccesible.sln && dotnet test MiGestorBDAccesible.sln --no-build`
- La compilación necesita Windows y el SDK de .NET 10 (net10.0-windows, WPF).
  En otra plataforma Verify no se puede ejecutar: se informa como no
  ejecutado y la referencia es el trabajo de CI `Compilar y probar`, que
  ejecuta lo mismo en `windows-latest`.
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
- Sin uso de red ni telemetría. Las conexiones, el historial y las pestañas se
  quedan en `%APPDATA%\MiGestorBDAccesible`, y las contraseñas se cifran con
  DPAPI para el usuario actual de Windows.
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
  el núcleo; no hay pruebas de integración contra servidores reales. El guard
  de ATDD lee las rutas de pruebas de `.atdd/test-paths`.
- `MANUAL.md` va incrustado en el ensamblado del núcleo y
  `MiGestorBDAccesible.Herramientas` lo exporta como manual HTML; la ayuda de
  F1 sale de los `fichas.json` de cada motor en `MiGestorBDAccesible.Core/Ayuda/`.

## Convenciones

- Idioma: todo en español, como el proyecto original: identificadores,
  comentarios, textos de la interfaz, documentación, este archivo, ADRs,
  issues, commits y pull requests. Los textos de la interfaz tutean al
  usuario. Se mantienen en inglés solo los términos estándar: los tipos de
  Conventional Commits, los identificadores `AC-n` y los valores de `status`
  de los ADRs.
- Git: nombres de rama de gitflow (`feature/`, `fix/`, `hotfix/`, `release/`,
  `chore/`, `docs/`, `refactor/`, `test/`) con la descripción en español y sin
  tildes; merge con squash; las versiones son etiquetas anotadas `vX.Y.Z`.
- Commits y títulos de pull request: Conventional Commits en una línea, con
  el tipo estándar y la descripción en español en infinitivo, por ejemplo
  `feat(explorador): agregar búsqueda por esquema`. El ámbito va sin tildes ni
  eñes, porque la comprobación de títulos solo admite `[a-z0-9._/-]`.
- Un cambio de comportamiento visible o de una tecla actualiza `MANUAL.md` en
  el mismo cambio, y las fichas de F1 cuando toca la ayuda de SQL.
- Los diagramas de la documentación usan Mermaid y llevan una breve
  descripción en prosa.
- Las decisiones que cierran una alternativa se registran en `docs/adr/`
  (formato MADR 4; las reglas están en su `README.md`).

## Fork y repositorio original

- Remotos: `origin` es el fork y `upstream` el original; `remote.pushDefault`
  es `origin`. Nunca se hace push a `upstream`.
- `main` es un espejo exacto de `upstream/main`: solo avanza por fast-forward
  (Sync fork o `gh repo sync`) y nunca lleva commits propios.
- `workflow` es la rama predeterminada del fork: `main` más los commits del
  flujo de trabajo (este archivo, `.atdd/`, `docs/adr/`,
  `scripts/check-adrs.mjs`, las comprobaciones de título y de ADRs). Cuando
  `main` avanza, `workflow` se rebasa sobre `main`; ese force push lo hace o
  lo aprueba el humano.
- Las ramas de trabajo salen de `workflow`, para tener estas instrucciones.
  Antes del pull request al original, la rama se pasa a `main` con
  `git rebase --onto main workflow <rama>` y se comprueba con
  `git diff main --stat` que no lleva ningún archivo del flujo. El pull
  request apunta a `WilmerRv1989/MiGestorBDAccesible` y dice qué
  comprobaciones se ejecutaron; la evidencia del oráculo de ATDD va en su
  descripción, porque el rebase reescribe los commits a los que apunta la
  etiqueta.
- Lo que se propone al original y ya está en `workflow` (el CI de compilación
  y las plantillas de issues y pull requests) va en sus propios commits,
  debajo de los del flujo, para que el rebase los descarte cuando el original
  los incorpore.

## Flujo de trabajo

- Los issues llevan criterios de aceptación como `CUANDO ... ENTONCES ...`,
  numerados `AC-n`; equivalen a los `WHEN ... THEN ...` de las skills. Las
  pruebas que demuestran un criterio llevan su identificador en el nombre.
  Los criterios que solo se pueden comprobar con un lector de pantalla se
  marcan `(manual, NVDA)`.
- Review: none
- Implement: codex
- Human merge: `*.csproj`, `*.sln`, `Directory.Build.props`, `instalador/*.nsi`, `*.ps1`
