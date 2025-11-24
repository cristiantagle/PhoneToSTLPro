using System.IO;
using UnityEngine;

// Muestra el STL generado en escena.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ResultViewer : MonoBehaviour
{
    [SerializeField] private ProcessingController processing;
    [SerializeField] private string stlFileName = "model.stl";

    private MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    public void LoadLatest()
    {
        string path = Path.Combine(processing.OutputDir, stlFileName);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"STL no encontrado en {path}");
            return;
        }
        try
        {
            var mesh = StlLoader.LoadAscii(path);
            meshFilter.sharedMesh = mesh;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error cargando STL: {ex.Message}");
        }
    }
}
