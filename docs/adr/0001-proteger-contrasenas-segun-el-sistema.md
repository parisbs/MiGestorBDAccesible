---
status: proposed
date: 2026-11-02
---
# 0001. Proteger las contraseñas guardadas con DPAPI en Windows y Data Protection en macOS y Linux

## Contexto

`AlmacenConexionesService` cifra las contraseñas de `conexiones.json` con
`ProtectedData` (DPAPI, ámbito de usuario actual). DPAPI solo existe en
Windows, y es la principal razón por la que `MiGestorBDAccesible.Core` apunta a
`net10.0-windows`. El issue parisbs/MiGestorBDAccesible#2 (fase 1 de
portabilidad) pide que el núcleo, las pruebas y las herramientas funcionen en
Windows, macOS y Linux, como requisito previo de un escritorio multiplataforma
y de un posible modo servidor en un VPS sin entorno gráfico.

Restricciones:

- Los archivos de la versión 1.0, con contraseñas cifradas con DPAPI, tienen
  que seguir funcionando en Windows sin migración.
- El producto promete que no usa la red y que los datos se quedan en el
  equipo, cifrados para el usuario.
- La solución tiene que funcionar también sin sesión gráfica ni servicios de
  escritorio, y se tiene que poder probar en la CI de Linux y macOS.

## Decisión

- En Windows las contraseñas se siguen protegiendo con DPAPI, ámbito de
  usuario actual, sin cambios en el formato de los datos existentes.
- En macOS y Linux se protegen con ASP.NET Core Data Protection. El anillo de
  claves se guarda en la carpeta de la aplicación (`RutasAplicacion.Carpeta`),
  con lectura y escritura solo para el propietario (modo 600 en los archivos
  y 700 en su carpeta).
- La protección queda detrás de una interfaz del núcleo, con una
  implementación por sistema que se elige al arrancar.
- Una contraseña que no se puede descifrar en el sistema actual, porque viene
  de otro sistema u otro usuario o porque se perdieron las claves, no lanza
  una excepción: se informa que hay que volver a escribirla.

## Alternativas consideradas

- Llavero del sistema (Keychain en macOS, libsecret o Secret Service en
  Linux): es la opción más segura en un escritorio, porque las claves no son
  un archivo legible por el usuario. Se descarta porque libsecret necesita un
  servicio de sesión que no suele existir en un VPS sin entorno gráfico,
  añade dependencias nativas distintas por sistema y no se puede probar de
  forma fiable en la CI.
- No guardar contraseñas fuera de Windows y pedirlas en cada conexión: es
  simple y no deja secretos en disco. Se descarta porque crea una diferencia
  de comportamiento entre sistemas, en contra de la invariante de que la misma
  acción funcione igual en todas partes, y porque obliga a escribir la
  contraseña en cada conexión, cosa especialmente costosa con un lector de
  pantalla.
- Data Protection también en Windows, en lugar de DPAPI: unificaría el
  código, pero obligaría a migrar los archivos de la versión 1.0 y no aporta
  seguridad sobre DPAPI. Se descarta.

## Consecuencias

- `MiGestorBDAccesible.Core` puede apuntar a `net10.0`; `ProtectedData` solo se
  usa en Windows.
- La seguridad en macOS y Linux equivale a la de DPAPI de usuario actual:
  cualquier proceso del mismo usuario puede descifrar las contraseñas. No
  protege frente a quien tenga acceso a la cuenta, igual que hoy en Windows.
- Si se pierde o se borra el anillo de claves, las contraseñas guardadas en
  ese equipo dejan de poder descifrarse y hay que volver a escribirlas.
- Copiar `conexiones.json` a otro sistema o a otro usuario conserva las
  conexiones, pero no sus contraseñas.
- Un modo servidor con varios usuarios necesitará otra decisión: un único
  anillo de claves por proceso no separa a los usuarios.
- Añade la dependencia `Microsoft.AspNetCore.DataProtection` al núcleo.
