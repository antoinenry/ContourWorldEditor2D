using System;
using System.Collections.Generic;
using UnityEngine;

public class ContourColliderBuilder : ContourBuilder
{
    [SerializeField]
    new private Collider2D collider2D;

    [Flags]
    private enum UpdateType { None = 0, All = ~0, Positions = 2 }

    //public override void Build()
    //{
    //    int bpCount = blueprints != null ? blueprints.Count : 0;
    //    if (readers == null || readers.Count != blueprints.Count) ResetReaders();
    //    // Optimized update
    //    UpdateType requiredUpdates = UpdateType.None;
    //    for (int i = 0; i < bpCount; i++)
    //    {
    //        ContourBlueprint bp = blueprints[i];
    //        ContourReader rd = readers[i];
    //        if (bp == null || rd == null) continue;
    //        ContourBlueprint.BlueprintChanges bpChanges = bp.changes;
    //        if (bpChanges != ContourBlueprint.BlueprintChanges.None)
    //        {
    //            if (bpChanges.HasFlag(ContourBlueprint.BlueprintChanges.LengthChanged))
    //            {
    //                rd.TryReadBlueprint(bp);
    //                requiredUpdates = UpdateType.All;
    //            }
    //            if (bpChanges.HasFlag(ContourBlueprint.BlueprintChanges.PositionMoved))
    //            {
    //                rd.ReadBlueprintPositions(bp);
    //                requiredUpdates |= UpdateType.Positions;
    //            }
    //            bp.changes = ContourBlueprint.BlueprintChanges.None;
    //        }
    //    }
    //    // Apply required updates
    //    if (requiredUpdates == UpdateType.All)
    //        RebuildAll();
    //    else
    //    {
    //        if (requiredUpdates.HasFlag(UpdateType.Positions)) UpdatePositions();
    //    }
    //}

    public override void RebuildAll()
    {
        // Reread all blueprints
        ResetReaders();
        // Set collider shape
        UpdatePositions();
        // Set collider parameters (all readers should have the same parameter so we get values from the first one)
        if (readers != null && readers.Count > 0 && readers[0] != null)
        {
            ContourColliderReader topReader = readers[0] as ContourColliderReader;
            collider2D.isTrigger = topReader.IsTrigger;
            collider2D.sharedMaterial = topReader.PhysicsMaterial;
        }
    }

    protected override bool CanBuildFrom(ContourReader reader)
    {
        if (reader == null || reader is ContourColliderReader == false) return false;
        if (collider2D == null) return true;
        ContourColliderReader colliderReader = reader as ContourColliderReader;
        return (colliderReader.ColliderType == collider2D.GetType() && colliderReader.IsTrigger == collider2D.isTrigger);
    }

    public override bool TryAddBlueprint(ContourBlueprint bp)
    {
        if (base.TryAddBlueprint(bp))
        {
            UpdateColliderComponent();
            return collider2D != null;
        }
        else
            return false;
    }

    protected override void UpdatePositions()
    {
        // Set collider shape
        if (collider2D is EdgeCollider2D)
        {
            // Edge collider: can build only one contour (one continuous line)
            EdgeCollider2D edgeCollider = collider2D as EdgeCollider2D;
            if (readers.Count == 1 && readers[0] is ContourColliderReader)
            {
                ContourColliderReader colliderReader = readers[0] as ContourColliderReader;
                edgeCollider.SetPoints(colliderReader.Positions);
            }
            else
                edgeCollider.SetPoints(new List<Vector2>());
        }
        else if (collider2D is PolygonCollider2D)
        {
            // Polygon collider: can build several contours (but only loops)
            PolygonCollider2D polygonCollider = collider2D as PolygonCollider2D;
            int readerCount = readers.Count;
            polygonCollider.pathCount = readerCount;
            for (int ri = 0; ri < readerCount; ri++)
            {
                if (readers[ri] != null && readers[ri] is ContourColliderReader)
                {
                    ContourColliderReader colliderReader = readers[ri] as ContourColliderReader;
                    polygonCollider.SetPath(ri, colliderReader.Positions);
                }
                else
                    polygonCollider.SetPath(ri, new Vector2[0]);
            }
        }
    }

    private void UpdateColliderComponent()
    {
        // Create or get collider component with correct type
        ContourColliderReader colliderReader = (readers != null && readers.Count > 0) ? readers[0] as ContourColliderReader : null;
        Type colliderType = colliderReader != null ? colliderReader.ColliderType : null;
        // Delete existing collider if it doesn't match blueprint's needs
        if (collider2D != null)
        {
            if (collider2D.GetType() != colliderType)
            {
                DestroyImmediate(collider2D);
                collider2D = null;
            }
        }
        // Add collider if needed
        if (collider2D == null)
        {
            if (colliderType != null && typeof(Collider2D).IsAssignableFrom(colliderType))
                collider2D = gameObject.AddComponent(colliderType) as Collider2D;
        }
        // Set collider parameters
        if (collider2D != null)
        {
            collider2D.isTrigger = colliderReader.IsTrigger;
        }
    }
}