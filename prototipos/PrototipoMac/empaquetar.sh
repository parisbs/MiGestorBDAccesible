#!/bin/bash
set -euo pipefail
cd "$(dirname "$0")"

# Se conservan las arquitecturas Release predeterminadas de Mac Catalyst.
dotnet build -c Release -p:EnableCodeSigning=false -p:CreatePackage=false

# Con las dos arquitecturas, la aplicación universal queda en la raíz de bin/Release.
aplicacion="bin/Release/net10.0-maccatalyst26.0/Prototipo Mi Gestor BD Accesible.app"
if [[ ! -d "$aplicacion" ]]; then
    echo "No se encontró la aplicación universal en $aplicacion." >&2
    exit 1
fi
mkdir -p salida
archivo="salida/PrototipoMac-$(date +%Y%m%d-%H%M%S).zip"

# Firma local sin cuenta de desarrollador ni notarización.
codesign --force --deep -s - --entitlements Platforms/MacCatalyst/Entitlements.plist "$aplicacion"
codesign --verify --deep --strict "$aplicacion"
ditto -c -k --sequesterRsrc --keepParent "$aplicacion" "$archivo"
echo "Paquete preparado: $PWD/$archivo"
