using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class ContourBlocBuilder : MonoBehaviour
{
    public List<Contour> contours;
    [HideInInspector] public ContourBloc bloc;
    [HideInInspector] public ContourPalette palette;

    //[HideInInspector] [SerializeField] private List<ContourReader> readers;
    [HideInInspector] [SerializeField] private List<ContourBuilder> builders;

    [Serializable]
    public struct Contour
    {
        [HideInInspector] public List<ContourReader> readers;
        [HideInInspector] public int paletteIndex;
        [SerializeField] public ContourShape shape;
        //[SerializeField] private Vector2[] positions;

        public Contour(ContourShape shape)
        {
            this.shape = shape;
            //positions = shape.GetPositions();
            //if (positions == null) positions = new Vector2[0];
            paletteIndex = 0;
            readers = new List<ContourReader>();
        }

        public Vector2[] GetPositions()
        {
            Vector2[] shapePositions = shape.GetPositions();
            return shapePositions != null ? shapePositions : new Vector2[0]; //positions;            
        }

        public List<ContourBlueprint> GetBlueprints()
        {
            if (readers == null) return new List<ContourBlueprint>();
            else return readers.ConvertAll(r => r.Blueprint);
        }

        public int ContourLength => shape != null && shape.positions != null ? shape.positions.Count : 0;

        //public void UpdatePositions()
        //{
        //    positions = GetPositions();
        //}

        //public void SetShape(ContourShape shape)
        //{
        //    this.shape = shape;
        //    UpdatePositions();
        //}
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
        // Rebuild contours when needed
        if (bloc != null)
        {
            // Changes in bloc that require complete rebuild
            if (bloc.changes.HasFlag(ContourBloc.BlocChanges.ContourAdded) || bloc.changes.HasFlag(ContourBloc.BlocChanges.ContourRemoved))
            {
                RebuildAll();
            }
            // Localized changes that allow selective rebuild
            else if (bloc.changes.HasFlag(ContourBloc.BlocChanges.ContourChanged))
            {
                // Find contours that were changed
                for (int cti = 0, ctCount = ContourCount; cti < ctCount; cti++)
                {
                    Contour contour = contours[cti];
                    ContourShape shape = contour.shape;
                    if (shape != null)
                    {
                        ContourShape.ShapeChange shapeChanges = shape.changes;
                        if (shapeChanges != ContourShape.ShapeChange.None)
                        {
                            List<ContourReader> updateReader = contour.readers;
                            if (updateReader != null)
                            {
                                if (shapeChanges.HasFlag(ContourShape.ShapeChange.PositionMoved) || shapeChanges.HasFlag(ContourShape.ShapeChange.LengthChanged))
                                {
                                    // Update readers and blueprints
                                    foreach (ContourReader rd in updateReader)
                                        rd.SetContourPositions(shape.positions.ToArray());
                                }
                            }
                            // Force builder updates (better editor reactivity)
                            foreach (ContourBuilder builder in builders)
                                if (builder != null) builder.Update();
                        }
                        contour.shape.changes = ContourShape.ShapeChange.None;
                    }
                    contours[cti] = contour;
                }
            } 
            bloc.changes = ContourBloc.BlocChanges.None;
        }
    }

    public int ContourCount => contours != null ? contours.Count : 0;
    
    public void RebuildAll()
    {
        if (bloc != null) GetContoursFromBloc();
        ResetAllBlueprintsAndReaders();
        ResetAllBuilders();
    }

    public Vector2[] GetPositions(int contourIndex)
    {
        if (contours == null || contourIndex < 0 || contourIndex >= contours.Count) return new Vector2[0];
        return contours[contourIndex].GetPositions();
    }

    public ContourBlueprint[] GetContourBlueprints(int contourIndex)
    {
        if (contours == null || contourIndex < 0 || contourIndex >= contours.Count) return new ContourBlueprint[0];
        return contours[contourIndex].GetBlueprints().ToArray();
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
            ResetContourBlueprintsAndReaders(contourIndex);
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
                if (contour.shape == null) contour = contours.Find(ct => Enumerable.SequenceEqual(ct.shape.positions, shape.positions));
                // If no match, contour will have default values
                contour.shape = shape;
                //contour.UpdatePositions();
                updatedContours.Add(contour);
            }
            // Apply update
            contours = updatedContours;
        }
    }

    private void ResetAllBlueprintsAndReaders()
    {
        // Reset readers and blueprint for each contour
        for (int cti = 0, ctCount = ContourCount; cti < ctCount; cti++)
                ResetContourBlueprintsAndReaders(cti);
        // Destroy all unused blueprints
        List<ContourBlueprint> unusedBlueprints = new List<ContourBlueprint>();
        GetComponents(unusedBlueprints);
        foreach (Contour contour in contours)
            foreach (ContourBlueprint usedBlueprint in contour.GetBlueprints())
                unusedBlueprints.Remove(usedBlueprint);
        foreach (ContourBlueprint bp in unusedBlueprints)
            DestroyImmediate(bp);
    }

    private void ResetContourBlueprintsAndReaders(int contourIndex)
    {
        // Each contour needs one blueprint and reader per material
        Contour contour = contours[contourIndex];
        List<ContourMaterial> contourMaterials = palette != null ? palette.GetContourMaterials(contour.paletteIndex) : new List<ContourMaterial>();
        int cmCount = contourMaterials.Count;
        List<ContourReader> oldReaders = contour.readers;
        List<ContourBlueprint> unusedBlueprints = contour.GetBlueprints();
        contour.readers = new List<ContourReader>(cmCount);
        // Find each corresponding blueprint to each material, or create new blueprints when needed
        foreach (ContourMaterial cm in contourMaterials)
        {
            if (cm == null) continue;
            int findReaderIndex = oldReaders != null ? oldReaders.FindIndex(rd => rd != null && rd.Material == cm) : -1;
            ContourReader reader;
            ContourBlueprint blueprint;
            // Found blueprint
            if (findReaderIndex != -1)
            {
                reader = oldReaders[findReaderIndex];
                oldReaders.RemoveAt(findReaderIndex);
                blueprint = reader.Blueprint;
                reader.SetContourPositions(contour.GetPositions());
                unusedBlueprints.Remove(blueprint);
            }
            // Or create blueprint and reader
            else
            {
                Vector2[] contourPositions = contour.GetPositions();
                blueprint = gameObject.AddComponent(cm.BlueprintType) as ContourBlueprint;
                blueprint.material = cm;
                blueprint.positions = contourPositions;
                reader = ContourReader.NewReader(blueprint);
                //reader.MoveBlueprintPositions(contourPositions);
            }
            contour.readers.Add(reader);
        }
        // Apply changes
        contours[contourIndex] = contour;
        // Destroy unused blueprints
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
        // Get all readers
        List<ContourReader> readers = new List<ContourReader>();
        if (contours != null)
            foreach (Contour ct in contours)
                readers.AddRange(ct.readers);
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
        foreach (ContourBuilder b in unusedBuilders)
            if (b != null) DestroyImmediate(b.gameObject);
        // Replace old builders with new ones
        builders = new List<ContourBuilder>(newBuilders);
        // Rebuild all
        foreach (ContourBuilder b in builders) b.Build();
    }
}
