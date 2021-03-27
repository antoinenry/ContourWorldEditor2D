using System;
using System.Collections.Generic;
using UnityEngine;

public class ContourColliderReader : ContourReader
{
    public List<Vector2> Positions { get; private set; }
    public Type ColliderType { get; private set; }
    public bool IsTrigger { get; private set; }
    public PhysicsMaterial2D PhysicsMaterial { get; private set; }

    public override bool TryReadBlueprint(ContourBlueprint blueprint)
    {
        // Read if possible
        if (blueprint != null && blueprint.material is ContourColliderMaterial)
        {
            // Check blueprint for positions and material
            Vector2[] positions = blueprint.positions;
            if (positions == null || positions.Length < 2 || blueprint.material == null)
            {
                Clear();
                return false;
            }
            int positionCount = positions.Length;
            // Get material
            ContourColliderMaterial contourMaterial = blueprint.material as ContourColliderMaterial;
            // Set collider type
            ColliderType = null;
            switch(contourMaterial.type)
            {
                // Edge is always possible
                case ContourColliderMaterial.ColliderType.Edge:
                    ColliderType = typeof(EdgeCollider2D);
                    break;
                // Polygon is possible if contour is a loop
                case ContourColliderMaterial.ColliderType.Polygon:
                    if (positions.Length > 2 && positions[0] == positions[positionCount - 1]) ColliderType = typeof(PolygonCollider2D);
                    break;
                case ContourColliderMaterial.ColliderType.Auto:
                    ColliderType = (positionCount > 2 && positions[0] == positions[positionCount - 1]) ? typeof(PolygonCollider2D) : typeof(EdgeCollider2D);
                    break;
            }
            if (ColliderType == null)
            {
                Clear();
                return false;
            }
            // Get positions
            Positions = new List<Vector2>(blueprint.positions);
            if (ColliderType == typeof(PolygonCollider2D)) Positions.RemoveAt(positionCount - 1);
            // Get collider parameters
            IsTrigger = contourMaterial.isTrigger;
            PhysicsMaterial = contourMaterial.physicsMaterial;
        }
        return false;
    }

    public void Clear()
    {
        Positions = null;
        ColliderType = null;
    }
}