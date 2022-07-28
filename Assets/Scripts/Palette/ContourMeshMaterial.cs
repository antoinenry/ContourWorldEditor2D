using System;
using UnityEngine;

public abstract class ContourMeshMaterial : ContourMaterial
{
    public Material meshMaterial;
    public Color color = Color.white;
    public BlueprintMode colorMode;
    public Vector3 normal = Vector3.back;
    public BlueprintMode normalMode;
    public Vector2 uvScale = Vector2.one;
    public float zOffset = 0f;

    //public override Type BlueprintType => typeof(ContourMeshBlueprint);
}
