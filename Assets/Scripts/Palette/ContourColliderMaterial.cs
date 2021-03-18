using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ContourColliderMaterial", menuName = "Contour/Material/Collider Material")]
public class ContourColliderMaterial : ContourMaterial
{
    public ColliderType type;
    public bool isTrigger;
    public PhysicMaterial physicMaterial;

    public enum ColliderType { Auto, Edge, Polygon }
}