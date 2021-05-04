using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[Serializable]
public class ContourBlueprint : MonoBehaviour
{
    public ContourShape shape;
    public ContourMaterial material;
    public string changedParameters;

    public Vector2[] Positions => shape != null ? shape.GetPositions() : null;

    public ContourShape.ShapeChanged ShapeChanges => shape != null ? shape.changes : ContourShape.ShapeChanged.None;
    public BlueprintChange blueprintChanges;

    [Flags]
    public enum BlueprintChange { None = 0, MaterialChanged = 1, ParameterChanged = 2 }

    private void LateUpdate()
    {
        //if (shape != null) shape.changes = ContourShape.ShapeChanged.None;
        blueprintChanges = BlueprintChange.None;
        changedParameters = "";
    }
}