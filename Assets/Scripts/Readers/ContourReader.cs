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

    public abstract void ReadBlueprintPositions(ContourBlueprint blueprint);

    public abstract bool TryReadBlueprint(ContourBlueprint blueprint);

    public abstract bool Clear();

    public abstract Type BuilderType { get; }

    public abstract string BuilderName { get; }
}
