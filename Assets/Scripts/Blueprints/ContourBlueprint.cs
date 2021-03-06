﻿using System;
using UnityEngine;

[DefaultExecutionOrder(1)]
[ExecuteAlways]
[Serializable]
public class ContourBlueprint : MonoBehaviour
{
    public ContourShape shape;
    public string changedParameters; 
    public ContourMaterial material;

    public Vector2[] Positions => shape != null ? shape.GetPositions() : null;

    public ContourShape.ShapeChanged ShapeChanges => shape != null ? shape.changes : ContourShape.ShapeChanged.None;
    public BlueprintChange blueprintChanges;

    [Flags]
    public enum BlueprintChange { None = 0, MaterialChanged = 1, ParameterChanged = 2 }

    private void Update()
    {
        if (shape != null) shape.changes = ContourShape.ShapeChanged.None;
        blueprintChanges = BlueprintChange.None;
        changedParameters = "";
    }

    protected virtual void SetShape(ContourShape shape)
    {
        this.shape = shape;
    }
}