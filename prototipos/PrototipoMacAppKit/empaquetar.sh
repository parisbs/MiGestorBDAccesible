#!/bin/bash
set -euo pipefail
cd "$(dirname "$0")"

# Restaura y compila Release para osx-x64 y osx-arm64 (aplicación universal).
dotnet build -c Release -p:EnableCodeSigning=false -p:CreatePackage=false

aplicacion="bin/Release/net10.0-macos26.0/Prototipo AppKit Mi Gestor BD Accesible.app"
if [[ ! -d "$aplicacion" ]]; then
    echo "No se encontró la aplicación universal en $aplicacion." >&2
    exit 1
fi
ejecutable=$(/usr/libexec/PlistBuddy -c 'Print :CFBundleExecutable' "$aplicacion/Contents/Info.plist")
lipo "$aplicacion/Contents/MacOS/$ejecutable" -verify_arch x86_64 arm64

# Firma local sin cuenta de desarrollador, App Sandbox ni permisos de red.
codesign --force --deep -s - --entitlements Entitlements.plist "$aplicacion"
codesign --verify --deep --strict "$aplicacion"
mkdir -p salida
archivo="salida/PrototipoMacAppKit-$(date +%Y%m%d-%H%M%S).zip"
ditto -c -k --sequesterRsrc --keepParent "$aplicacion" "$archivo"
echo "Paquete preparado: $PWD/$archivo"
