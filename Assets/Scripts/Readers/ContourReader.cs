
public abstract class ContourReader
{
    public static ContourReader NewReader(ContourBlueprint blueprint)
    {
        ContourReader newReader = null;
        if (blueprint != null)
        {
            if (blueprint.material is ContourFaceMaterial) newReader = new ContourFaceReader();
            else if (blueprint.material is ContourLineMaterial) newReader = new ContourLineReader();
            else if (blueprint.material is ContourColliderMaterial) newReader = new ContourColliderReader();
        }

        if (newReader != null) newReader.TryReadBlueprint(blueprint);
        return newReader;
    }

    public abstract bool TryReadBlueprint(ContourBlueprint blueprint);
}
