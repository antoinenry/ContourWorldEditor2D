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
            paletteIndex = -1;
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
        UpdateBluePrints();
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
        if (contours == null || contourIndex < 0 || contourIndex > contours.Count) return -1;
        return contours[contourIndex].paletteIndex;
    }

    public void SetPaletteIndex(int contourIndex, int paletteIndex)
    {
        if (contours == null || contourIndex < 0 || contourIndex > contours.Count) return;
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
                    int findContourIndex = contours.FindIndex(ct => ct.GetPositions() != null && ct.shape.Equals(shape));
                    if (findContourIndex != -1)
                    {
                        contours[findContourIndex].UpdatePositions();
                        updatedContours.Add(contours[findContourIndex]);
                    }
                    else
                        updatedContours.Add(new Contour(shape));
                }
                contours = updatedContours;
            }
        }
    }

    private void UpdateBluePrints()
    {
        // Existing blueprints components
        List<ContourBlueprint> existingBlueprints = new List<ContourBlueprint>();
        GetComponents(existingBlueprints);
        int existingBlueprintCount = existingBlueprints.Count;
        int newBlueprintCount = 0;
        // Converting contours to blueprints
        if (contours != null)
        {
            for (int cti = 0, ctCount = ContourCount; cti < ctCount; cti++)
            {
                Contour contour = contours[cti];
                // Get contour materials
                int paletteIndex = contour.paletteIndex;
                if (paletteIndex < 0 || paletteIndex >= PaletteSize) continue;
                List<ContourMaterial> contourMaterials = palette.items[paletteIndex].contourMaterials;
                if (contourMaterials == null) continue;
                // Compare contour materials and blueprints
                int cmCount = contourMaterials.Count;
                int bpCount = contour.blueprints != null ? contour.blueprints.Count : 0;
                for (int cmi = 0; cmi < cmCount; cmi++)
                {
                    // What type of blueprint is needed for this material
                    Type blueprintType = contourMaterials[cmi].BlueprintType;
                    //
                    if (cmi >= bpCount)
                    {
                    }
                    else
                    {

                    }
                }
                if (contour.blueprints == null)
                {
                    // Blueprints are not set => create new blueprints
                }
                else
                {
                    // Blueprints already exist => update existing blueprints
                }
            }
        }
        // Destroy excess blueprint components
        if (newBlueprintCount < existingBlueprintCount)
            for (int bpi = newBlueprintCount; bpi < existingBlueprintCount; bpi++)
                if (existingBlueprints[bpi] != null) DestroyImmediate(existingBlueprints[bpi]);
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
