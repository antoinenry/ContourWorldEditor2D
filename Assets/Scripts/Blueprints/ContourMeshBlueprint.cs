using System;
using UnityEngine;

[Serializable]
public class ContourMeshBlueprint : ContourBlueprint
{
    [UnitVector(autoAdjust = UnitVectorAttribute.AutoAdjustBehaviour.AdjustZ)]
    public Vector3 normal = Vector3.back;
}
