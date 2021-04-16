using UnityEngine;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
public abstract class ContourBuilder : MonoBehaviour
{
    public List<ContourBlueprint> blueprints; 
    protected List<ContourReader> readers;

    public static ContourBuilder NewBuilder(ContourBlueprint blueprint, Transform setParent = null)
    {
        // Create new gameobject
        GameObject builderGO = new GameObject("Contour builder");
        builderGO.transform.SetParent(setParent, false);
        // Add builder component according to reader type and return it
        ContourBuilder newBuilder = null;
        if(blueprint != null)
        {
            ContourReader reader = ContourReader.NewReader(blueprint);
            if (reader is ContourMeshReader)
            {
                builderGO.name = "Mesh builder";
                newBuilder = builderGO.AddComponent<ContourMeshBuilder>();
            }
            if (reader is ContourColliderReader)
            {
                builderGO.name = "Collider builder";
                newBuilder = builderGO.AddComponent<ContourColliderBuilder>();
            }
        }
        // If add component has failed, cancel gameobject creation and return null
        if (newBuilder == null) DestroyImmediate(builderGO);
        else newBuilder.TryAddBlueprint(blueprint);
        return newBuilder;
    }

    public void Reset()
    {
        if (readers != null) readers.Clear();
    }

    public virtual void Update()
    {
        // Default update: rebuild all for any change in blueprints
        // can be further optimized in children classes
        if (blueprints != null)
        {
            int blueprintCount = blueprints.Count;
            if (readers == null || readers.Count != blueprintCount) ResetReaders();
            bool rebuild = false;
            for (int bpi = 0; bpi < blueprintCount; bpi++)
            {
                ContourBlueprint bp = blueprints[bpi];
                if (bp == null)
                {
                    readers[bpi] = null;
                }
                else if (readers[bpi] == null)
                {
                    readers[bpi] = ContourReader.NewReader(bp);
                }
                else if (bp.changes != ContourBlueprint.BlueprintChanges.None)
                {
                    ContourReader rd = readers[bpi];
                    bool rdCanReadBp = rd.TryReadBlueprint(bp);
                    if (rdCanReadBp)
                    {
                        rebuild = true;
                        bp.changes = ContourBlueprint.BlueprintChanges.None;
                        bp.changedParameters = "";
                    }
                    else
                    {
                        readers[bpi] = ContourReader.NewReader(bp);
                    }
                }
            }
            if (rebuild) RebuildAll();
        }
    }

    public virtual bool TryAddBlueprint(ContourBlueprint bp)
    {
        if (bp == null) return false;
        ContourReader newReader = ContourReader.NewReader(bp);
        if (CanBuildFrom(newReader))
        {
            // Add blueprint to existing list if not already in the list and if it is readable or create a new list
            if (blueprints == null)
            {
                blueprints = new List<ContourBlueprint>() { bp };
                readers = new List<ContourReader>() { newReader };
            }
            else if (!blueprints.Contains(bp))
            {
                int bpCount = blueprints.Count;
                blueprints.Add(bp);
                if (readers == null || readers.Count != bpCount) ResetReaders();
                else readers.Add(newReader);
            }
            return true;
        }
        else
            return false;
    }

    public void ResetReaders()
    {
        if (blueprints == null) readers = null;
        else
        {
            int bpCount = blueprints.Count;
            readers = new List<ContourReader>(bpCount);
            for (int i = 0; i < bpCount; i++)
                readers.Add(ContourReader.NewReader(blueprints[i]));
        }
    }

    public abstract void RebuildAll();

    protected abstract bool CanBuildFrom(ContourReader reader);

    protected abstract void UpdatePositions();

    protected virtual void OnChangeBlueprintParameters()
    {
        return;
    }
}
