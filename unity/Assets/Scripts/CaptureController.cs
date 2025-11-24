using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;

// Captura frames RGB + depth + pose y los persiste para el plugin nativo.
public class CaptureController : MonoBehaviour
{
    [Header("Componentes AR")]
    [SerializeField] private ARCameraManager cameraManager;
    [SerializeField] private AROcclusionManager occlusionManager;
    [SerializeField] private XROrigin xrOrigin;

    [Header("Configuración")]
    [SerializeField] private int keyframeInterval = 3;
    [SerializeField] private int maxFrames = 120;
    [SerializeField] private string captureFolderName = "Frames";

    private int frameCount;
    private int captureIndex;
    private bool capturing;
    private string capturePath;
    private readonly List<FrameInfo> captured = new();
    
    public void Configure(XROrigin origin, ARCameraManager camMgr, AROcclusionManager occMgr)
    {
        xrOrigin = origin;
        cameraManager = camMgr;
        occlusionManager = occMgr;
    }

    private void Awake()
    {
        capturePath = Path.Combine(Application.persistentDataPath, captureFolderName);
        Directory.CreateDirectory(capturePath);
    }

    public void StartCapture()
    {
        captured.Clear();
        captureIndex = 0;
        frameCount = 0;
        capturing = true;
    }

    public void StopCapture()
    {
        capturing = false;
    }

    private void OnEnable()
    {
        if (cameraManager != null)
        {
            cameraManager.frameReceived += OnFrameReceived;
        }
    }

    private void OnDisable()
    {
        if (cameraManager != null)
        {
            cameraManager.frameReceived -= OnFrameReceived;
        }
    }

    private void OnFrameReceived(ARCameraFrameEventArgs args)
    {
        if (!capturing) return;
        if (frameCount % keyframeInterval != 0) { frameCount++; return; }
        if (captureIndex >= maxFrames) { StopCapture(); return; }

        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image)) return;

        Texture2D rgbTex = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(image.width, image.height),
            outputFormat = TextureFormat.RGBA32,
            transformation = XRCpuImage.Transformation.MirrorY
        };
        var rawTextureData = rgbTex.GetRawTextureData<byte>();
        image.Convert(conversionParams, rawTextureData);
        rgbTex.Apply();
        image.Dispose();

        Texture2D depthTex = null;
        if (occlusionManager != null && occlusionManager.TryAcquireEnvironmentDepthCpuImage(out var depthImage))
        {
            depthTex = new Texture2D(depthImage.width, depthImage.height, TextureFormat.R16, false);
            var depthParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, depthImage.width, depthImage.height),
                outputDimensions = new Vector2Int(depthImage.width, depthImage.height),
                outputFormat = TextureFormat.R16,
                transformation = XRCpuImage.Transformation.MirrorY
            };
            var depthRaw = depthTex.GetRawTextureData<byte>();
            depthImage.Convert(depthParams, depthRaw);
            depthTex.Apply();
            depthImage.Dispose();
        }

        Matrix4x4 pose = xrOrigin != null && xrOrigin.Camera != null
            ? xrOrigin.Camera.transform.localToWorldMatrix
            : Matrix4x4.identity;

        string frameDir = Path.Combine(capturePath, captureIndex.ToString("D4"));
        Directory.CreateDirectory(frameDir);

        File.WriteAllBytes(Path.Combine(frameDir, "rgb.png"), rgbTex.EncodeToPNG());
        if (depthTex != null)
        {
            File.WriteAllBytes(Path.Combine(frameDir, "depth.png"), depthTex.EncodeToPNG());
        }
        File.WriteAllText(Path.Combine(frameDir, "pose.txt"), PoseToString(pose));

        captured.Add(new FrameInfo
        {
            folder = frameDir,
            rgbPath = Path.Combine(frameDir, "rgb.png"),
            depthPath = depthTex != null ? Path.Combine(frameDir, "depth.png") : null,
            pose = pose
        });

        captureIndex++;
        frameCount++;
    }

    private static string PoseToString(Matrix4x4 m)
    {
        // column-major 4x4
        return $"{m.m00} {m.m10} {m.m20} {m.m30}\n" +
               $"{m.m01} {m.m11} {m.m21} {m.m31}\n" +
               $"{m.m02} {m.m12} {m.m22} {m.m32}\n" +
               $"{m.m03} {m.m13} {m.m23} {m.m33}\n";
    }

    public IReadOnlyList<FrameInfo> GetCapturedFrames() => captured;
}

public class FrameInfo
{
    public string folder;
    public string rgbPath;
    public string depthPath;
    public Matrix4x4 pose;
}
