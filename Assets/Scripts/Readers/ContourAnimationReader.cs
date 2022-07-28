using System;
using UnityEngine;

public class ContourAnimationReader : ContourReader
{
    public override Type BuilderType => typeof(ContourAnimationBuilder);
    public override string BuilderName => "Animation Builder";

    public override bool ReadSuccess => throw new NotImplementedException();

    public override bool CanReadBlueprint(ContourBlueprint blueprint)
    {
        return blueprint != null && blueprint.material != null && blueprint.material is ContourAnimationMaterial && blueprint.ContourLength > 0;
    }
}
