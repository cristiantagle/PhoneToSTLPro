# Ejecuta el build oficial de Open3D para Android arm64 usando el script de Open3D
# (util/android/build-android-arm64.sh). Requiere: Git Bash (o WSL), CMake, Ninja, NDK.
param(
    [string]$NdKRoot = "",
    [string]$BashPath = "",
    [string]$RepoDir = ""
)

$ErrorActionPreference = "Stop"

function Resolve-Ndk {
    param([string]$Override)
    if ($Override -and (Test-Path $Override)) { return (Resolve-Path $Override).Path }
    $ndkBase = Join-Path $env:LOCALAPPDATA "Android\Sdk\ndk"
    if (-not (Test-Path $ndkBase)) { throw "No se encontró la carpeta NDK en $ndkBase. Instala el NDK desde Android SDK Manager." }
    $candidates = Get-ChildItem $ndkBase -Directory | Sort-Object Name -Descending
    if ($candidates.Count -eq 0) { throw "No hay versiones de NDK dentro de $ndkBase." }
    return $candidates[0].FullName
}

function Resolve-Bash {
    param([string]$Override)
    if ($Override -and (Test-Path $Override)) { return (Resolve-Path $Override).Path }
    $gitBash = "C:\Program Files\Git\bin\bash.exe"
    if (Test-Path $gitBash) { return $gitBash }
    throw "No se encontró bash. Instala Git for Windows o WSL y pasa la ruta con -BashPath."
}

$ndk = (Resolve-Ndk -Override $NdKRoot) -replace '\\','/'
$bash = Resolve-Bash -Override $BashPath

$open3dRoot = if ($RepoDir) { $RepoDir } else { Join-Path $PSScriptRoot "Open3D-official" }
$script = Join-Path $open3dRoot "util/android/build-android-arm64.sh"

if (-not (Test-Path $script)) {
    Write-Host "Clonando repo oficial de Open3D en $open3dRoot..."
    Remove-Item -Recurse -Force $open3dRoot -ErrorAction SilentlyContinue
    git clone --depth 1 https://github.com/isl-org/Open3D.git $open3dRoot | Out-Null
}

if (-not (Test-Path $script)) { throw "No existe $script incluso tras clonar. Revisa que util/android esté en el repo clonado." }

Write-Host "NDK   : $ndk"
Write-Host "Bash  : $bash"
Write-Host "Script: $script"

$env:ANDROID_NDK = $ndk

# Llama al build oficial. Genera el SDK en Open3D/build-android-arm64/install
& $bash $script

Write-Host "Build finalizado. El SDK quedará en Open3D/build-android-arm64/install."
