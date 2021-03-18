using UnityEngine;
using System.Collections.Generic;

public class ContourFaceReader : ContourMeshReader
{
    public override bool TryReadBlueprint(ContourBlueprint blueprint)
    {
        // Read if possible
        if (blueprint != null && blueprint is ContourMeshBlueprint && blueprint.material is ContourFaceMaterial)
        {
            ContourMeshBlueprint meshBlueprint = blueprint as ContourMeshBlueprint;
            // Get positions
            Vector2[] positions = blueprint.positions;
            if (positions == null) return false;
            // Get material
            ContourFaceMaterial contourMaterial = blueprint.material as ContourFaceMaterial;
            if (contourMaterial == null) return false;
            MeshMaterial = contourMaterial.meshMaterial;
            // To build a face, there must be enough positions and contour must be a loop
            int positionCount = positions != null ? positions.Length : 0;
            if (positionCount < 3 || positions[0] != positions[positionCount - 1])
            {
                Clear();
                return false;
            }
            // Set vertices: copy positions x,y and set z to value in contourMaterial (last position is ignored because of loop)
            int vertexCount = positionCount - 1;
            Vector3 zOffset = contourMaterial.zOffset * Vector3.forward;
            Vertices = new List<Vector3>(vertexCount);
            for (int i = 0; i < vertexCount; i++)
                Vertices.Add((Vector3)positions[i] + zOffset);
            // Set triangles: simple convex triangulation
            int triangleCount = Mathf.Max(vertexCount - 2, 0);
            Triangles = new List<int>(triangleCount * 3);
            for (int i = 0; i < triangleCount; i++)
                Triangles.AddRange(new int[] { 0, i + 1, i + 2 });
            // Set normals: fetch value in blueprint
            Normals = new List<Vector3>(vertexCount);
            Vector3 normal = meshBlueprint.Normal;
            for (int i = 0; i < vertexCount; i++)
                Normals.Add(normal);
            // Set uvs: since it's 2D, use vertex positions
            UVs = new List<Vector2>(vertexCount);
            for (int i = 0; i < vertexCount; i++)
                UVs.Add(Vertices[i]);
            // Set colors: fetch color in blueprint
            Color color = meshBlueprint.Color;
            Colors = new List<Color>(vertexCount);
            for (int i = 0; i < vertexCount; i++)
                Colors.Add(color);
            return true;
        }
        // If not, return false
        return false;
    }
}