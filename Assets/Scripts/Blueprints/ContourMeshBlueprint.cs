using System;
using UnityEngine;

[Serializable]
public class ContourMeshBlueprint : ContourBlueprint
{
    [LinkMaterialAndBlueprint("normal", "normalMode")]
    [UnitVector(autoAdjust = UnitVectorAttribute.AutoAdjustBehaviour.AdjustZ)]
    public Vector3 normal = Vector3.back;
    [LinkMaterialAndBlueprint("color", "colorMode")]
    public Color color = Color.white;

    public Vector3 Normal
    {
        get
        {
            if (material != null && material is ContourMeshMaterial)
            {
                ContourMeshMaterial cmm = material as ContourMeshMaterial;
                switch (cmm.normalMode)
                {
                    case ContourMaterial.BlueprintMode.UseBlueprintValue: return normal;
                    case ContourMaterial.BlueprintMode.UseMaterialValue: return cmm.normal;
                    case ContourMaterial.BlueprintMode.UseBoth: return normal + cmm.normal;
                }
            }
            return normal;
        }
    }

    public Color Color
    {
        get
        {
            if (material != null && material is ContourMeshMaterial)
            {
                ContourMeshMaterial cmm = material as ContourMeshMaterial;
                switch (cmm.colorMode)
                {
                    case ContourMaterial.BlueprintMode.UseBlueprintValue: return color;
                    case ContourMaterial.BlueprintMode.UseMaterialValue: return cmm.color;
                    case ContourMaterial.BlueprintMode.UseBoth: return (color + cmm.color) / 2f;
                }
            }
            return color;
        }
    }
}
