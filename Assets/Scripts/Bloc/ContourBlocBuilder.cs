using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ContourBlocBuilder : MonoBehaviour
{
    public List<Contour> contours;
    [HideInInspector] public ContourBloc bloc;
    [HideInInspector] public ContourPalette palette;

    [HideInInspector] [SerializeField] private List<ContourReader> readers;
    [HideInInspector] [SerializeField] private List<ContourBuilder> builders;

    [Serializable]
    public struct Contour
    {
        [HideInInspector] public List<ContourBlueprint> blueprints;
        [HideInInspector] public int paletteIndex;
        [HideInInspector] public ContourShape shape;
        [SerializeField] private Vector2[] positions;

        public Contour(ContourShape shape)
        {
            this.shape = shape;
            positions = shape.GetPositions();
            if (positions == null) positions = new Vector2[0];
            paletteIndex = 0;
            blueprints = new List<ContourBlueprint>();
        }

        public Vector2[] GetPositions()
        {
            Vector2[] shapePositions = shape.GetPositions();
            return shapePositions != null ? shapePositions : positions;
        }

        public void UpdatePositions()
        {
            positions = GetPositions();
        }

        public void SetShape(ContourShape shape)
        {
            this.shape = shape;
            UpdatePositions();
        }
    }

    private void Reset()
    {
        bloc = GetComponent<ContourBloc>();
        foreach (ContourBlueprint bp in GetComponents<ContourBlueprint>())
            DestroyImmediate(bp);
        foreach (ContourBuilder b in GetComponentsInChildren<ContourBuilder>())
            if (b != null && b.gameObject != null) DestroyImmediate(b.gameObject);
    }

    private void Update()
    {        
        Build();
    }

    public int ContourCount => contours != null ? contours.Count : 0;
    
    public void Build()
    {
        UpdateContours();
        UpdateBlueprints();
        UpdateBuilders();
        if (builders != null) foreach (ContourBuilder b in builders) b.Build();
    }

    public Vector2[] GetPositions(int contourIndex)
    {
        if (contours == null || contourIndex < 0 || contourIndex >= contours.Count) return new Vector2[0];
        return contours[contourIndex].GetPositions();
    }

    public ContourBlueprint[] GetContourBlueprints(int contourIndex)
    {
        if (contours == null || contourIndex < 0 || contourIndex >= contours.Count || contours[contourIndex].blueprints == null) return new ContourBlueprint[0];
        return contours[contourIndex].blueprints.ToArray();
    }

    public int PaletteSize => palette != null ? palette.Size : 0;

    public string[] GetPaletteOptionNames()
    {
        int paletteSize = PaletteSize;
        string[] options = new string[paletteSize];
        for (int pi = 0; pi < paletteSize; pi++)
            options[pi] = palette.items[pi].name;
        return options;
    }

    public int GetPaletteIndex(int contourIndex)
    {
        if (contours == null || contourIndex < 0 || contourIndex >= contours.Count) return -1;
        return contours[contourIndex].paletteIndex;
    }

    public void SetPaletteIndex(int contourIndex, int paletteIndex)
    {
        if (contours == null || contourIndex < 0 || contourIndex >= contours.Count) return;
        Contour ct = contours[contourIndex];
        ct.paletteIndex = paletteIndex;
        contours[contourIndex] = ct;
    }

    public List<ContourBlueprint> GetAllBlueprints()
    {
        List<ContourBlueprint> blueprints = new List<ContourBlueprint>();
        GetComponents(blueprints);
        return blueprints;
    }

    public void UpdateContours()
    {
        // Update contour shapes from bloc, while keeping materials & blueprints related parameters
        if (bloc != null)
        {
            List<ContourShape> shapesInBloc = bloc.ContourShapes;
            int contourCount = shapesInBloc.Count;
            // If builder has no contours, create contour list from scratch
            if (ContourCount == 0)
                contours = shapesInBloc.ConvertAll(shape => new Contour(shape));
            // Or update existing list
            else
            {
                List<Contour> updatedContours = new List<Contour>(contourCount);
                foreach (ContourShape shape in shapesInBloc)
                {
                    Contour contour = contours.Find(ct => ct.shape == shape);
                    if (contour.shape == null) contour.shape = shape;
                    contour.UpdatePositions();
                    updatedContours.Add(contour);
                }
                contours = updatedContours;
            }
        }
    }

    private void UpdateBlueprints()
    {
        // Get all current blueprints
        List<ContourBlueprint> unusedBlueprints = GetAllBlueprints();
        //Each contour needs one blueprint per material
        if (palette != null)
        {
            for (int cti = 0, ctCount = ContourCount; cti < ctCount; cti++)
            {
                Contour contour = contours[cti];
                List<ContourMaterial> contourMaterials = palette.GetContourMaterials(contour.paletteIndex);
                int cmCount = contourMaterials.Count;
                List<ContourBlueprint> oldBlueprints = contour.blueprints;
                contour.blueprints = new List<ContourBlueprint>(cmCount);
                // Find each corresponding blueprint to each material, or create new blueprints when needed
                foreach (ContourMaterial cm in contourMaterials)
                {
                    if (cm == null) continue;
                    int findBlueprintIndex = oldBlueprints != null ? oldBlueprints.FindIndex(bp => bp != null && bp.material == cm) : - 1;
                    ContourBlueprint blueprint;
                    // Found blueprint
                    if (findBlueprintIndex != -1)
                    {
                        blueprint = oldBlueprints[findBlueprintIndex];
                        oldBlueprints.RemoveAt(findBlueprintIndex);
                        unusedBlueprints.Remove(blueprint);
                    }
                    // Or create blueprint
                    else
                    {
                        blueprint = gameObject.AddComponent(cm.BlueprintType) as ContourBlueprint;
                        blueprint.material = cm;
                    }
                    // Update blueprint positions and add it to contour
                    blueprint.positions = contour.GetPositions();
                    contour.blueprints.Add(blueprint);
                }                    
                // Apply changes
                contours[cti] = contour;
            }
        }
        // Destroy unused blueprints
        foreach (ContourBlueprint bp in unusedBlueprints)
            DestroyImmediate(bp);
    }

    private void UpdateBuilders()
    {
        List<ContourBlueprint> blueprints = GetAllBlueprints();
        // Update readers
        {
            if (blueprints == null)
                readers = null;
            else
                readers = blueprints.ConvertAll(bp => ContourReader.NewReader(bp));
        }
        // Update builders
        {
            // Reset old builders
            int oldBuilderCount = builders != null ? builders.Count : 0;
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
                newBuilders = new List<ContourBuilder>(readers.Count);
                // Dispatch readers to adequate builders
                foreach (ContourReader r in readers)
                {
                    ContourBuilder matchingBuilder = null;
                    // Check if adequate builder exist in old builders
                    if (oldBuilderCount > 0)
                    {
                        matchingBuilder = builders.Find(b => b != null && b.TryAddReader(r));
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
            builders = new List<ContourBuilder>(newBuilders);
        }
    }
}
