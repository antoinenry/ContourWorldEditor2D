using UnityEngine;
using System.Collections.Generic;

public class ContourLineReader : ContourMeshReader
{
    public override bool TryReadBlueprint(ContourBlueprint blueprint)
    {
        // Read if possible
        if (blueprint != null && blueprint is ContourMeshBlueprint && blueprint.material is ContourLineMaterial)
        {
            ContourMeshBlueprint meshBlueprint = blueprint as ContourMeshBlueprint;
            // Get positions
            Vector2[] positions = blueprint.positions;
            if (positions == null) return false;
            // Get material
            ContourLineMaterial contourMaterial = blueprint.material as ContourLineMaterial;
            if (contourMaterial == null) return false;
            MeshMaterial = contourMaterial.meshMaterial;
            // Check if enough points to build at least one segment
            int positionCount = positions != null ? positions.Length : 0;
            if (positionCount < 2)
            {
                Clear();
                return false;
            }
            // Loop contour
            bool loopContour = positionCount >= 3 && positions[0] == positions[positionCount - 1];
            // Set vertices: two vertices per point
            int vertexCount = positionCount * 2;
            Vertices = new List<Vector3>(vertexCount);
            Vector3 zOffset = contourMaterial.zOffset * Vector3.forward;
            float halfWidth = contourMaterial.width / 2f;
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
                // Add two vertices for each side of the ribbon
                Vertices.Add((Vector3)positions[i] + median * halfWidth * witdh_correction + zOffset);
                Vertices.Add((Vector3)positions[i] - median * halfWidth * witdh_correction + zOffset);
                inDirection = outDirection;
            }
            // Set triangles: one quad per segment, two triangles per quad
            int quadCount = positionCount - 1;
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
            Vector3 normal = meshBlueprint.Normal;
            for (int i = 0; i < vertexCount; i++)
                Normals.Add(normal);
            // Set uvs: repeat texture along segments
            UVs = new List<Vector2>(vertexCount);
            float coveredLength = 0f;
            float yTop = .5f + contourMaterial.uvScale.y / 2f;
            float yBot = .5f - contourMaterial.uvScale.y / 2f;
            UVs.Add(new Vector2(0f, yTop));
            UVs.Add(new Vector2(0f, yBot));
            for (int i = 1; i < positionCount; i++)
            {
                coveredLength += Vector2.Distance(positions[i - 1], positions[i]) * contourMaterial.uvScale.x;
                UVs.Add(new Vector2(coveredLength, yTop));
                UVs.Add(new Vector2(coveredLength, yBot));
            }
            // Set colors: fetch value in blueprint
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