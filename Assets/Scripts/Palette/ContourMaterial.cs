using UnityEngine;
using System;

public abstract class ContourMaterial : ScriptableObject
{
    public enum BlueprintMode { UseMaterialValue, UseBlueprintValue, UseBoth }
    public int Version { get; private set; }

    public virtual Type BlueprintType
    { 
        get
        {
            return typeof(ContourBlueprint);
        }
    }

    public void ChangeVersion()
    {
        if (Version >= int.MaxValue)
            Version = int.MinValue;
        else
            Version++;
    }
}