using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class ContourBuilder : MonoBehaviour
{
    protected List<ContourReader> readers;

    public static ContourBuilder NewBuilder(ContourReader reader, Transform setParent = null)
    {
        // Create new gameobject
        GameObject builderGO = new GameObject("Contour builder");
        builderGO.transform.SetParent(setParent, false);
        // Add builder component according to reader type and return it
        if(reader != null)
        {
            if (reader is ContourMeshReader)
            {
                builderGO.name = "Mesh builder";
                return builderGO.AddComponent<ContourMeshBuilder>();
            }
            if (reader is ContourColliderReader)
            {
                ContourColliderReader colliderReader = reader as ContourColliderReader;
                builderGO.name = "Collider builder";
                Type colliderType = colliderReader.ColliderType;
                if (colliderType != null && typeof(Collider2D).IsAssignableFrom(colliderType))
                {
                    Collider2D colliderComponent = builderGO.AddComponent(colliderType) as Collider2D;
                    colliderComponent.isTrigger = colliderReader.IsTrigger;
                }
                return builderGO.AddComponent<ContourColliderBuilder>();
            }
        }
        // If add component has failed, cancel gameobject creation and return null
        DestroyImmediate(builderGO);
        return null;
    }

    public void Reset()
    {
        if (readers != null) readers.Clear();
    }

    public bool TryAddReader(ContourReader reader)
    {
        if (CanBuildFrom(reader))
        {
            // Add reader to existing list (if not already in the list) or create a new list
            if (readers == null) readers = new List<ContourReader>() { reader };
            else if (!readers.Contains(reader)) readers.Add(reader);
            return true;
        }
        else
            return false;
    }

    public abstract bool CanBuildFrom(ContourReader reader);

    public abstract void Build();
}
