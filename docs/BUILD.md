# Build nativo y Unity (alto nivel)

## Dependencias
- Unity 2022.3+ con URP, AR Foundation, ARCore XR Plugin, ARKit XR Plugin.
- CMake 3.15+, Android NDK r25+ (para Android).
- Xcode + toolchain iOS (para iOS).
- Librerías: Open3D (sin CUDA), libigl (header-only), Eigen (header-only).

## Android (NDK)
1) Construir Open3D para Android (armeabi-v7a/arm64-v8a) con CMake/NDK. Ejemplo (ajusta rutas):
```
cmake -S /path/to/Open3D -B build-android \
  -DANDROID_ABI=arm64-v8a \
  -DANDROID_PLATFORM=android-24 \
  -DANDROID_NDK=$ANDROID_NDK_ROOT \
  -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK_ROOT/build/cmake/android.toolchain.cmake \
  -DBUILD_SHARED_LIBS=OFF -DBUILD_GUI=OFF -DBUILD_WEBRTC=OFF
cmake --build build-android --config Release
```
2) Ajustar `native/CMakeLists.txt` con `OPEN3D_ROOT` y `Eigen3`.
3) Generar `.so` y empaquetar en AAR (carpeta `Assets/Plugins/Android/` en Unity).

## iOS
1) Construir Open3D para iOS (arm64):
```
cmake -S /path/to/Open3D -B build-ios \
  -DCMAKE_TOOLCHAIN_FILE=/path/to/ios.toolchain.cmake \
  -DPLATFORM=OS64 \
  -DBUILD_SHARED_LIBS=OFF -DBUILD_GUI=OFF -DBUILD_WEBRTC=OFF
cmake --build build-ios --config Release
```
2) Ajustar `native/CMakeLists.txt` y generar lib estática; añadir a `Assets/Plugins/iOS/`.

## Unity
1) Crear proyecto en `unity/` (URP 3D).  
2) Importar AR Foundation + ARCore/ARKit.  
3) Añadir escena con `ARSession`, `ARSessionOrigin`, `ARCamera`, `AROcclusionManager`.  
4) Añadir scripts `CaptureController`, `ProcessingController`, `UIOverlayController` y conectar referencias.  
5) Configurar Player Settings: XR Plug-in → ARCore/ARKit; permisos de cámara.  
6) Build para dispositivo y probar captura básica; luego integrar plugin nativo compilado.

## Próximos pasos (estado actual en Windows)
- Open3D instalado en `Open3D/build-release/install` (headers+libs).
- Plugin nativo compilado: `native/build/Release/phonetsl.dll` copiado a `unity/Assets/Plugins/x86_64/`.
- Unity Editor 2022.3.62f3 con módulos Android/Windows; JDK/SDK/NDK/Gradle visibles en External Tools.
- Terminar instalación de Visual Studio Community 2022 con workloads: Desarrollo de juego con Unity + Desarrollo para escritorio con C++ (MSVC v14x, Windows 10 SDK, CMake). Reiniciar si lo pide.
- En Unity Editor (proyecto `C:/PhoneToSTLPro/unity`): Edit > Preferences > External Tools, seleccionar Microsoft Visual Studio como External Script Editor.
- Reabrir el proyecto fuera de Safe Mode y dejar que importe; revisar y resolver errores de compilación si aparecen.
