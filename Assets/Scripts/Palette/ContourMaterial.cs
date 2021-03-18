using UnityEngine;
using System;

public abstract class ContourMaterial : ScriptableObject
{
    public enum BlueprintMode { UseMaterialValue, UseBlueprintValue, UseBoth }

    public virtual Type BlueprintType
    { 
        get
        {
            return typeof(ContourBlueprint);
        }
    }
}