using System.Collections.Generic;
using UnityEngine;

public class ContourColliderBuilder : ContourBuilder
{
    new private Collider2D collider2D;

    public override void Build()
    {
        if (readers == null || collider2D == null)
            return;
        // Set collider shape
        SetColliderPoints();
        // Set collider parameters (all readers should have the same parameter so we get values from the first one)
        if (readers.Count > 0 && readers[0] != null)
        {
            ContourColliderReader topReader = readers[0] as ContourColliderReader;
            collider2D.isTrigger = topReader.IsTrigger;
            collider2D.sharedMaterial = topReader.PhysicsMaterial;
        }
    }

    public override bool CanBuildFrom(ContourReader reader)
    {
        if (collider2D == null)
            collider2D = GetComponent<Collider2D>();
        if (reader != null && reader is ContourColliderReader && collider2D != null)
        {
            ContourColliderReader colliderReader = reader as ContourColliderReader;
            return (colliderReader.ColliderType == collider2D.GetType() && colliderReader.IsTrigger == collider2D.isTrigger);
        }
        return false;
    }

    protected override void OnMovePositions()
    {
        SetColliderPoints();
    }

    private void SetColliderPoints()
    {
        // Set collider shape
        if (collider2D is EdgeCollider2D)
        {
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
}