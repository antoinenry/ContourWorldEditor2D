using UnityEngine;
using System;
using System.Collections.Generic;

public class ContourLineReader : ContourMeshReader
{
    public override bool TryReadBlueprint(ContourBlueprint blueprint)
    {
        // Read if possible
        if (blueprint != null && blueprint.material is ContourLineMaterial)
        {
            ReadBlueprint(blueprint.GetEnabledPoints(), blueprint.material as ContourLineMaterial, blueprint.loop);
            return true;
        }
        // If not, return false
        return false;
    }

    private void ReadBlueprint(ContourBlueprint.Point[] unloopedPoints, ContourLineMaterial contourMaterial, bool loopContour)
    {
        // Check if enough points to build at least one segment
        base.ReadBlueprint(unloopedPoints, contourMaterial);
        if (unloopedPoints.Length < 2)
        {
            Clear();
            return;
        }
        // Loop contour
        ContourBlueprint.Point[] points;
        if (loopContour)
        {
            points = new ContourBlueprint.Point[unloopedPoints.Length + 1];
            unloopedPoints.CopyTo(points, 0);
            points[unloopedPoints.Length] = points[0];
        }
        else
            points = unloopedPoints;
        // Set vertices: two vertices per point
        int vertexCount = points.Length * 2;
        Vertices = new List<Vector3>(vertexCount);
        float halfWidth = contourMaterial.width / 2f;
        //bool loopContour = points.Length > 2 && points[0].position == points[points.Length - 1].position;
        Vector2 outDirection = (points[1].position - points[0].position).normalized;
        Vector2 inDirection = loopContour ? (points[points.Length - 1].position - points[points.Length - 2].position).normalized : outDirection; 
        for (int i = 0, iend = points.Length; i < iend; i++)
        {
            // Calculate current angle median
            if (i < iend - 1)
                outDirection = (points[i + 1].position - points[i].position).normalized;
            else
                outDirection = loopContour ? (points[1].position - points[0].position).normalized : inDirection;
            Vector2 median = Vector3.Cross(inDirection + outDirection, Vector3.forward).normalized;
            // Correct width depending on angle
            float angle_deg = Vector2.Angle(-inDirection, outDirection);
            float witdh_correction = 1f;
            if (angle_deg != 0f)
                witdh_correction = 1f / Mathf.Sin(Mathf.Deg2Rad * angle_deg / 2f);
            // Add two vertices for each side of the ribbon
            Vertices.Add(points[i].position + median * halfWidth * witdh_correction);
            Vertices.Add(points[i].position - median * halfWidth * witdh_correction);
            inDirection = outDirection;
        }
        // Set triangles: one quad per segment, two triangles per quad
        int quadCount = points.Length - 1;
        Triangles = new List<int>(quadCount * 6);
        for (int i = 0; i < quadCount; i++)
        {
            int q = i * 2;
            // First half of a quad
            Triangles.AddRange(new int[] { q, q + 1, q + 3 });
            // Second half
            Triangles.AddRange(new int[] { q, q + 3, q + 2 });
        }
        // Set normals: all to (0, 0, -1)
        Normals = new List<Vector3>(vertexCount);
        for (int i = 0; i < vertexCount; i++)
            Normals.Add(Vector3.back);
        // Set uvs: since it's 2D, use vertex positions
        UVs = new List<Vector2>(vertexCount);
        for (int i = 0; i < vertexCount; i++)
            UVs.Add(Vertices[i]);
        // Set colors: all to one color (white)
        Color color = contourMaterial.color;
        Colors = new List<Color>(vertexCount);
        for (int i = 0; i < vertexCount; i++)
            Colors.Add(color);
    }
}