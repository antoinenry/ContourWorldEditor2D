using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ContourBloc))]
public class ContourBlocBuilder : MonoBehaviour
{
    public ContourBloc bloc;
    public bool bluePrintsHaveChanged;

    [SerializeField] private ContourBlueprint[] blueprints;
    [SerializeField] private ContourReader[] readers;
    [SerializeField] private ContourBuilder[] builders;

    private void Reset()
    {
        bloc = GetComponent<ContourBloc>();
    }

    public void Build()
    {
        // Get blueprints from bloc
        if (bloc == null) SetBlueprints(null);
        SetBlueprints(bloc.ContourBluePrints);
        // Apply changes
        if (bluePrintsHaveChanged && builders != null)
        {
            foreach(ContourBuilder b in builders) b.Build();
            bluePrintsHaveChanged = false;
        }
    }

    private void SetBlueprints(ContourBlueprint[] newBlueprints)
    {
        blueprints = newBlueprints;

        // Update readers
        {
            if (blueprints == null)
                readers = null;
            else
                readers = Array.ConvertAll(blueprints, bp => ContourReader.NewReader(bp));
        }
        // Update builders
        {
            // Reset old builders
            int oldBuilderCount = builders != null ? builders.Length : 0;
            List<ContourBuilder> unusedBuilders = new List<ContourBuilder>(oldBuilderCount);
            for (int i = 0; i < oldBuilderCount; i++)
            {
                ContourBuilder oldBuilder = builders[i];
                if (oldBuilder != null)
                {
                    oldBuilder.Reset();
                    if (!unusedBuilders.Contains(oldBuilder)) unusedBuilders.Add(oldBuilder);
                }
            }
            // Set new builders
            List<ContourBuilder> newBuilders;
            if (readers != null)
            {
                newBuilders = new List<ContourBuilder>(readers.Length);
                // Dispatch readers to adequate builders
                foreach (ContourReader r in readers)
                {
                    ContourBuilder matchingBuilder = null;
                    // Check if adequate builder exist in old builders
                    if (oldBuilderCount > 0)
                    {
                        matchingBuilder = Array.Find(builders, b => b != null && b.TryAddReader(r));
                        if (matchingBuilder != null)
                        {
                            unusedBuilders.Remove(matchingBuilder);
                            newBuilders.Add(matchingBuilder);
                            continue;
                        }
                    }
                    // Else check if adequate builder exists in new builders
                    matchingBuilder = newBuilders.Find(b => b != null && b.TryAddReader(r));
                    if (matchingBuilder != null)
                    {
                        unusedBuilders.Remove(matchingBuilder);
                        newBuilders.Add(matchingBuilder);
                        continue;
                    }
                    // Else create builder from scratch
                    matchingBuilder = ContourBuilder.NewBuilder(r, transform);
                    if (matchingBuilder == null) continue;
                    matchingBuilder.TryAddReader(r);
                    newBuilders.Add(matchingBuilder);
                }
            }
            else
            {
                newBuilders = null;
            }
            // Destroy all unused builders
            foreach(ContourBuilder b in unusedBuilders)
            {
                if (b != null) DestroyImmediate(b.gameObject);
            }
            // Replace old builders with new ones
            builders = newBuilders != null ? newBuilders.ToArray() : null;
            // Signal blueprint change (look into that for future optimization)
            bluePrintsHaveChanged = true;
        }
    }
}
