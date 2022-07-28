using System;

[Serializable]
public abstract class ContourReader
{
    public static ContourReader NewReader(ContourBlueprint blueprint)
    {
        if (blueprint == null || blueprint.material == null) return null;
        ContourReader newReader = null;
        if (blueprint.material is ContourFaceMaterial) newReader = new ContourFaceReader();
        else if (blueprint.material is ContourLineMaterial) newReader = new ContourLineReader();
        else if (blueprint.material is ContourColliderMaterial) newReader = new ContourColliderReader();
        else if (blueprint.material is ContourAnimationMaterial) newReader = new ContourAnimationReader();
        if (newReader == null) throw new Exception(newReader.ToString() + " creation error. " + blueprint.material.ToString() + " reading not implemented");
        newReader.TryReadBlueprint(blueprint);
        // Return created reader
        //newReader.blueprint = blueprint;
        return newReader;
    }

    public abstract bool CanReadBlueprint(ContourBlueprint blueprint);

    public virtual bool ReadBlueprintPositions(ContourBlueprint blueprint) { return true; }

    public virtual void ReadBlueprintNormal(ContourBlueprint blueprint) { return; }

    public virtual bool TryReadBlueprint(ContourBlueprint blueprint) { return false; }

    public virtual bool Clear() { return false; }

    public abstract Type BuilderType { get; }

    public abstract string BuilderName { get; }
}
