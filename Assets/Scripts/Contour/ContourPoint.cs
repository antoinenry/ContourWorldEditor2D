using System;
using UnityEngine;

[Serializable]
public class ContourPoint
{
    public Vector2 position;
    public bool isStatic;

    public ContourPoint(Vector2 pos)
    {
        position = pos;
    }
}