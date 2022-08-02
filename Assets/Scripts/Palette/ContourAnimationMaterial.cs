using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ContourAnimationMaterial", menuName = "Contour/Material/Animation Material")]
public class ContourAnimationMaterial : ContourMaterial
{
    public AnimationCurve xAnimation = AnimationCurve.Constant(0f,1f,0f);
    public AnimationCurve yAnimation = AnimationCurve.Constant(0f, 1f, 0f);
    public float amplitudeMultiplier = 1f;
    public float frequencyMultiplier = 1f;
    public float outPhasing = 1f;

    public override bool IsStatic => false;
}
