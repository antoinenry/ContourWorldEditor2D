using UnityEngine;

public abstract class ContourMeshMaterial : ContourMaterial
{
    public Color color = Color.white;
    public Material meshMaterial;
    public Vector2 uvScale = Vector2.one;
    public float zOffset = 0f;
}
