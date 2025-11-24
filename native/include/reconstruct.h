#pragma once

#if defined(_WIN32) || defined(_WIN64)
#  ifdef PHONETSL_EXPORTS
#    define PHONETSL_API __declspec(dllexport)
#  else
#    define PHONETSL_API __declspec(dllimport)
#  endif
#else
#  define PHONETSL_API
#endif

extern "C" {

struct FrameInput {
    const char* rgb_path;
    const char* depth_path;
    float intrinsics[4];   // fx, fy, cx, cy
    float pose[16];        // column-major 4x4
};

struct ReconParams {
    int voxel_mm;           // 2-6 mm
    int max_frames;         // 60-120
    int target_tris;        // 50k-200k
    float trunc_mult;       // 4.0 tipico
    float min_thickness_mm; // 1.5 por defecto
};

struct ReconOutput {
    const char* stl_path;
    const char* glb_path;
    const char* report_path;
};

// Retorna 0 en exito, negativo en error.
PHONETSL_API int Reconstruct(const FrameInput* frames, int frame_count,
                             const ReconParams* params, const ReconOutput* out_paths);

// Codigos de error sugeridos:
// -1: sin frames, -2: no implementado (o stub activo), -3: fallo de carga, -4: fallo de reconstruccion, -5: export fallo.

} // extern "C"

