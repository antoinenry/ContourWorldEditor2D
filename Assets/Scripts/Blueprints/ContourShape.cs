using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ContourShape
{
    public List<Vector2> positions;
    public bool hasChanged;

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