using UnityEngine;
using System.Collections.Generic;

public class ContourFaceReader : ContourMeshReader
{
    public override bool TryReadBlueprint(ContourBlueprint blueprint)
    {
        // Read if possible
        if (blueprint != null && blueprint.material is ContourFaceMaterial)
        {
            ReadBlueprint(blueprint.positions, blueprint.material as ContourFaceMaterial);
            return true;
        }
        // If not, return false
        return false;
    }

    private void ReadBlueprint(Vector2[] positions, ContourFaceMaterial contourMaterial)
    {
        // Set material
        MeshMaterial = contourMaterial != null ? contourMaterial.meshMaterial : null;
        // Check if enough points to build at least one triangle
        int positionCount = positions != null ? positions.Length : 0;
        if (positionCount < 3)
        {
            Clear();
            return;
        }
        if (positions == null) return;
        // Set vertices: copy the enabled positions x,y and set z to 0
        int vertexCount = positionCount;
        Vertices = new List<Vector3>(vertexCount);
        foreach (Vector2 pos in positions) Vertices.Add(pos);
        // Set triangles: simple convex triangulation
        int triangleCount = Mathf.Max(vertexCount - 2, 0);
        Triangles = new List<int>(triangleCount * 3);
        for (int i = 0; i < triangleCount; i++)
            Triangles.AddRange(new int[] { 0, i + 1, i + 2 });
        // Set normals: all to (0, 0, -1)
        Normals = new List<Vector3>(vertexCount);
        for (int i = 0; i < vertexCount; i++)
            Normals.Add(Vector3.back);
        // Set uvs: since it's 2D, use vertex positions
        UVs = new List<Vector2>(vertexCount);
        for (int i = 0; i < vertexCount; i++)
            UVs.Add(Vertices[i]);
        // Set colors: all to the same color
        Color color = contourMaterial.color;
        Colors = new List<Color>(vertexCount);
        for (int i = 0; i < vertexCount; i++)
            Colors.Add(color);
    }
}