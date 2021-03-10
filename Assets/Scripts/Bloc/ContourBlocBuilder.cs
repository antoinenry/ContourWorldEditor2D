using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ContourBloc))]
public class ContourBlocBuilder : MonoBehaviour
{
    public ContourBloc bloc;
    public List<Contour> contours;
    public ContourPalette palette;
    public List<ContourBlueprint> blueprints;
    //public List<ContourMaterialBundle> contourMaterialBundles;

    [SerializeField] private List<ContourReader> readers;
    [SerializeField] private List<ContourBuilder> builders;

    [Serializable]
    public struct Contour
    {
        public List<Vector2> positions;
        public int paletteIndex;
    }

    private void Reset()
    {
        bloc = GetComponent<ContourBloc>();
    }

    public void Build()
    {
        GetContoursFromBloc();
        UpdateBluePrints();
        BuildFromBlueprints();
        // Apply changes
        if (builders != null) foreach(ContourBuilder b in builders) b.Build();
    }

    public void GetContoursFromBloc()
    {
        if (bloc != null)
        {
            // Adjust contour list size
            int contourCount = bloc.GetContourCount();
            int listSize = contours != null ? contours.Count : 0;
            if (contours == null) contours = new List<Contour>(contourCount);
            else if (listSize < contourCount) contours.Capacity = contourCount;
            else if (listSize > contourCount) contours.RemoveRange(contourCount, listSize - contourCount);
            // Set positions
            for (int cti = 0; cti < contourCount; cti++)
            {
                if (cti >= listSize) contours.Add(new Contour());
                Contour ct = contours[cti];
                ct.positions = bloc.GetContourPositions(cti);
                contours[cti] = ct;
            }
        }
    }

    public int ContourCount => contours != null ? contours.Count : 0;

    public Vector2[] GetPositions(int contourIndex)
    {
        if (contours == null || contourIndex < 0 || contourIndex > contours.Count || contours[contourIndex].positions == null) return new Vector2[0];
        return contours[contourIndex].positions.ToArray();
    }

    public void SetPositions(int contourIndex, Vector2[] positions)
    {
        if (contours == null || contourIndex < 0 || contourIndex > contours.Count) return;
        Contour ct = contours[contourIndex];
        ct.positions = positions != null ? new List<Vector2>(positions) : new List<Vector2>();
        contours[contourIndex] = ct;
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

    private void UpdateBluePrints()
    {
        if (contours != null)
        {
            blueprints = new List<ContourBlueprint>();
            for (int cti = 0, ctCount = ContourCount; cti < ctCount; cti++)
            {
                List<Vector2> contourPositions = contours[cti].positions;
                int paletteIndex = contours[cti].paletteIndex;
                if (paletteIndex < 0 || paletteIndex >= PaletteSize) continue;
                // Create one blueprint for each contour material
                List<ContourMaterial> cms = palette.items[paletteIndex].contourMaterials;
                if (cms == null) continue;
                foreach (ContourMaterial cm in cms)
                {
                    if (cm == null) continue;
                    ContourBlueprint newBlueprint = ScriptableObject.CreateInstance(typeof(ContourMeshBlueprint)) as ContourBlueprint;
                    newBlueprint.positions = contourPositions != null ? contourPositions.ToArray() : new Vector2[0];
                    newBlueprint.material = cm;
                    blueprints.Add(newBlueprint);
                }
            }
        }
    }

    //public int UpdateMaterialListSize()
    //{
    //    if (bloc == null)
    //    {
    //        contourMaterialBundles = new List<ContourMaterialBundle>();
    //        return 0;
    //    }
    //    else
    //    {
    //        int tagCount = bloc.contourTagNames != null ? bloc.contourTagNames.Count : 0;
    //        if (contourMaterialBundles == null) contourMaterialBundles = new List<ContourMaterialBundle>(tagCount);
    //        int listSize = contourMaterialBundles.Count;
    //        if (listSize == tagCount) return listSize;
    //        if (listSize < tagCount) contourMaterialBundles.AddRange(new ContourMaterialBundle[tagCount - listSize]);
    //        else contourMaterialBundles.RemoveRange(tagCount, listSize - tagCount);
    //        return tagCount;
    //    }
    //}

    private void BuildFromBlueprints()
    {
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
            builders = newBuilders != null ? newBuilders : null;
        }
    }
}
