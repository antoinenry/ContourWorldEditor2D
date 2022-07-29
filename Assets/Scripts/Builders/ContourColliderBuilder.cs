using System;
using System.Collections.Generic;
using UnityEngine;

public class ContourColliderBuilder : ContourBuilder
{
    [Serializable]
    private class SubColliderBuilder
    {
        public Collider2D collider;
        public List<ContourColliderReader> readers;

        public Type ColliderType => collider != null ? collider.GetType() : null;
        public bool IsTrigger  => collider != null ? collider.isTrigger : false;

        public PhysicsMaterial2D PhysicMaterial => collider != null ? collider.sharedMaterial : null;

        public SubColliderBuilder(ContourColliderReader reader, ContourColliderBuilder overBuilder)
        {
            if(reader != null && overBuilder != null)
            {
                readers = new List<ContourColliderReader>() { reader };
                if (reader.ColliderType != null)
                {
                    collider = overBuilder.gameObject.AddComponent(reader.ColliderType) as Collider2D;
                    collider.isTrigger = reader.IsTrigger;
                    collider.sharedMaterial = reader.PhysicsMaterial;
                }
            }
        }

        public bool ColliderIsCompatibleWith(ContourColliderReader reader)
        {
            if (collider != null)
            {
                return (reader != null
                    && reader.ReadSuccess
                    && ColliderType == reader.ColliderType
                    && IsTrigger == reader.IsTrigger
                    && PhysicMaterial == reader.PhysicsMaterial);
            }
            else return false;
        }

        public bool CanAddReader(ContourColliderReader reader)
        {
            // Check general compatibility
            if (!ColliderIsCompatibleWith(reader))
                return false;
            // Exception for edge collider: can only create one contour
            if (ColliderType == typeof(EdgeCollider2D))
                return readers == null || readers.Count == 0;
            else
                return true;
        }

        public bool TrySetColliderPoints()
        {
            int readerCount = readers != null ? readers.Count : 0;
            if (readerCount == 0 || collider == null)
                return false;
            if (collider is PolygonCollider2D)
            {
                PolygonCollider2D polygon = collider as PolygonCollider2D;
                polygon.pathCount = readerCount;
                for (int i = 0; i < readerCount; i++)
                {
                    if (ColliderIsCompatibleWith(readers[i]))
                    {
                        List<Vector2> positions = readers[i] != null ? readers[i].Points : null;
                        polygon.SetPath(i, positions != null ? positions.ToArray() : new Vector2[0]);
                    }
                    else
                        return false;
                }
                return true;
            }
            else if (collider is EdgeCollider2D)
            {
                EdgeCollider2D edge = collider as EdgeCollider2D;
                // Edge collider can only build one contour
                if (readerCount == 1 && ColliderIsCompatibleWith(readers[0]))
                {
                    List<Vector2> positions = readers[0] != null ? readers[0].Points : new List<Vector2>(0);
                    edge.SetPoints(positions);
                    return true;
                }
                else
                    return false;
            }
            else return false;
        }
    }

    private List<SubColliderBuilder> subBuilders;

    public override void RebuildAll()
    {
        ResetReaders();
        // Find all collider components
        List<Collider2D> unusedColliders = new List<Collider2D>();
        GetComponents(unusedColliders);
        // Reset subbuilders
        subBuilders = new List<SubColliderBuilder>();
        if (readers != null)
        {
            foreach (ContourColliderReader reader in readers)
            {
                if (reader == null || !reader.ReadSuccess) continue;
                int subBuilderIndex = subBuilders.FindIndex(sub => sub.CanAddReader(reader));
                SubColliderBuilder subBuilderMatch;
                if (subBuilderIndex == -1)
                {
                    subBuilderMatch = new SubColliderBuilder(reader, this);
                    subBuilders.Add(subBuilderMatch);
                }
                else
                {
                    subBuilderMatch = subBuilders[subBuilderIndex];
                    if (subBuilderMatch.readers == null) subBuilderMatch.readers = new List<ContourColliderReader>(1);
                    subBuilderMatch.readers.Add(reader);
                }
                unusedColliders.Remove(subBuilderMatch.collider);
            }
        }
        // Remove unused collider components
        foreach (Component c in unusedColliders)
            DestroyImmediate(c);
        // Set collider points
        foreach (SubColliderBuilder sub in subBuilders)
            if (sub.TrySetColliderPoints() == false)
                Debug.LogError("Could not set collider points");
    }

    protected override bool CanBuildFrom(ContourReader reader)
    {
        return reader != null && reader is ContourColliderReader && reader.ReadSuccess;       
    }

    protected override void UpdatePositions()
    {
        if (readers == null || readers.Contains(null) || subBuilders == null)
            RebuildAll();
        else if (subBuilders != null)
        {
            foreach (SubColliderBuilder sub in subBuilders)
            {
                if (sub == null || sub.TrySetColliderPoints() == false)
                    RebuildAll();
            }
        }      
    }
}