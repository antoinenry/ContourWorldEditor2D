using System;
using UnityEngine;

//[DefaultExecutionOrder(1)]
//[ExecuteAlways]
[Serializable]
public class ContourBlueprint //: MonoBehaviour
{
    public ContourShape shape;
    public ContourMaterial material;
    public bool materialHasChanged;

    public Vector2[] Positions => shape != null ? shape.GetPositions() : null;
    public int ContourLength => shape != null ? shape.Length : 0;
    public bool IsLoop => shape != null && shape.IsLoop;
    public Vector3 Normal => shape != null ? shape.Normal : Vector3.zero;
    public bool IsStatic => material != null && material.IsStatic;

    //private void Update()
    //{
    //    if (shape != null) shape.changes = ContourShape.ShapeChanged.None;
    //}
}