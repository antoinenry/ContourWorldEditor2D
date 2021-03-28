using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ContourShape
{
    public List<Vector2> positions;
    public ShapeChange changes;

    [Flags]
    public enum ShapeChange { None = 0, PositionMoved = 1, LengthChanged = 2 }

    public ContourShape(List<Vector2> positions)
    {
        this.positions = positions;
    }

    public Vector2[] GetPositions()
    {
        if (positions != null) return positions.ToArray();
        else return null;
    }
}