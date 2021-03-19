using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ContourShape
{
    private List<Vector2> positions;

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