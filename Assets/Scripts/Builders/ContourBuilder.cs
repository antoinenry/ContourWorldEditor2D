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
            //builderGO.name = reader.BuilderName;
            //builderGO.AddComponent(reader.BuilderType);
            if (reader is ContourMeshReader)
            {
                builderGO.name = "Mesh builder";
                newBuilder = builderGO.AddComponent<ContourMeshBuilder>();
            }
            else if (reader is ContourColliderReader)
            {
                builderGO.name = "Collider builder";
                newBuilder = builderGO.AddComponent<ContourColliderBuilder>();
            }
            else if (reader is ContourAnimationReader)
            {
                builderGO.name = "Animation builder";
                newBuilder = builderGO.AddComponent<ContourAnimationBuilder>();
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

    private void Update()
    {
        Build();
    }

    public virtual void Build()
    {
        // Default update: rebuild all for any change in blueprints
        // can be further optimized in children classes
        if (blueprints != null)
        {
            int blueprintCount = blueprints.Count;
            if (readers == null || readers.Count != blueprintCount) ResetReaders();
            bool rebuild = false;
            bool updatePositions = false;
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
                else if (bp.blueprintChanges != ContourBlueprint.BlueprintChange.None)
                {
                    if (bp.blueprintChanges.HasFlag(ContourBlueprint.BlueprintChange.MaterialChanged))
                    {
                        rebuild = true;
                    }
                    if (bp.blueprintChanges.HasFlag(ContourBlueprint.BlueprintChange.ParameterChanged))
                    {
                        OnChangeBlueprintParameters();
                    }
                }
                else if (bp.ShapeChanges != ContourShape.ShapeChanged.None)
                {
                    if (bp.ShapeChanges.HasFlag(ContourShape.ShapeChanged.LengthChanged))
                    {
                        rebuild = true;
                    }
                    else if (bp.ShapeChanges.HasFlag(ContourShape.ShapeChanged.PositionMoved))
                    {
                        readers[bpi].ReadBlueprintPositions(bp);
                        updatePositions = true;
                    }
                }
            }
            if (rebuild)
            {
                Debug.Log("RebuildAll");
                RebuildAll();
            }
            if (updatePositions)
            {
                Debug.Log("UpdatePositions");
                UpdatePositions();
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

    public abstract void RebuildAll();

    protected abstract bool CanBuildFrom(ContourReader reader);

    protected abstract void UpdatePositions();

    protected virtual void OnChangeBlueprintParameters()
    {
        return;
    }
}
