using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

// Orquesta la llamada al plugin nativo y devuelve paths de export.
public class ProcessingController : MonoBehaviour
{
    [SerializeField] private int voxelMm = 4;
    [SerializeField] private int targetTris = 50000;
    [SerializeField] private float minThicknessMm = 1.5f;
    [SerializeField] private float truncMult = 4.0f;
    [SerializeField] private ARCameraManager cameraManager;

    private CaptureController captureController;

    private void Awake()
    {
        captureController = GetComponent<CaptureController>();
    }
    
    public void Configure(ARCameraManager camMgr)
    {
        cameraManager = camMgr;
    }

    [DllImport("phonetsl")]
    private static extern int Reconstruct(FrameInput[] frames, int frame_count,
        ref ReconParams p, ref ReconOutput o);

    [Serializable]
    public struct FrameInput
    {
        public string rgb_path;
        public string depth_path;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] intrinsics;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public float[] pose;
    }

    [Serializable]
    public struct ReconParams
    {
        public int voxel_mm;
        public int max_frames;
        public int target_tris;
        public float trunc_mult;
        public float min_thickness_mm;
    }

    [Serializable]
    public struct ReconOutput
    {
        public string stl_path;
        public string glb_path;
        public string report_path;
    }

    public string OutputDir => Path.Combine(Application.persistentDataPath, "Recons");

    public int RunReconstruction()
    {
        var frames = captureController.GetCapturedFrames();
        if (frames.Count == 0) return -1;

        var intr = GetIntrinsics();

        FrameInput[] inputs = new FrameInput[frames.Count];
        for (int i = 0; i < frames.Count; i++)
        {
            inputs[i] = new FrameInput
            {
                rgb_path = frames[i].rgbPath,
                depth_path = frames[i].depthPath,
                intrinsics = intr,
                pose = MatrixToArray(frames[i].pose)
            };
        }

        Directory.CreateDirectory(OutputDir);
        var output = new ReconOutput
        {
            stl_path = Path.Combine(OutputDir, "model.stl"),
            glb_path = Path.Combine(OutputDir, "model.glb"),
            report_path = Path.Combine(OutputDir, "report.json")
        };

        var p = new ReconParams
        {
            voxel_mm = voxelMm,
            max_frames = frames.Count,
            target_tris = targetTris,
            trunc_mult = truncMult,
            min_thickness_mm = minThicknessMm
        };

        return Reconstruct(inputs, frames.Count, ref p, ref output);
    }

    private float[] GetIntrinsics()
    {
        if (cameraManager != null && cameraManager.TryGetIntrinsics(out var intr))
        {
            return new[] { intr.focalLength.x, intr.focalLength.y, intr.principalPoint.x, intr.principalPoint.y };
        }
        // Fallback aproximado
        return new[] { 1f, 1f, 0.5f, 0.5f };
    }

    private static float[] MatrixToArray(Matrix4x4 m)
    {
        return new[]
        {
            m.m00, m.m10, m.m20, m.m30,
            m.m01, m.m11, m.m21, m.m31,
            m.m02, m.m12, m.m22, m.m32,
            m.m03, m.m13, m.m23, m.m33
        };
    }
}
