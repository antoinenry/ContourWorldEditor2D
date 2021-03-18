using System.Collections;
using UnityEngine;

public class ContourColliderBuilder : ContourBuilder
{
    new private Collider2D collider2D;

    public override void Build()
    {

    }

    public override bool CanBuildFrom(ContourReader reader)
    {
        if (collider2D == null)
            collider2D = GetComponent<Collider2D>();
        if (reader != null && reader is ContourColliderReader && collider2D != null)
        {
            ContourColliderReader colliderReader = reader as ContourColliderReader;
            return (colliderReader.ColliderType == collider2D.GetType());
        }
        return false;
    }
}