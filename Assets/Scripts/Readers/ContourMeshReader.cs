using UnityEngine;
using System.Collections.Generic;

public abstract class ContourMeshReader : ContourReader
{
    public Material MeshMaterial { get; protected set; }
    public List<Vector3> Vertices { get; protected set; }
    public List<int> Triangles { get; protected set; }
    public List<Vector3> Normals { get; protected set; }
    public List<Vector2> UVs { get; protected set; }
    public List<Color> Colors { get; protected set; }

    protected void Clear()
    {
        Vertices = null;
        Triangles = null;
        Normals = null;
        UVs = null;
        Colors = null;
    }

    protected void ReadBlueprint(ContourBlueprint.Point[] points, ContourMeshMaterial contourMaterial)
    {
        // Set material
        MeshMaterial = contourMaterial != null ? contourMaterial.meshMaterial : null;
        // No points case
        if (points == null) Clear();
    }
}
