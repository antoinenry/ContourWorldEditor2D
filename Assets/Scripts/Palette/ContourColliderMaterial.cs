using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ContourColliderMaterial", menuName = "Contour/Material/Collider Material")]
public class ContourColliderMaterial : ContourMaterial
{
    public enum ColliderType { Auto, Edge, Polygon }

    public ColliderType type;
    public bool isTrigger;
    public PhysicsMaterial2D physicsMaterial;

}