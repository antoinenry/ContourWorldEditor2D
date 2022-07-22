using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ContourAnimationMaterial", menuName = "Contour/Material/Animation Material")]
public class ContourAnimationMaterial : ContourMaterial
{
    public float amplitude = 1f;
    public float cycleDuration = 1f;
    public float phase = 180f;

    public override Type BlueprintType => typeof(ContourAnimationBlueprint);
}
