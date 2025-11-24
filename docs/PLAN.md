# Proyecto: App móvil de escaneo a STL imprimible

## Objetivo
App móvil (Android/iOS) que guía al usuario para capturar un objeto, reconstruye una malla 3D y exporta STL imprimible, todo offline y sin costes de licencias.

## Arquitectura
- **Cliente**: Unity + AR Foundation (ARCore/ARKit) + URP.
- **Plugin nativo C++**: Pipeline de reconstrucción (TSDF → Marching Cubes → limpieza → STL/GLB), usando Open3D, libigl, Eigen.
- **Capas**:
  - `Capture`: fotogramas RGB, profundidad, poses (AR).
  - `Reconstruction`: fusión TSDF y extracción de malla.
  - `Post`: suavizado, decimación, cierre de agujeros, base plana, chequeo de grosor mínimo, escala real.
  - `Export`: STL (sin textura), GLB/OBJ opcional (con textura).
  - `PrintCheck`: reporte de imprimibilidad (manifold, grosor, bounding box, escala).

## Flujo de usuario
1. **Onboarding**: 3 pantallas ilustradas (buena luz, fondo mate, rodear objeto). Tip: coloca tarjeta de crédito para escala.
2. **Captura guiada**:
   - Overlay de cobertura (círculo que se llena; neón sobre fondo oscuro).
   - Indicadores: distancia (muy cerca/lejos), luz (baja), ángulo (sube/baja).
   - Botón finalizar activo cuando cobertura ≥80% o una vuelta completa.
3. **Procesado**:
   - Presets: Rápido (2-3 min, ~50k tris), Preciso (más tiempo, ~150k).
   - Progreso por etapas: Fusionando → Malla → Limpieza → Revisando imprimible.
4. **Resultado**:
   - Visor 3D (autospin), mapa de calor de grosor (<1.5 mm en rojo).
   - Checklist imprimible: manifold ✓, agujeros ✓, grosor ≥ umbral, tamaño real, base plana.
   - Exportar STL/GLB, compartir.

## Presets recomendados (móviles gama media/alta)
- Rápido: voxel 4-6 mm, máx 60 keyframes, mesh objetivo 50k caras.
- Preciso: voxel 2-3 mm, máx 120 keyframes, mesh objetivo 150k-200k caras.
- Grosor mínimo por defecto: 1.5 mm (configurable 1.2-2.0).

## Roadmap de ejecución
1) **Base Unity**: escena con `ARSessionOrigin`, captura y guardado de RGB/depth/poses.  
2) **Plugin C++ mínimo**: integra TSDF y devuelve malla PLY simple.  
3) **Meshing y export**: Marching Cubes → decimate → STL/GLB.  
4) **Post imprimible**: fill holes, manifold check, base plana, grosor mínimo, reporte.  
5) **UX/UI**: onboarding, overlay guiado, presets, visor con mapa de grosor.  
6) **Pulido**: escala de referencia, manejo de errores (tracking/luz), compartir.

## Riesgos y mitigación
- **Tiempo de procesado**: presets rápidos y procesamiento en segundo plano con ETA.
- **Superficies brillantes/transparentes**: aviso previo, sugerir cinta mate.
- **Escala incorrecta**: pedir referencia (segmento AR o tarjeta) antes de procesar.
- **RAM limitada**: TSDF escalable en tiles; límite de keyframes.
