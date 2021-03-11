using System;
using UnityEngine;

[Serializable]
public class ContourMeshBlueprint : ContourBlueprint
{
    [LinkMaterialAndBlueprint("normal", "normalMode")]
    [UnitVector(autoAdjust = UnitVectorAttribute.AutoAdjustBehaviour.AdjustZ)]
    public Vector3 normal = Vector3.back;
    [LinkMaterialAndBlueprint("color", "colorMode")]
    public Color color = Color.white;
}
