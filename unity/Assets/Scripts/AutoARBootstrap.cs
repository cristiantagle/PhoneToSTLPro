using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.XR.CoreUtils;

// Crea y enlaza los componentes AR básicos en runtime si faltan,
// para minimizar pasos manuales en escena.
public static class AutoARBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureArSetup()
    {
        var session = Object.FindObjectOfType<ARSession>();
        if (session == null)
        {
            var go = new GameObject("AR Session");
            session = go.AddComponent<ARSession>();
        }

        var origin = Object.FindObjectOfType<XROrigin>();
        if (origin == null)
        {
            var go = new GameObject("XR Origin");
            origin = go.AddComponent<XROrigin>();
        }

        var cam = origin.Camera;
        if (cam == null)
        {
            cam = Object.FindObjectOfType<Camera>();
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                cam.tag = "MainCamera";
            }
            cam.transform.SetParent(origin.transform, false);
            origin.Camera = cam;
        }

        var camMgr = cam.GetComponent<ARCameraManager>();
        if (camMgr == null) camMgr = cam.gameObject.AddComponent<ARCameraManager>();

        var occMgr = cam.GetComponent<AROcclusionManager>();
        if (occMgr == null) occMgr = cam.gameObject.AddComponent<AROcclusionManager>();

        var bg = cam.GetComponent<ARCameraBackground>();
        if (bg == null) bg = cam.gameObject.AddComponent<ARCameraBackground>();

        var capture = Object.FindObjectOfType<CaptureController>();
        var processing = Object.FindObjectOfType<ProcessingController>();
        if (capture == null || processing == null)
        {
            var go = new GameObject("Controllers");
            if (capture == null) capture = go.AddComponent<CaptureController>();
            if (processing == null) processing = go.AddComponent<ProcessingController>();
        }

        capture.Configure(origin, camMgr, occMgr);
        processing.Configure(camMgr);
    }
}
