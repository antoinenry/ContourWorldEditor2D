using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ContourShape
{
    public ShapeChanged changes;

    [SerializeField] private Point[] points;
    [SerializeField] private Vector3 normal;

    [Flags]
    public enum ShapeChanged { None = 0, PositionMoved = 1, LengthChanged = 2, NormalChanged = 4 }

    [Serializable]
    public class Point
    {
        public Vector2 position;
        public bool preventAnimation;
    }

    public ContourShape()
    {
        points = null;
        normal = Vector3.back;
    }

    #region Positions

    public int Length => points != null ? points.Length : 0;    

    public Vector2[] GetPositions()
    {
        if (points != null) return new List<Point>(points).ConvertAll(pt => pt.position).ToArray();
        else return new Vector2[0];
    }

    public Vector2 GetPosition(int index)
    {
        return points[index].position;
    }

    public void SetPosition(int index, Vector2 value)
    {
        Point pt = points[index];
        pt.position = value;
        points[index] = pt;
        changes |= ShapeChanged.PositionMoved;
    }

    public void AddPosition(Vector2 value)
    {
        List<Point> ptList = points != null ? new List<Point>(points) : new List<Point>(1);
        ptList.Add(new Point() { position = value });
        points = ptList.ToArray();
        changes |= ShapeChanged.LengthChanged;
    }

    public void InsertPosition(int index, Vector2 value)
    {
        List<Point> ptList = points != null ? new List<Point>(points) : new List<Point>(1);
        ptList.Insert(index, new Point() { position = value });
        points = ptList.ToArray();
        changes |= ShapeChanged.LengthChanged;
    }

    public void RemovePosition(int index)
    {
        if (points == null) return;
        List<Point> ptList = new List<Point>(points);
        ptList.RemoveAt(index);
        points = ptList.ToArray();
        changes |= ShapeChanged.LengthChanged;
    }

    public void RemovePositions(int index, int count)
    {
        if (points == null) return;
        List<Point> ptList = new List<Point>(points);
        ptList.RemoveRange(index, count);
        points = ptList.ToArray();
        changes |= ShapeChanged.LengthChanged;
    }

    public Vector2 GetCenter()
    {
        Vector2 center = Vector2.zero;
        if (points == null) return center;
        int length = Length;
        for (int i = 0; i < length; i++)
            center += points[i].position;
        return center / length;
    }

    public bool CanAnimatePoint(int index)
    {
        return !points[index].preventAnimation;
    }

    public void SetCanAnimatePoint(int index, bool value)
    {
        points[index].preventAnimation = !value;
    }

    public void SetCanAnimatePoints(bool value)
    {
        for(int i = 0; i < Length; i++)
            points[i].preventAnimation = !value;
    }

    #endregion

    #region Normal
    public Vector3 Normal
    {
        get
        {
            return normal.normalized;
        }
        set
        {
            normal = normal.normalized;
            if (normal != value.normalized)
            {
                normal = value.normalized;
                changes |= ShapeChanged.NormalChanged;
            }
        }
    }
    #endregion
}