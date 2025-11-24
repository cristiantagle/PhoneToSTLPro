# Roadmap detallado

## Fase 0: Setup
- [ ] Crear proyecto Unity 2022.3+ URP + AR Foundation (ARCore/ARKit).
- [ ] Añadir escena base con `ARSession`, `ARSessionOrigin`, `ARCamera`, `AROcclusionManager`.
- [ ] Copiar scripts `CaptureController`, `ProcessingController`, `UIOverlayController` y asignar referencias.

## Fase 1: Captura robusta
- [ ] Integrar intrínsecos reales de cámara (ARCameraManager) en `ProcessingController`.
- [ ] Métrica de cobertura: usar puntos de característica/poses para estimar % de recorrido; actualizar `UIOverlayController.UpdateCoverage`.
- [ ] Guardado eficiente (compresión opcional, límites de disco).

## Fase 2: Núcleo nativo
- [ ] Compilar Open3D + libigl + Eigen para Android/iOS.
- [ ] Implementar `Reconstruct`: TSDF → Marching Cubes → suavizado → decimación.
- [ ] Añadir limpieza: cerrar agujeros, manifold check, remover islas pequeñas.
- [ ] Base plana y centrado en origen; grosor mínimo + mapa de calor.
- [ ] Export STL/GLB y reporte JSON (manifold, grosor, bbox, escala).
- [ ] Exponer códigos de error y logs al C# (marshal de strings opcional).

## Fase 3: UI/UX guiada
- [ ] Onboarding ilustrado (3 pantallas) con tips de luz/fondo/escala.
- [ ] Overlay de captura con indicadores (luz, distancia, ángulo) y checklist de cobertura.
- [ ] Presets Rápido/Preciso + barra de progreso por etapas.
- [ ] Visor 3D con autospin, mapa de grosor, checklist imprimible.

## Fase 4: Escala y validación
- [ ] Referencia de escala: segmento medido en AR o tarjeta de crédito reconocida.
- [ ] Validar imprimibilidad: mostrar alertas por grosor insuficiente y agujeros.
- [ ] Ajustes de usuario: umbral de grosor, decimación, recorte de base.

## Fase 5: Pulido y QA
- [ ] Manejo de errores (tracking perdido, poca luz, falta de profundidad).
- [ ] Performance: TSDF en tiles, limitador de keyframes, procesamiento en segundo plano.
- [ ] Pruebas en dispositivos (gama media y alta), métricas de tiempo/memoria.
- [ ] Flujo de compartir/exportar y ver STL en apps externas.
