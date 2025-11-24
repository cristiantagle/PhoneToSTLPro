# Requires: CMake, Ninja, Android NDK; tested from PowerShell.
param(
    [string]$NdKRoot = ""
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

function Resolve-Ninja {
    $cmd = Get-Command ninja -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    # fallback a instalación vía pip
    $pipNinja = Join-Path $env:USERPROFILE "AppData\Roaming\Python\Python313\Scripts\ninja.exe"
    if (Test-Path $pipNinja) { return $pipNinja }
    throw "No se encontró Ninja. Instálalo con 'python -m pip install --user ninja' o añade ninja al PATH."
}

$ndk = (Resolve-Ndk -Override $NdKRoot) -replace '\\','/'
$ninja = Resolve-Ninja

$open3dSrc     = Join-Path $PSScriptRoot "Open3D"
$open3dBuild   = Join-Path $open3dSrc "build-android"
$open3dInstall = "C:/libs/open3d-android"
$phonetslSrc   = Join-Path $PSScriptRoot "native"
$phonetslBuild = Join-Path $phonetslSrc "build-android"
$unityPlugins  = Join-Path $PSScriptRoot "unity/Assets/Plugins/Android"

Write-Host "NDK        : $ndk"
Write-Host "Ninja      : $ninja"
Write-Host "Open3D src : $open3dSrc"
Write-Host "Install    : $open3dInstall"

Remove-Item -Recurse -Force $open3dBuild -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force $phonetslBuild -ErrorAction SilentlyContinue

$env:PATH = "$(Split-Path $ninja);$ndk/prebuilt/windows-x86_64/bin;$env:PATH"

# 1) Open3D para Android arm64, sin IPP ni OpenGL/visualización.
cmake -S $open3dSrc -B $open3dBuild `
  -DCMAKE_TOOLCHAIN_FILE="$ndk/build/cmake/android.toolchain.cmake" `
  -DCMAKE_MAKE_PROGRAM="$ninja" `
  -DCMAKE_TRY_COMPILE_TARGET_TYPE=STATIC_LIBRARY `
  -DANDROID_ABI=arm64-v8a `
  -DANDROID_PLATFORM=android-24 `
  -DANDROID_STL=c++_shared `
  -DBUILD_SHARED_LIBS=ON `
  -DBUILD_GUI=OFF -DBUILD_VISUALIZATION=OFF -DBUILD_EXAMPLES=OFF -DBUILD_PYTHON_MODULE=OFF -DBUILD_JUPYTER_EXTENSION=OFF `
  -DBUILD_RENDERING=OFF `
  -DBUILD_FILAMENT_FROM_SOURCE=OFF -DBUILD_CUDA_MODULE=OFF `
  -DBUILD_ISPC_MODULE=OFF `
  -DOPEN3D_ML=OFF `
  -DBUILD_IPPICV=OFF `
  -DBUILD_TBB=OFF `
  -DWITH_IPP=OFF `
  -DWITH_OPENGL=OFF `
  -DBUILD_TOOLS=OFF `
  -DBUILD_BENCHMARKS=OFF `
  -DBUILD_WEBRTC=OFF `
  -DENABLE_HEADLESS_RENDERING=OFF `
  -DBUILD_OSMESA=OFF `
  -DUSE_SYSTEM_GLEW=OFF `
  -DUSE_SYSTEM_GLFW=OFF `
  -G "Ninja"

cmake --build $open3dBuild --config Release
cmake --install $open3dBuild --prefix $open3dInstall

# 2) phonetsl.so enlazando Open3D Android
cmake -S $phonetslSrc -B $phonetslBuild `
  -DOPEN3D_ROOT="$open3dInstall" `
  -DCMAKE_SYSTEM_NAME=Android `
  -DCMAKE_ANDROID_NDK="$ndk" `
  -DCMAKE_MAKE_PROGRAM="$ninja" `
  -DCMAKE_TRY_COMPILE_TARGET_TYPE=STATIC_LIBRARY `
  -DCMAKE_SYSTEM_VERSION=24 `
  -DCMAKE_ANDROID_ARCH_ABI=arm64-v8a `
  -DCMAKE_ANDROID_STL_TYPE=c++_shared `
  -DCMAKE_BUILD_TYPE=Release `
  -G "Ninja"

cmake --build $phonetslBuild --config Release

New-Item -ItemType Directory -Force -Path $unityPlugins | Out-Null
Copy-Item "$phonetslBuild/libphonetsl.so" "$unityPlugins/phonetsl.so" -Force

Write-Host ""
Write-Host "Listo. Se copió phonetsl.so a $unityPlugins"
Write-Host "Si Unity está abierto, reimporta o haz Build & Run de nuevo."
