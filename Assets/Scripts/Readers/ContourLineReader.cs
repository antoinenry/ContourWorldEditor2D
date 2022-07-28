using UnityEngine;
using System;
using System.Collections.Generic;

public class ContourLineReader : ContourMeshReader
{
    public override bool ReadSuccess => true;

    public override bool CanReadBlueprint(ContourBlueprint blueprint)
    {
        return blueprint != null && blueprint.material != null && blueprint.material is ContourLineMaterial && blueprint.ContourLength > 1;
    }

    public override bool TryReadBlueprint(ContourBlueprint blueprint)
    {
        // Read if possible
        if (CanReadBlueprint(blueprint))
        {
            // Get positions and material
            Vector2[] positions = blueprint.Positions;
            ContourLineMaterial contourMaterial = blueprint.material as ContourLineMaterial;
            MeshMaterial = contourMaterial.meshMaterial;
            // Set vertices: two vertices per point
            int vertexCount = blueprint.ContourLength * 2;
            Vertices = new List<Vector3>(new Vector3[vertexCount]);
            GenerateVertices(positions, contourMaterial.zOffset, contourMaterial.width);
            // Set triangles: one quad per segment, two triangles per quad
            int quadCount = blueprint.ContourLength - 1;
            Triangles = new List<int>(quadCount * 6);
            for (int i = 0; i < quadCount; i++)
            {
                int q = i * 2;
                // First half of a quad
                Triangles.AddRange(new int[] { q, q + 1, q + 3 });
                // Second half
                Triangles.AddRange(new int[] { q, q + 3, q + 2 });
            }
            // Set normals: fetch value in blueprint
            Normals = new List<Vector3>(vertexCount);
            Vector3 normal = blueprint.Normal;
            for (int i = 0; i < vertexCount; i++)
                Normals.Add(normal);
            // Set uvs: repeat texture along segments
            GenerateUVs(positions, contourMaterial.uvScale);
            // Set colors: fetch value in blueprint
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
            int vertexCount = Vertices.Count;
            if (vertexCount == positionCount * 2)
            {
                if (vertexCount > 0)
                {
                    ContourLineMaterial contourMaterial = blueprint.material as ContourLineMaterial;
                    GenerateVertices(positions, contourMaterial.zOffset, contourMaterial.width);
                    GenerateUVs(positions, contourMaterial.uvScale);
                }
                return true;
            }
            else return false;
        }
        // Notify if there's a problem with the blueprint
        else return false;
    }

    private void GenerateVertices(Vector2[] positions, float zOffset, float width)
    {
        // Set vertices: two vertices per point
        int positionCount = positions.Length;
        bool loopContour = positionCount >= 3 && positions[0] == positions[positionCount - 1];
        float halfWidth = width / 2f;
        Vector2 outDirection = (positions[1] - positions[0]).normalized;
        Vector2 inDirection = loopContour ? (positions[0] - positions[positionCount - 2]).normalized : outDirection;
        for (int i = 0; i < positionCount; i++)
        {
            // Calculate current angle median
            if (i < positionCount - 1)
                outDirection = (positions[i + 1] - positions[i]).normalized;
            else
                outDirection = loopContour ? (positions[1] - positions[0]).normalized : inDirection;
            Vector3 median = Vector3.Cross(inDirection + outDirection, Vector3.forward).normalized;
            // Correct width depending on angle
            float angle_deg = Vector2.Angle(-inDirection, outDirection);
            float witdh_correction = 1f;
            if (angle_deg != 0f)
                witdh_correction = 1f / Mathf.Sin(Mathf.Deg2Rad * angle_deg / 2f);
            // Set two vertices for each side of the ribbon
            Vertices[2 * i] = (Vector3)positions[i] + median * halfWidth * witdh_correction + zOffset * Vector3.forward;
            Vertices[2 * i + 1] = (Vector3)positions[i] - median * halfWidth * witdh_correction + zOffset * Vector3.forward;
            inDirection = outDirection;
        }
    }

    private void GenerateUVs(Vector2[] positions, Vector2 uvScale)
    {
        // Set uvs: repeat texture along segments
        int positionCount = positions.Length;
        UVs = new List<Vector2>(positionCount * 2);
        // Line width
        float yTop = .5f + uvScale.y / 2f;
        float yBot = .5f - uvScale.y / 2f;
        // Start at U = 0
        UVs.Add(new Vector2(0f, yTop));
        UVs.Add(new Vector2(0f, yBot));
        // Unfold texture along positions
        float coveredLength = 0f;
        for (int i = 1; i < positionCount; i++)
        {
            coveredLength += Vector2.Distance(positions[i - 1], positions[i]) * uvScale.x;
            UVs.Add(new Vector2(coveredLength, yTop));
            UVs.Add(new Vector2(coveredLength, yBot));
        }
    }
}