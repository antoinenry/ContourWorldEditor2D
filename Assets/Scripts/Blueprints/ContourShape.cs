using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ContourShape
{
    public ShapeChanged changes;

    [SerializeField] private List<Vector2> positions;

    [Flags]
    public enum ShapeChanged { None = 0, PositionMoved = 1, LengthChanged = 2 }

    public ContourShape(List<Vector2> positions)
    {
        this.positions = positions;
    }

    public int Length => positions != null ? positions.Count : 0;    

    public Vector2[] GetPositions()
    {
        if (positions != null) return positions.ToArray();
        else return new Vector2[0];
    }

    public Vector2 GetPosition(int index)
    {
        return positions[index];
    }

    public void SetPosition(int index, Vector2 value)
    {
        positions[index] = value;
        changes |= ShapeChanged.PositionMoved;
    }

    public void AddPosition(Vector2 value)
    {
        positions.Add(value);
        changes |= ShapeChanged.LengthChanged;
    }

    public void InsertPosition(int index, Vector2 value)
    {
        positions.Insert(index, value);
        changes |= ShapeChanged.LengthChanged;
    }

    public void RemovePosition(int index)
    {
        positions.RemoveAt(index);
        changes |= ShapeChanged.LengthChanged;
    }

    public void RemovePositions(int index, int count)
    {
        positions.RemoveRange(index, count);
        changes |= ShapeChanged.LengthChanged;
    }

    public Vector2 GetCenter()
    {
        Vector2 center = Vector2.zero;
        if (positions == null) return center;
        int length = Length;
        for (int i = 0; i < length; i++)
            center += positions[i];
        return center / length;
    }
}