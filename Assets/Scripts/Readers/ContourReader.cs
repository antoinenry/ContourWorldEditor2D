using UnityEngine;
using System;

public abstract class ContourReader
{
    [SerializeField]
    protected ContourBlueprint blueprint;

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

    public void ReadBlueprint()
    {
        if (blueprint != null) TryReadBlueprint(blueprint);
    }

    public abstract void ReadBlueprintPositions();

    public abstract bool Clear();

    protected abstract bool TryReadBlueprint(ContourBlueprint blueprint);

}
