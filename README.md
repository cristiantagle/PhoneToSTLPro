# PhoneToSTLPro (prototipo)

App móvil (Android/iOS) que guía la captura de un objeto y genera un STL imprimible offline. Este repo contiene el plan, estructura y especificaciones para empezar el desarrollo en Unity + AR Foundation con un plugin nativo C++ (Open3D/libigl).

## Estructura propuesta
- `unity/` – Proyecto Unity (crear con URP + AR Foundation).  
- `native/` – Plugin C++ (NDK/Swift) con Open3D/libigl/Eigen.  
- `docs/` – Plan, especificaciones y checklists.

## Pasos rápidos para comenzar
1) Crear proyecto Unity URP 3D y agregar paquetes: AR Foundation, ARCore XR Plugin, ARKit XR Plugin.  
2) Copiar/crear carpeta `unity/` y configurar escena base con `ARSessionOrigin`, `ARSession`, cámara AR.  
3) Preparar toolchain nativa: Android NDK r25+, CMake; en iOS usar Xcode toolchain.  
4) Compilar `native/` como biblioteca estática y exponer funciones C API para Unity.

## Tareas clave (checklist)
- [ ] Captura: RGB + depth + poses (AR), guardado en disco.
- [ ] Orquestación: cola de trabajos captura → TSDF → malla → limpieza → export.
- [ ] Reconstrucción: integración TSDF, Marching Cubes, decimación.
- [ ] Post: cierre de agujeros, manifold check, base plana, grosor mínimo.
- [ ] Export: STL obligatorio, GLB/OBJ opcional.
- [ ] UI: onboarding ilustrado, overlay de cobertura, presets rápido/preciso, visor 3D + mapa de grosor, checklist imprimible.
- [ ] Escala: referencia con segmento AR o tarjeta.
- [ ] Manejo de errores: poca luz, tracking perdido, cobertura insuficiente.

## Presets
- Rápido: voxel 4-6 mm, ≤60 keyframes, ~50k caras.
- Preciso: voxel 2-3 mm, ≤120 keyframes, ~150k-200k caras.

## Exportables mínimos
- STL limpio (manifold, agujeros cerrados, base plana, grosor >= umbral).
- Reporte de imprimibilidad (manifold, grosor, bounding box, escala) mostrado en UI y adjunto al export.
