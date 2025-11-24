# Uso: ajustar rutas de OPEN3D_ROOT y ejecutar en PowerShell.
param(
    [string]$Open3DRoot = "C:/libs/open3d",
    [string]$BuildDir = "build"
)

$ErrorActionPreference = "Stop"

New-Item -ItemType Directory -Force -Path $BuildDir | Out-Null
Push-Location $BuildDir

cmake .. `
  -DOPEN3D_ROOT="$Open3DRoot" `
  -DCMAKE_BUILD_TYPE=Release

cmake --build . --config Release

Pop-Location
