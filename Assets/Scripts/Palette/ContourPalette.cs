using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ContourPalette", menuName = "Contour/Palette")]
public class ContourPalette : ScriptableObject
{
    public List<ContourDress> items;

    [Serializable]
    public struct ContourDress
    {
        public string name;
        public List<ContourMaterial> contourMaterials;
    }

    public int Size => items != null ? items.Count : 0;

    public List<ContourMaterial> GetContourMaterials(int paletteIndex)
    {
        if (paletteIndex < 0 || paletteIndex >= Size) return new List<ContourMaterial>();
        else return items[paletteIndex].contourMaterials;
    }
}
