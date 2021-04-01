using UnityEngine;
using System;

public abstract class ContourReader
{
    [SerializeField]
    protected ContourBlueprint blueprint;
    //public ReaderChanges changes;

    //[Flags]
    //public enum ReaderChanges { None = 0, PositionMoved = 1, LengthChanged = 2 }

    public ContourBlueprint Blueprint => blueprint;
    public ContourMaterial Material => blueprint != null ? blueprint.material : null;

    public static ContourReader NewReader(ContourBlueprint blueprint)
    {
        ContourReader newReader = null;
        if (blueprint != null)
        {
            if (blueprint.material is ContourFaceMaterial) newReader = new ContourFaceReader();
            else if (blueprint.material is ContourLineMaterial) newReader = new ContourLineReader();
            else if (blueprint.material is ContourColliderMaterial) newReader = new ContourColliderReader();
        }
        // Check for errors
        else throw new Exception(newReader.ToString() + " creation error. Blueprint is null.");
        if (newReader == null) throw new Exception(newReader.ToString() + " creation error. " + blueprint.material.ToString() + " reading not implemented");
        newReader.TryReadBlueprint(blueprint);
        // Return created reader
        newReader.blueprint = blueprint;
        return newReader;
    }

    public abstract bool Clear();

    public virtual bool CheckBlueprint()
    {
        if (blueprint == null)
            return Clear();
        else
        {
            ContourBlueprint.BlueprintChanges changes = blueprint.changes;
            if (changes == ContourBlueprint.BlueprintChanges.None)
                return false;
            if (changes.HasFlag(ContourBlueprint.BlueprintChanges.LengthChanged))
                TryReadBlueprint(blueprint);
            if (changes.HasFlag(ContourBlueprint.BlueprintChanges.PositionMoved))
                ReadBlueprintPositions();
            if (changes.HasFlag(ContourBlueprint.BlueprintChanges.ParameterChanged))
                OnBlueprintParameterChanged();
            return true;
        }
    }

    protected abstract bool TryReadBlueprint(ContourBlueprint blueprint);

    protected abstract void ReadBlueprintPositions();

    protected virtual void OnBlueprintParameterChanged()
    {
        return;
    }

    //public void SetContourPositions(Vector2[] positions)
    //{
    //    if (blueprint == null) throw new Exception("Blueprint is null");
    //    if (blueprint.positions == null || positions == null) throw new Exception("Blueprint does't match positions array");
    //    if (blueprint.positions.Length == positions.Length)
    //    {
    //        blueprint.positions = positions;
    //        ReadBlueprintPositions();
    //        changes |= ReaderChanges.PositionMoved;
    //    }
    //    else
    //    {
    //        blueprint.positions = positions;
    //        TryReadBlueprint(blueprint);
    //        changes |= ReaderChanges.LengthChanged;
    //    }
    //}
}
