using UnityEngine;
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
        if (readers != null) readers.Clear();
    }

    private void Update()
    {
        Build();
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
                else if (readers[bpi] == null)
                {
                    readers[bpi] = ContourReader.NewReader(bp);
                }
                else
                {
                    ContourShape.ShapeChanged shapeChanges = bp.ShapeChanges;
                    if (shapeChanges != ContourShape.ShapeChanged.None)
                    {
                        if (shapeChanges.HasFlag(ContourShape.ShapeChanged.LengthChanged))
                        {
                            RebuildAll();
                        }
                        else if (shapeChanges.HasFlag(ContourShape.ShapeChanged.PositionMoved))
                        {
                            readers[bpi].ReadBlueprintPositions(bp);
                            UpdatePositions();
                        }
                        if (shapeChanges.HasFlag(ContourShape.ShapeChanged.NormalChanged))
                        {
                            readers[bpi].ReadBlueprintNormal(bp);
                            UpdateNormals();
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
                readers.Add(ContourReader.NewReader(blueprints[i]));
        }
    }

    public virtual void RebuildAll()
    {
        return;
    }

    protected virtual bool CanBuildFrom(ContourReader reader)
    {
        return false;
    }

    protected virtual void UpdatePositions()
    {
        return;
    }

    protected virtual void UpdateNormals()
    {
        return;
    }
}
