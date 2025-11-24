# Especificación técnica

## Stack
- Unity + AR Foundation (ARCore/ARKit), URP.
- Plugin nativo C++: Open3D (TSDF, meshing, decimate), libigl (manifold/hole fill), Eigen (math).
- Export: STL (ASCII/binario), GLB/OBJ opcional.

## Pipeline
1. **Captura** (C#):
   - Recoger fotogramas RGB, profundidad, intrínsecos y poses (matriz 4x4) de AR.
   - Selección de keyframes: cada n frames, descartar blur/redundancia.
   - Guardar en disco: `Frames/{idx}/rgb.png`, `depth.png`, `pose.txt`.
2. **Reconstrucción** (C++):
   - Crear TSDF escalable con voxel y truncation configurables.
   - Integrar cada frame: `Integrate(rgb, depth, intrinsics, extrinsics)`.
   - Extraer malla: Marching Cubes sobre TSDF.
3. **Postprocesado**:
   - Suavizado ligero (simple/taubin) y decimación (quadric) al objetivo de caras.
   - Cierre de agujeros y corrección de normales (libigl/Open3D).
   - Comprobación manifold; remover islas pequeñas.
   - Grosor mínimo: offset ±d y raycast para detectar < umbral; generar mapa de calor.
   - Base plana: cortar por plano Z=0 y tapar; centrar en origen.
   - Escala: usar pose/depth para metros; opción de referencia manual (segmento AR).
4. **Export**:
   - STL (sin texturas) y GLB/OBJ (con texturas si se generaron).
   - Reporte imprimible (JSON) con: manifold bool, hole_fill bool, min_thickness mm, bbox, escala usada.

## Interfaces (C API del plugin)
```c
typedef struct {
  const char* rgb_path;
  const char* depth_path;
  float intrinsics[4];   // fx, fy, cx, cy
  float pose[16];        // column-major 4x4
} FrameInput;

typedef struct {
  int voxel_mm;          // 2-6 mm
  int max_frames;        // 60-120
  int target_tris;       // 50k-200k
  float trunc_mult;      // 4.0 típico
  float min_thickness_mm;// 1.5 por defecto
} ReconParams;

typedef struct {
  const char* stl_path;
  const char* glb_path;
  const char* report_path;
} ReconOutput;

int Reconstruct(const FrameInput* frames, int frame_count,
                const ReconParams* params, const ReconOutput* out_paths);
```

## Pseudocódigo núcleo (C++)
```cpp
Reconstruct(frames, params, out_paths) {
  auto tsdf = ScalableTSDFVolume(params.voxel_mm/1000.0f,
    params.voxel_mm/1000.0f * params.trunc_mult,
    RGB8);
  for (i in 0..frame_count) {
    auto rgb = LoadImage(frames[i].rgb_path);
    auto depth = LoadDepth(frames[i].depth_path);
    tsdf.Integrate(rgb, depth, frames[i].intrinsics, frames[i].pose);
  }
  auto mesh = tsdf.ExtractTriangleMesh();
  mesh = mesh.FilterSmoothSimple(1);
  mesh = mesh.SimplifyQuadricDecimation(params.target_tris);
  CleanManifoldAndHoles(mesh);      // libigl/Open3D
  RemoveSmallComponents(mesh);
  EnsureMinThickness(mesh, params.min_thickness_mm, heatmap);
  FlattenAndSealBase(mesh);
  ExportSTL(mesh, out_paths->stl_path);
  ExportGLB(mesh, out_paths->glb_path);
  WriteReport(mesh, heatmap, out_paths->report_path);
  return 0;
}
```

## UI y UX (resumen)
- Tema oscuro con acentos cian/neón, tipografía Sora/Exo, botones grandes y texto explícito.
- Onboarding ilustrado, checklist antes de capturar (luz, fondo mate, gira alrededor).
- Overlay de cobertura + indicadores de distancia/luz/ángulo; botón finalizar cuando cobertura ≥80%.
- Presets Rápido/Preciso; progreso por etapas; visor con mapa de grosor y checklist imprimible.

## Build (alto nivel)
- Android: Unity + NDK r25+, CMake; compilar Open3D minimal (sin CUDA); enlazar lib estática en `native/` y binding C#.
- iOS: Xcode toolchain; build Open3D para iOS; enlazar estática y binding.
- Evitar dependencias con licencias no free; solo MIT/BSD compatible.
