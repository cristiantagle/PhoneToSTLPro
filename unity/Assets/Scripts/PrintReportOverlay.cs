using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

// Muestra el reporte de imprimibilidad generado por el plugin.
public class PrintReportOverlay : MonoBehaviour
{
    [SerializeField] private ProcessingController processing;
    [SerializeField] private Text manifoldText;
    [SerializeField] private Text holesText;
    [SerializeField] private Text thicknessText;
    [SerializeField] private Text bboxText;

    [Serializable]
    private class Report
    {
        public bool manifold;
        public bool holes_closed;
        public float min_thickness_mm;
        public float[] bbox_mm;
        public string scale_source;
    }

    public void Refresh()
    {
        string path = Path.Combine(processing.OutputDir, "report.json");
        if (!File.Exists(path))
        {
            manifoldText.text = "Reporte no encontrado";
            return;
        }
        try
        {
            var json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<Report>(json);
            manifoldText.text = data.manifold ? "Manifold: Sí" : "Manifold: No";
            holesText.text = data.holes_closed ? "Agujeros: Cerrados" : "Agujeros: Pendientes";
            thicknessText.text = $"Grosor min: {data.min_thickness_mm:0.0} mm";
            if (data.bbox_mm != null && data.bbox_mm.Length == 3)
                bboxText.text = $"Caja: {data.bbox_mm[0]:0} x {data.bbox_mm[1]:0} x {data.bbox_mm[2]:0} mm";
        }
        catch (Exception ex)
        {
            manifoldText.text = $"Error reporte: {ex.Message}";
        }
    }
}
