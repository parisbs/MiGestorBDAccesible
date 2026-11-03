# Registros de decisiones de arquitectura

Decisiones que cierran una alternativa, en formato MADR 4: copia
[template.md](template.md) como `NNNN-nombre-breve.md` con el número siguiente.

El estado de un registro es el campo `status` de su front matter YAML, con
los valores en inglés que entiende la comprobación (`proposed`, `accepted`,
`rejected`, `deprecated`, `superseded by NNNN`). Un registro es inmutable desde
el commit de `main` en el que su estado pasa a aceptado o rechazado; los
registros propuestos siguen siendo borradores editables mediante pull
requests. Una decisión que cambia es un registro nuevo, y en el registro
sustituido solo cambia el estado. `scripts/check-adrs.mjs` lo comprueba en CI.

Estos registros viven solo en la rama `workflow` del fork.

## Registros

- [0001. Proteger las contraseñas guardadas con DPAPI en Windows y Data Protection en macOS y Linux](0001-proteger-contrasenas-segun-el-sistema.md)
