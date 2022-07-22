using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ContourShape
{
    public ShapeChanged changes;

    [SerializeField] private ContourPoint[] points;
    [SerializeField] private Vector3 normal;

    [Flags]
    public enum ShapeChanged { None = 0, PositionMoved = 1, LengthChanged = 2, NormalChanged = 4 }       

    public ContourShape()
    {
        points = null;
        normal = Vector3.back;
    }

    #region Positions

    public int Length => points != null ? points.Length : 0;    

    public Vector2 GetPosition(int index)
    {
        return points[index].position;
    }

    public void SetPosition(int index, Vector2 position)
    {
        points[index].position = position;
        changes |= ShapeChanged.PositionMoved;
    }

    public Vector2[] GetPositions()
    {
        if (points != null) return Array.ConvertAll(points, pt=> pt.position);
        else return new Vector2[0];
    }

    public ContourPoint GetPoint(int index)
    {
        return points[index];
    }

    public void AddPoint(ContourPoint pt)
    {
        List<ContourPoint> ptList = points != null ? new List<ContourPoint>(points) : new List<ContourPoint>(1);
        ptList.Add(pt);
        points = ptList.ToArray();
        changes |= ShapeChanged.LengthChanged;
    }

    public void InsertPoint(int index, ContourPoint pt)
    {
        List<ContourPoint> ptList = points != null ? new List<ContourPoint>(points) : new List<ContourPoint>(1);
        ptList.Insert(index, pt);
        points = ptList.ToArray();
        changes |= ShapeChanged.LengthChanged;
    }

    public void ReplacePoint(int index, ContourPoint pt)
    {
        bool positionChanged = points[index].position != pt.position;
        points[index] = pt;
        if (positionChanged) changes |= ShapeChanged.PositionMoved;
    }

    public void RemovePoint(int index)
    {
        List<ContourPoint> ptList = new List<ContourPoint>(points);
        ptList.RemoveAt(index);
        points = ptList.ToArray();
        changes |= ShapeChanged.LengthChanged;
    }

    public void RemovePoints(int index, int count)
    {
        List<ContourPoint> ptList = new List<ContourPoint>(points);
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

    #region Animation
    public void SetPointsStatic(bool value)
    {
        foreach (ContourPoint pt in points) pt.isStatic = value;
    }

    public void SetPointStatic(int index, bool value)
    {
        points[index].isStatic = value;
    }

    public List<int> GetNonStaticPointsIndices()
    {
        if (points == null) return new List<int>(0);
        int length = Length;
        List<int> indices = new List<int>(length);
        for(int i = 0; i < length; i++)
            if (points[i].isStatic == false) indices.Add(i);
        return indices;
    }
    #endregion
}