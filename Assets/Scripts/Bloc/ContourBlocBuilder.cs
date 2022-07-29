using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
public class ContourBlocBuilder : MonoBehaviour
{
    [Serializable]
    public struct ContourBuildInfos
    {
        public ContourShape shape;
        public ContourBlueprint[] blueprints;
        public int paletteIndex;

        public ContourBuildInfos(ContourShape shape)
        {
            this.shape = shape;
            paletteIndex = -1;
            blueprints = new ContourBlueprint[0];
        }

        public Vector2[] GetPositions()
        {
            Vector2[] shapePositions = shape.GetPositions();
            return shapePositions != null ? shapePositions : new Vector2[0];
        }
    }

    public ContourBloc bloc;
    public ContourPalette palette;
    public int defaultPaletteIndex;

    [SerializeField] private List<ContourBuildInfos> contourBuildInfos;
    [SerializeField] private List<ContourBuilder> builders;

    private void Reset()
    {
        bloc = GetComponent<ContourBloc>();
        RebuildAll();
    }

    private void OnEnable()
    {
        RebuildAll();
        FixStaticContours();
    }

    private void Update()
    {
        // Rebuild bloc when needed
        if (bloc != null)
        {
            if (bloc.changes.HasFlag(ContourBloc.BlocChanges.ContourAdded) || bloc.changes.HasFlag(ContourBloc.BlocChanges.ContourRemoved))
                RebuildAll();
        }
    }

    public int ContourCount => contourBuildInfos != null ? contourBuildInfos.Count : 0;
    
    public void RebuildAll()
    {
        GetContoursFromBloc();
        ResetAllContourBuildInfos();
        ResetAllBuilders();
    }

    public ContourBuildInfos GetContourInfos(int contourIndex)
    {
        return contourBuildInfos[contourIndex];
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
        if (contourBuildInfos == null || contourIndex < 0 || contourIndex >= contourBuildInfos.Count) return -1;
        return contourBuildInfos[contourIndex].paletteIndex;
    }

    public void SetPaletteIndex(int contourIndex, int paletteIndex)
    {
        if (contourBuildInfos == null || contourIndex < 0 || contourIndex >= contourBuildInfos.Count) return;
        ContourBuildInfos ct = contourBuildInfos[contourIndex];
        if (ct.paletteIndex != paletteIndex)
        {
            ct.paletteIndex = paletteIndex;
            contourBuildInfos[contourIndex] = ct;
            ResetContourBuildInfosAt(contourIndex);
            ResetAllBuilders();
        }
    }

    public void GetContoursFromBloc()
    {
        // Update contour shapes from bloc, while keeping materials & blueprints related parameters
        List<ContourShape> shapesInBloc = bloc != null ? bloc.ContourShapes : new List<ContourShape>(0);
        int contourCount = shapesInBloc.Count;
        // If builder has no contours, create contour list from scratch
        if (ContourCount == 0)
            contourBuildInfos = shapesInBloc.ConvertAll(shape => new ContourBuildInfos(shape));
        // Or update existing list
        else
        {
            List<ContourBuildInfos> updatedContours = new List<ContourBuildInfos>(contourCount);
            foreach (ContourShape shape in shapesInBloc)
            {
                if (shape == null) continue;
                // First we try to find a match by reference
                ContourBuildInfos contour = contourBuildInfos.Find(ct => ct.shape != null && ct.shape == shape);
                // Then we try to fing a match by positions (usefull for first update)
                if (contour.shape == null) contour = contourBuildInfos.Find(ct => Enumerable.SequenceEqual(ct.shape.GetPositions(), shape.GetPositions()));
                // If no match, contour will have default values
                if (contour.shape == null) contour.paletteIndex = -1;
                contour.shape = shape;
                //contour.UpdatePositions();
                updatedContours.Add(contour);
            }
            // Apply update
            contourBuildInfos = updatedContours;
        }
    }

    private void ResetContourBuildInfosAt(int contourIndex)
    {
        // Each contour needs one blueprint and reader per material
        ContourBuildInfos contour = contourBuildInfos[contourIndex];
        // If undefined, set default paleete index for contour
        if (contour.paletteIndex == -1) contour.paletteIndex = defaultPaletteIndex;
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
                blueprint = new ContourBlueprint();
                blueprint.material = cm;
            }
            // Set blueprint positions
            blueprint.shape = contour.shape;
            usedBlueprints.Add(blueprint);
        }
        // Apply changes
        contour.blueprints = usedBlueprints.ToArray();
        contourBuildInfos[contourIndex] = contour;
    }

    private void ResetAllContourBuildInfos()
    {
        // Reset blueprints for each contour
        for (int cti = 0, ctCount = ContourCount; cti < ctCount; cti++)
            ResetContourBuildInfosAt(cti);
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
        // Set new builders
        List<ContourBuilder> newBuilders;
        newBuilders = new List<ContourBuilder>();
        // Dispatch readers to adequate builders
        if (contourBuildInfos != null)
        {
            foreach (ContourBuildInfos ct in contourBuildInfos)
                foreach (ContourBlueprint bp in ct.blueprints)
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
        }        
        // Destroy all unused builders
        foreach (ContourBuilder b in unusedBuilders)
            if (b != null) DestroyImmediate(b.gameObject);
        // Replace old builders with new ones
        builders = new List<ContourBuilder>(newBuilders);
        // Rebuild all
        foreach (ContourBuilder b in builders) b.RebuildAll();
    }

    public void FixStaticContours()
    {
        if (contourBuildInfos == null) return;
        foreach(ContourBuildInfos contour in contourBuildInfos)
        {
            if(contour.shape != null) contour.shape.SetPointsStatic(false);
        }
        foreach (ContourBuildInfos contour in contourBuildInfos)
        {
            if (contour.blueprints == null) continue;
            bool isContourAnimated = Array.FindIndex(contour.blueprints, bp => bp != null && !bp.IsStatic) != -1;
            if (isContourAnimated == false && contour.shape != null) contour.shape.SetPointsStatic(true);
        }
    }
}
