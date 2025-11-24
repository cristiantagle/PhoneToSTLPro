# Unity setup (gama media/alta, ARCore/ARKit)

1) Crear proyecto Unity 2022.3+ (URP 3D).  
2) Paquetes: AR Foundation, ARCore XR Plugin, ARKit XR Plugin (Window → Package Manager).  
3) Escena base: añadir `ARSession`, `ARSessionOrigin`, `ARCamera` (con `ARCameraManager`, `ARCameraBackground`, `ARPoseDriver`), `AROcclusionManager` (depth).  
4) Copiar scripts de `Assets/Scripts/` y asignarlos:
   - `CaptureController` al `ARSessionOrigin` o GameObject vacío.
   - `ProcessingController` (mismo objeto posible) con referencia al plugin nativo y `ARCameraManager`.
   - `UIOverlayController` en el Canvas; referenciar `CoverageEstimator`.
   - `CoverageEstimator` (usa la cámara) para estimar cobertura.
   - `ResultViewer` + `PrintReportOverlay` en el panel de resultado para mostrar STL/checklist.
5) Android: Instalar Android Build Support + NDK r25+, Player Settings → XR Plug-in → ARCore.  
6) iOS: Xcode, Player Settings → XR Plug-in → ARKit.

## Flujo en escena
- Botón “Capturar”: inicia captura de frames RGB + depth + pose en disco.
- Overlay muestra cobertura e indicaciones.
- Botón “Procesar”: llama al plugin nativo (DLL) con paths de frames, genera STL/GLB y reporte.
- Visor 3D carga el STL y muestra checklist imprimible.

## Después de procesar
- `ResultViewer.LoadLatest()` carga el STL generado (ASCII placeholder).
- `PrintReportOverlay.Refresh()` lee `report.json` y actualiza textos de manifold/grosor/bbox.

## Notas
- Rutas de guardado: `Application.persistentDataPath`.
- Plugin nativo por plataforma en `Assets/Plugins/Android` (AAR/.so) y `Assets/Plugins/iOS` (bundle estático).
