using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ContourBlueprint
{
    [Serializable]
    public struct Point
    { 
        public Vector2 position;
        public bool enabled;

        public Point(Vector2 pos)
        {
            position = pos;
            enabled = true;
        }
    }

    public Point[] points = new Point[0];
    public ContourMaterial material;
    public bool loop;
    public bool highlightGizmo;

    public int TotalContourLength
    { 
        get
        {
            return points != null ? points.Length : 0;
        }

        set
        {
            if (value < 0)
            {
                Debug.LogError("Can't set contour length to negative.");
                return;
            }

            if (points == null) points = new Point[value];
            else Array.Resize(ref points, value);
        }
    }
 
    public int EnabledContourLength
    {
        get
        {
            Point[] enabledPoints = GetEnabledPoints();
            return enabledPoints.Length;
        }
    }
    public void DrawGizmo(Vector3 gizmoPosition, Color color)
    {
        Point[] enabledPoints = GetEnabledPoints();
        int pointCount = enabledPoints.Length;
        Gizmos.color = color;
        if (pointCount > 1)
        {
            for (int i = 0; i < pointCount - 1; i++)
                Gizmos.DrawLine(gizmoPosition + (Vector3)points[i].position, gizmoPosition + (Vector3)points[i + 1].position);
            if (loop)
                Gizmos.DrawLine(gizmoPosition + (Vector3)points[pointCount - 1].position, gizmoPosition + (Vector3)points[0].position);
        }
    }

    public Point[] GetEnabledPoints()
    {
        if (TotalContourLength == 0) return new Point[0];
        List<Point> enabledPoints = new List<Point>(points).FindAll(pt => pt.enabled == true);
        return enabledPoints.ToArray();
    }

    public Vector2[] GetEnabledPositions()
    {
        if (TotalContourLength == 0) return new Vector2[0];
        List<Point> enabledPoints = new List<Point>(points).FindAll(pt => pt.enabled == true);
        return enabledPoints.ConvertAll(pt => pt.position).ToArray();
    }
    
    public Point[] GetDisabledPoints()
    {
        if (TotalContourLength == 0) return new Point[0];
        List<Point> disabledPoints = new List<Point>(points).FindAll(pt => pt.enabled == false);
        return disabledPoints.ToArray();
    }

    public void SetPointAt(int index, bool enabled, Vector2 position = new Vector2())
    {
        if (index < 0 || index > points.Length) return;
        points[index].position = position;
        points[index].enabled = enabled;
    }

    public void InsertPositionAt(int insertAt, bool enabled, Vector2 position)
    {
        if (insertAt < 0 || insertAt > points.Length) return;
        List<Point> currentPoints = new List<Point>(points);
        Point insertedPoint = new Point(position);
        insertedPoint.enabled = enabled;
        if (insertAt < currentPoints.Count)
            currentPoints.Insert(insertAt, insertedPoint);
        else
            currentPoints.Add(insertedPoint);
        points = currentPoints.ToArray();
    }

    public void RemovePointAt(int removeAt)
    {
        if (removeAt < 0 || removeAt >= points.Length) return;
        List<Point> currentPoints = new List<Point>(points);
        currentPoints.RemoveAt(removeAt);
        points = currentPoints.ToArray();
    }
}
