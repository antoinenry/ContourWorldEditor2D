﻿using UnityEngine;
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
        // Set vertices: copy positions x,y and set z to value in contourMaterial
        int vertexCount = positionCount;
        Vector3 zOffset = contourMaterial.zOffset * Vector3.forward;
        if (positions[0] == positions[positionCount-1]) vertexCount--;
        Vertices = new List<Vector3>(vertexCount);
        for (int i = 0; i < vertexCount; i++)
            Vertices.Add((Vector3)positions[i] + zOffset);
        // Set triangles: simple convex triangulation
        int triangleCount = Mathf.Max(vertexCount - 2, 0);
        Triangles = new List<int>(triangleCount * 3);
        for (int i = 0; i < triangleCount; i++)
            Triangles.AddRange(new int[] { 0, i + 1, i + 2 });
        // Set normals: all to random vector
        Normals = new List<Vector3>(vertexCount);
        Vector3 randomNormal = Vector3.back;
        randomNormal.x = Random.Range(-1f, 1f);
        randomNormal.y = Random.Range(-1f, 1f);
        for (int i = 0; i < vertexCount; i++)
            Normals.Add(randomNormal);
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