using UnityEngine;
using UnityEngine.UI;

// UI simplificada para guiar captura: cobertura, mensajes y presets.
public class UIOverlayController : MonoBehaviour
{
    [SerializeField] private Slider coverageSlider;
    [SerializeField] private Text statusText;
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Button processButton;
    [SerializeField] private Dropdown qualityDropdown;
    [SerializeField] private CoverageEstimator coverageEstimator;
    [SerializeField] private ResultViewer resultViewer;
    [SerializeField] private PrintReportOverlay reportOverlay;

    private CaptureController capture;
    private ProcessingController processing;

    private void Awake()
    {
        capture = FindObjectOfType<CaptureController>();
        processing = FindObjectOfType<ProcessingController>();

        startButton.onClick.AddListener(OnStart);
        stopButton.onClick.AddListener(OnStop);
        processButton.onClick.AddListener(OnProcess);
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
    }

    private void OnStart()
    {
        statusText.text = "Capturando... muévete alrededor del objeto";
        capture.StartCapture();
        coverageEstimator.ResetEstimate();
    }

    private void OnStop()
    {
        capture.StopCapture();
        statusText.text = "Captura detenida. Listo para procesar.";
    }

    private void OnProcess()
    {
        statusText.text = "Procesando...";
        int code = processing.RunReconstruction();
        if (code == 0)
        {
            statusText.text = "Listo: STL generado";
            if (resultViewer != null) resultViewer.LoadLatest();
            if (reportOverlay != null) reportOverlay.Refresh();
        }
        else
        {
            statusText.text = $"Error {code}";
        }
    }

    private void OnQualityChanged(int idx)
    {
        // 0: Rápido, 1: Preciso
        var proc = processing;
        if (idx == 0)
        {
            proc.SendMessage("voxelMm", 5);
            proc.SendMessage("targetTris", 50000);
        }
        else
        {
            proc.SendMessage("voxelMm", 3);
            proc.SendMessage("targetTris", 150000);
        }
    }

    private void Update()
    {
        if (coverageEstimator != null)
        {
            UpdateCoverage(coverageEstimator.GetCoverage01());
        }
    }

    // Actualiza cobertura (0..1)
    private void UpdateCoverage(float value)
    {
        coverageSlider.value = value;
        statusText.text = value < 0.8f ? "Sigue rodeando el objeto" : "Cobertura suficiente";
    }
}
