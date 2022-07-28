using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ContourFaceReader : ContourMeshReader
{
    public override bool CanReadBlueprint(ContourBlueprint blueprint)
    {
        return blueprint != null && blueprint.material != null && blueprint.material is ContourFaceMaterial && blueprint.IsLoop;
    }

    public override bool TryReadBlueprint(ContourBlueprint blueprint)
    {
        // If contour is not looped, correct it
        if (blueprint != null && blueprint.material != null && blueprint.material is ContourFaceMaterial && !blueprint.IsLoop)
            blueprint.shape.LoopContour();
        // Read if possible
        if (CanReadBlueprint(blueprint))
        {
            // Get positions and material
            Vector2[] positions = blueprint.Positions;
            ContourFaceMaterial contourMaterial = blueprint.material as ContourFaceMaterial;
            MeshMaterial = contourMaterial.meshMaterial;
            // Set vertices: copy positions x,y and set z to value in contourMaterial (last position is ignored because of loop)
            int vertexCount = positions.Length - 1;
            Vector3 zOffset = contourMaterial.zOffset * Vector3.forward;
            Vertices = new List<Vector3>(vertexCount);
            for (int i = 0; i < vertexCount; i++)
                Vertices.Add((Vector3)positions[i] + zOffset);
            // Set triangles: simple triangulation (convex shape only)
            int triangleCount = Mathf.Max(vertexCount - 2, 0);
            Triangles = new List<int>(triangleCount * 3);
            for (int i = 0; i < triangleCount; i++)
                Triangles.AddRange(new int[] { 0, i + 1, i + 2 });
            // Set uvs: since it's 2D, use vertex positions
            UVs = new List<Vector2>(vertexCount);
            for (int i = 0; i < vertexCount; i++)
                UVs.Add(Vertices[i]);
            // Set normals: fetch value in blueprint
            Normals = new List<Vector3>(vertexCount);
            Vector3 normal = blueprint.Normal;
            for (int i = 0; i < vertexCount; i++)
                Normals.Add(normal);
            // Set colors: fetch color in material
            Color color = contourMaterial.color;
            Colors = new List<Color>(vertexCount);
            for (int i = 0; i < vertexCount; i++)
                Colors.Add(color);
            return true;
        }
        // If not, return false
        return false;
    }

    public override bool ReadBlueprintPositions(ContourBlueprint blueprint)
    {
        // Read contour positions only (assumes only modification on contour is some point moved)
        if (CanReadBlueprint(blueprint))
        {
            // Get positions
            Vector2[] positions = blueprint.Positions;
            // Check if blueprint matches reader mesh's length
            int positionCount = positions != null ? positions.Length : 0;
            int vertexCount = Vertices != null ? Vertices.Count : 0;
            if (vertexCount == positionCount - 1)
            {
                if (vertexCount > 0)
                {
                    // Update vertices
                    for (int i = 0; i < vertexCount; i++)
                        Vertices[i] = new Vector3(positions[i].x, positions[i].y, Vertices[i].z);
                    // Update uvs (to avoid texture stretching)
                    for (int i = 0; i < vertexCount; i++)
                        UVs[i] = Vertices[i];
                }
            }
            return true;
        }
        else if (blueprint != null && blueprint.material != null && blueprint.material is ContourFaceMaterial && !blueprint.IsLoop)
        {
            blueprint.shape.LoopContour();
            return true;
        }
        // Notify if there's a problem with the blueprint
        else return false;
    }
}