using System;
using UnityEngine;

[Serializable]
public class ContourBlueprint : ScriptableObject
{
    public Vector2[] positions;
    public ContourMaterial material;
}