using UnityEngine;
using System;
using System.Collections.Generic;

[DefaultExecutionOrder(0)]
[ExecuteAlways]
public abstract class ContourBuilder : MonoBehaviour
{
    [SerializeField] protected List<ContourBlueprint> blueprints;
    [SerializeField] protected List<ContourReader> readers;

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
            builderGO.name = reader.BuilderName;
            newBuilder = builderGO.AddComponent(reader.BuilderType) as ContourBuilder;
        }
        // If add component has failed, cancel gameobject creation and return null
        if (newBuilder == null) DestroyImmediate(builderGO);
        else newBuilder.TryAddBlueprint(blueprint);
        return newBuilder;
    }

    public void Reset()
    {
        blueprints = new List<ContourBlueprint>();
        readers = new List<ContourReader>();
        RebuildAll();
    }

    public void Update()
    {
        Build();
    }

    public void LateUpdate()
    {
        // Every changes in blueprints should have been adressed => reset change flags
        if (blueprints != null)
        {
            foreach (ContourBlueprint bp in blueprints)
            {
                if (bp.shape != null) bp.shape.changes = ContourShape.Changes.None;
                bp.materialHasChanged = false;
            }
        }
    }

    public virtual void Build()
    {
        // Default update
        // can be further optimized in children classes
        if (blueprints != null)
        {
            int blueprintCount = blueprints.Count;
            if (readers == null || readers.Count != blueprintCount) ResetReaders();
            for (int bpi = 0; bpi < blueprintCount; bpi++)
            {
                ContourBlueprint bp = blueprints[bpi];
                if (bp == null)
                {
                    readers[bpi] = null;
                }
                else if (bp.materialHasChanged)// || readers[bpi] == null)
                {
                    RebuildAll();
                }
                else
                {
                    if (bp.shape != null)
                    {
                        ContourShape.Changes shapeChanges = bp.shape.changes;
                        if (shapeChanges != 0)
                        {
                            if (shapeChanges.HasFlag(ContourShape.Changes.LengthChanged))
                            {
                                RebuildAll();
                            }
                            else if (shapeChanges.HasFlag(ContourShape.Changes.PositionMoved))
                            {
                                bool canReadPositions = readers[bpi] != null && readers[bpi].ReadBlueprintPositions(bp);
                                if (canReadPositions) UpdatePositions();
                                else
                                {
                                    // If there's a problem with blueprints positions, cancel everything and rebuild from scratch
                                    RebuildAll();
                                    return;
                                }
                            }
                            if (shapeChanges.HasFlag(ContourShape.Changes.NormalChanged))
                            {
                                readers[bpi].ReadBlueprintNormal(bp);
                                UpdateNormals();
                            }
                        }
                    }
                }
            }
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
                blueprints = new List<ContourBlueprint>() { bp };
            else if (!blueprints.Contains(bp))
                blueprints.Add(bp);
            // Update readers accordingly
            int bpCount = blueprints.Count;
            if (readers != null && readers.Count == bpCount - 1) readers.Add(newReader);
            else ResetReaders();
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
            blueprints.RemoveAll(bp => bp == null);
            int bpCount = blueprints.Count;
            readers = new List<ContourReader>(bpCount);
            for (int i = 0; i < bpCount; i++)
            {
                ContourReader newReader = ContourReader.NewReader(blueprints[i]);
                if (CanBuildFrom(newReader))
                    readers.Add(newReader);
                else
                    readers.Add(null);
            }
        }
    }

    public abstract void RebuildAll();

    protected abstract bool CanBuildFrom(ContourReader reader);

    protected virtual void UpdatePositions() { }

    protected virtual void UpdateNormals() { }

    public virtual void OnDrawGizmosSelected()
    {
        if (blueprints != null && readers != null)
        {
            int bpCount = blueprints.Count;
            for (int i = 0; i < bpCount; i++)
            {
                ContourBlueprint bp = blueprints[i];
                if (readers == null || readers.Count <= i)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = (readers[i] != null && readers[i].ReadSuccess) ? Color.white : Color.red;
                if (bp != null && bp.shape != null) bp.shape.DrawGizmo(transform);
            }
        }
    }
}
