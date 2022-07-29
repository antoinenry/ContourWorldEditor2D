using System;
using System.Collections.Generic;
using UnityEngine;

public class ContourColliderReader : ContourReader
{
    public List<Vector2> Points { get; private set; }
    public Type ColliderType { get; private set; }
    public bool IsTrigger { get; private set; }
    public PhysicsMaterial2D PhysicsMaterial { get; private set; }

    public override Type BuilderType => typeof(ContourColliderBuilder);

    public override string BuilderName => "Collider Builder";

    public override bool ReadSuccess
    {
        get
        {
            if (ColliderType == null)
                return false;
            else if (ColliderType == typeof(PolygonCollider2D))
            {
                int pointCount = Points != null ? Points.Count : 0;
                return pointCount == 0 || Points[0] == Points[pointCount - 1];
            }
            else
                return true;
        }
    }

public override bool CanReadBlueprint(ContourBlueprint blueprint)
    {
        return blueprint != null && blueprint.material != null && blueprint.material is ContourColliderMaterial && blueprint.ContourLength > 1;
    }

    public override bool TryReadBlueprint(ContourBlueprint blueprint)
    {
        // Read if possible
        if (CanReadBlueprint(blueprint))
        {
            // Set collider type
            ColliderType = GetColliderType(blueprint);
            if (ColliderType == null)
            {
                Clear();
                return false;
            }
            // Get positions
            Points = new List<Vector2>(blueprint.Positions);
            // For polygon colliders, contour must be a loop
            if (ColliderType == typeof(PolygonCollider2D) && Points.Count > 1)
            {
                if (!blueprint.IsLoop)
                {
                    Clear();
                    return false;
                }
            }
            // Get collider parameters
            ContourColliderMaterial contourMaterial = blueprint.material as ContourColliderMaterial;
            IsTrigger = contourMaterial.isTrigger;
            PhysicsMaterial = contourMaterial.physicsMaterial;
            // Success
            return true;
        }
        // Failed
        return false;
    }

    public override bool ReadBlueprintPositions(ContourBlueprint blueprint)
    {
        // Read contour positions
        if (CanReadBlueprint(blueprint))
        {
            // Quick compatibility check with collider infos
            ContourColliderMaterial contourMaterial = blueprint.material as ContourColliderMaterial;
            if (contourMaterial.isTrigger != IsTrigger
                || contourMaterial.physicsMaterial != PhysicsMaterial
                || GetColliderType(blueprint) != ColliderType)
            {
                // If compatibility check fails, attempt to completely reread blueprint
                bool reread = TryReadBlueprint(blueprint);
                if (reread == false) return false;
            }
            else
            {
                int colliderLength = Points != null ? Points.Count : 0;
                // Polygon collider are loops and don't need to repeat the last position
                if (ColliderType == typeof(PolygonCollider2D)) colliderLength -= 1;
                for (int i = 0; i < colliderLength; i++)
                    Points[i] = blueprint.Positions[i];
            }
            return true;
        }
        // Notify if there's a problem with the blueprint
        else return false;
    }

    private Type GetColliderType(ContourBlueprint blueprint)
    {
        Type colliderType = null;
        if (CanReadBlueprint(blueprint))
        {
            ContourColliderMaterial contourMaterial = blueprint.material as ContourColliderMaterial;
            switch (contourMaterial.type)
            {
                // Edge is always possible
                case ContourColliderMaterial.ColliderType.Edge:
                    colliderType = typeof(EdgeCollider2D);
                    break;
                // Polygon is possible if contour is a loop
                case ContourColliderMaterial.ColliderType.Polygon:
                    if (blueprint.IsLoop) colliderType = typeof(PolygonCollider2D);
                    break;
                case ContourColliderMaterial.ColliderType.Auto:
                    colliderType = blueprint.IsLoop ? typeof(PolygonCollider2D) : typeof(EdgeCollider2D);
                    break;
            }
        }
        return colliderType;
    }

    public override bool Clear()
    {
        if (Points == null && ColliderType == null)
            return false;
        else
        {
            Points = null;
            ColliderType = null;
            return true;
        }
    }
}