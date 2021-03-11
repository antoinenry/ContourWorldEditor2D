﻿using UnityEngine;
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
            // Check if enough points to build at least one triangle
            int positionCount = positions != null ? positions.Length : 0;
            if (positionCount < 3)
            {
                Clear();
                return false;
            }
            // Set vertices: copy positions x,y and set z to value in contourMaterial
            int vertexCount = positionCount;
            Vector3 zOffset = contourMaterial.zOffset * Vector3.forward;
            if (positions[0] == positions[positionCount - 1]) vertexCount--;
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
            Vector3 normal = meshBlueprint.normal;
            for (int i = 0; i < vertexCount; i++)
                Normals.Add(normal);
            // Set uvs: since it's 2D, use vertex positions
            UVs = new List<Vector2>(vertexCount);
            for (int i = 0; i < vertexCount; i++)
                UVs.Add(Vertices[i]);
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
}