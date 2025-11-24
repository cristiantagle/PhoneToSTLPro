using UnityEngine;

// Estima cobertura usando distancia recorrida y rotación acumulada como proxy.
public class CoverageEstimator : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float targetRotationDegrees = 360f;
    [SerializeField] private float targetDistanceMeters = 2.0f;

    private Vector3 lastPos;
    private Quaternion lastRot;
    private float accDistance;
    private float accRotation;

    private void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
        ResetEstimate();
    }

    public void ResetEstimate()
    {
        if (cameraTransform == null) return;
        lastPos = cameraTransform.position;
        lastRot = cameraTransform.rotation;
        accDistance = 0f;
        accRotation = 0f;
    }

    private void Update()
    {
        if (cameraTransform == null) return;
        accDistance += Vector3.Distance(cameraTransform.position, lastPos);
        accRotation += Quaternion.Angle(cameraTransform.rotation, lastRot);
        lastPos = cameraTransform.position;
        lastRot = cameraTransform.rotation;
    }

    // Retorna 0..1 basado en progreso de rotación y distancia.
    public float GetCoverage01()
    {
        float rotProgress = Mathf.Clamp01(accRotation / targetRotationDegrees);
        float distProgress = Mathf.Clamp01(accDistance / targetDistanceMeters);
        return Mathf.Clamp01((rotProgress + distProgress) * 0.5f);
    }
}
