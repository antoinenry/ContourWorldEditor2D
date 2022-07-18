using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class ContourBlocBuilder : MonoBehaviour
{
    public ContourBloc bloc;
    public List<Contour> contours;
    public ContourPalette palette;

    //[HideInInspector] [SerializeField] private List<ContourReader> readers;
    [HideInInspector] [SerializeField] private List<ContourBuilder> builders;

    [Serializable]
    public struct Contour
    {
        public int paletteIndex;
        public ContourShape shape;
        public ContourBlueprint[] blueprints;

        public Contour(ContourShape shape)
        {
            this.shape = shape;
            paletteIndex = -1;
            blueprints = new ContourBlueprint[0];
        }

        public Vector2[] GetPositions()
        {
            Vector2[] shapePositions = shape.GetPositions();
            return shapePositions != null ? shapePositions : new Vector2[0]; //positions;            
        }
    }

    private void Reset()
    {
        bloc = GetComponent<ContourBloc>();
        foreach (ContourBlueprint bp in GetComponents<ContourBlueprint>())
            DestroyImmediate(bp);
        foreach (ContourBuilder b in GetComponentsInChildren<ContourBuilder>())
            if (b != null && b.gameObject != null) DestroyImmediate(b.gameObject);
        RebuildAll();
    }

    private void OnEnable()
    {
        RebuildAll();
    }

    private void Update()
    {
        // Rebuild bloc when needed
        if (bloc != null)
        {
            if (bloc.changes.HasFlag(ContourBloc.BlocChanges.ContourAdded) || bloc.changes.HasFlag(ContourBloc.BlocChanges.ContourRemoved))
            {
                RebuildAll();
            }
        }
    }

    public int ContourCount => contours != null ? contours.Count : 0;
    
    public void RebuildAll()
    {
        if (bloc != null) GetContoursFromBloc();
        ResetAllBlueprints();
        ResetAllBuilders();
    }

    public Vector2[] GetPositions(int contourIndex)
    {
        if (contours == null || contourIndex < 0 || contourIndex >= contours.Count) return new Vector2[0];
        return contours[contourIndex].GetPositions();
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
        if (ct.paletteIndex != paletteIndex)
        {
            ct.paletteIndex = paletteIndex;
            contours[contourIndex] = ct;
            ResetContourBlueprints(contourIndex);
            ResetAllBuilders();
        }
    }

    public void GetContoursFromBloc()
    {
        // Update contour shapes from bloc, while keeping materials & blueprints related parameters
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
                if (shape == null) continue;
                // First we try to find a match by reference
                Contour contour = contours.Find(ct => ct.shape != null && ct.shape == shape);
                // Then we try to fing a match by positions (usefull for first update)
                if (contour.shape == null) contour = contours.Find(ct => Enumerable.SequenceEqual(ct.shape.GetPositions(), shape.GetPositions()));
                // If no match, contour will have default values
                if (contour.shape == null) contour.paletteIndex = -1;
                contour.shape = shape;
                //contour.UpdatePositions();
                updatedContours.Add(contour);
            }
            // Apply update
            contours = updatedContours;
        }
    }

    private void ResetAllBlueprints()
    {
        // Reset blueprints for each contour
        for (int cti = 0, ctCount = ContourCount; cti < ctCount; cti++)
                ResetContourBlueprints(cti);
        // Put all existing blueprints in an "unused" pool
        List<ContourBlueprint> unusedBlueprints = new List<ContourBlueprint>();
        GetComponents(unusedBlueprints);
        // Check all contours for blueprints (these are "used" blueprints)
        if (contours != null)
        {
            foreach (Contour contour in contours)
                foreach (ContourBlueprint bp in contour.blueprints)
                    unusedBlueprints.Remove(bp);
        }
        // Destroy all unused blueprints
        foreach (ContourBlueprint bp in unusedBlueprints)
            DestroyImmediate(bp);                
    }

    private void ResetContourBlueprints(int contourIndex)
    {
        // Each contour needs one blueprint and reader per material
        Contour contour = contours[contourIndex];
        List<ContourMaterial> contourMaterials = palette != null ? palette.GetContourMaterials(contour.paletteIndex) : new List<ContourMaterial>();
        int cmCount = contourMaterials.Count;
        List<ContourBlueprint> usedBlueprints = new List<ContourBlueprint>();
        List<ContourBlueprint> unusedBlueprints = contour.blueprints != null ? new List<ContourBlueprint>(contour.blueprints) : new List<ContourBlueprint>();
        // Find each corresponding blueprint to each material, or create new blueprints when needed
        foreach (ContourMaterial cm in contourMaterials)
        {
            if (cm == null) continue;
            int findBlueprintIndex = unusedBlueprints.FindIndex(bp => bp != null && bp.material == cm);
            ContourBlueprint blueprint;
            // Found blueprint
            if (findBlueprintIndex != -1)
            {
                blueprint = unusedBlueprints[findBlueprintIndex];
                unusedBlueprints.RemoveAt(findBlueprintIndex);
            }
            // Or create blueprint
            else
            {
                blueprint = gameObject.AddComponent(cm.BlueprintType) as ContourBlueprint;
                blueprint.material = cm;
            }
            // Set blueprint positions
            //blueprint.Positions = contour.GetPositions();
            blueprint.shape = contour.shape;
            usedBlueprints.Add(blueprint);
        }
        // Apply changes
        contour.blueprints = usedBlueprints.ToArray();
        contours[contourIndex] = contour;
        //Destroy unused blueprints
        foreach (ContourBlueprint bp in unusedBlueprints) DestroyImmediate(bp);
    }

    private void ResetAllBuilders()
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
        // Get all blueprints
        List<ContourBlueprint> blueprints = new List<ContourBlueprint>();
        if (contours != null)
            foreach (Contour ct in contours)
                blueprints.AddRange(ct.blueprints);
        // Set new builders
        List<ContourBuilder> newBuilders;
        newBuilders = new List<ContourBuilder>(blueprints.Count);
        // Dispatch readers to adequate builders
        foreach (ContourBlueprint bp in blueprints)
        {
            ContourBuilder matchingBuilder = null;
            // Check if adequate builder exist in old builders
            if (oldBuilderCount > 0)
            {
                matchingBuilder = builders.Find(b => b != null && b.TryAddBlueprint(bp));
                if (matchingBuilder != null)
                {
                    unusedBuilders.Remove(matchingBuilder);
                    newBuilders.Add(matchingBuilder);
                    continue;
                }
            }
            // Else check if adequate builder exists in new builders
            matchingBuilder = newBuilders.Find(b => b != null && b.TryAddBlueprint(bp));
            if (matchingBuilder != null)
            {
                unusedBuilders.Remove(matchingBuilder);
                newBuilders.Add(matchingBuilder);
                continue;
            }
            // Else create builder from scratch
            matchingBuilder = ContourBuilder.NewBuilder(bp, transform);
            if (matchingBuilder == null) continue;
            newBuilders.Add(matchingBuilder);
        }
        // Destroy all unused builders
        foreach (ContourBuilder b in unusedBuilders)
            if (b != null) DestroyImmediate(b.gameObject);
        // Replace old builders with new ones
        builders = new List<ContourBuilder>(newBuilders);
        // Rebuild all
        foreach (ContourBuilder b in builders) b.RebuildAll();
    }
}
