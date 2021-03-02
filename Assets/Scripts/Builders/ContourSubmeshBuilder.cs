using UnityEngine;
using System.Collections.Generic;

public class ContourSubmeshBuilder
{
    public readonly Material submeshMaterial;
    public List<ContourMeshReader> readers;

    public ContourSubmeshBuilder(ContourMeshReader reader)
    {
        submeshMaterial = reader != null ? reader.MeshMaterial : null;
        readers = new List<ContourMeshReader>() { reader };
    }

    public List<Vector3> GetVertices()
    {
        // All readers in a row
        List<Vector3> vertices = new List<Vector3>();
        foreach (ContourMeshReader r in readers)
        {
            if (r != null && r.Vertices != null)
                vertices.AddRange(r.Vertices);
        }
        return vertices;
    }

    public List<int> GetTriangles(int startIndex, out int endIndex)
    {
        // All readers in a row, but we add an offset to the indices each time we change reader
        int offset = startIndex;
        List<int> triangles = new List<int>();
        foreach (ContourMeshReader r in readers)
        {
            if (r != null && r.Triangles != null)
            {                
                triangles.AddRange(r.Triangles.ConvertAll(index => index + offset));
                offset += r.Vertices.Count;
            }
        }
        endIndex = startIndex + offset;
        return triangles;
    }

    public List<Vector3> GetNormals()
    {
        // All readers in a row
        List<Vector3> normals = new List<Vector3>();
        foreach (ContourMeshReader r in readers)
        {
            if (r != null && r.Normals != null)
                normals.AddRange(r.Normals);
        }
        return normals;
    }

    public List<Vector2> GetUVs()
    {
        // All readers in a row
        List<Vector2> uvs = new List<Vector2>();
        foreach (ContourMeshReader r in readers)
        {
            if (r != null && r.UVs != null)
                uvs.AddRange(r.UVs);
        }
        return uvs;
    }

    public List<Color> GetColors()
    {
        // All readers in a row
        List<Color> colors = new List<Color>();
        foreach (ContourMeshReader r in readers)
        {
            if (r != null && r.Colors != null)
                colors.AddRange(r.Colors);
        }
        return colors;
    }
}