using System.Collections.Generic;
using UnityEngine;

public struct ContourShape
{
    private List<Vector2> positions;

    public ContourShape(List<Vector2> positions)
    {
        this.positions = positions;
    }

    public Vector2[] GetPositions()
    {
        if (positions != null) return positions.ToArray();
        else return new Vector2[0];
    }
}