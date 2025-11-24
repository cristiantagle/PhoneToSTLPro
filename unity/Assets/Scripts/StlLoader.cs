using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

#nullable enable

// Carga STL ASCII simple a Mesh para previsualización. No soporta binario.
public static class StlLoader
{
    public static Mesh LoadAscii(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException(path);
        var verts = new List<Vector3>();
        var tris = new List<int>();
        using var sr = new StreamReader(path);
        string? line;
        while ((line = sr.ReadLine()) != null)
        {
            line = line.Trim();
            if (line.StartsWith("vertex", StringComparison.OrdinalIgnoreCase))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4 &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) &&
                    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) &&
                    float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
                {
                    verts.Add(new Vector3(x, y, z));
                }
            }
            else if (line.StartsWith("endfacet", StringComparison.OrdinalIgnoreCase))
            {
                int count = verts.Count;
                if (count >= 3)
                {
                    tris.Add(count - 3);
                    tris.Add(count - 2);
                    tris.Add(count - 1);
                }
            }
        }

        if (tris.Count == 0) throw new Exception("No triangles parsed");

        var mesh = new Mesh();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
