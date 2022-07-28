using UnityEngine;

public abstract class ContourMaterial : ScriptableObject
{
    public virtual bool IsStatic => true;
    public enum BlueprintMode { UseMaterialValue, UseBlueprintValue, UseBoth }
    public int Version { get; private set; }

    public void ChangeVersion()
    {
        if (Version >= int.MaxValue)
            Version = int.MinValue;
        else
            Version++;
    }
}