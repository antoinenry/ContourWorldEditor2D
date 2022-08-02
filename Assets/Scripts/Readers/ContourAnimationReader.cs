using System;
using UnityEngine;

public class ContourAnimationReader : ContourReader
{
    public Vector2[] IdlePositions { get; private set; }
    public Vector2[] AnimatedPositions { get; private set; }

    private AnimationCurve xCurve;
    private AnimationCurve yCurve;
    private float fMultiplier;
    private float vMultiplier;
    private float outPhasing;

    public override Type BuilderType => typeof(ContourAnimationBuilder);
    public override string BuilderName => "Animation Builder";
    public override bool ReadSuccess => AnimatedPositions != null;

    public override bool CanReadBlueprint(ContourBlueprint blueprint)
    {
        return blueprint != null && blueprint.material != null && blueprint.material is ContourAnimationMaterial && blueprint.ContourLength > 0;
    }

    public override bool TryReadBlueprint(ContourBlueprint blueprint)
    {
        if (!CanReadBlueprint(blueprint)) return false;
        // Setup position array for contour animation
        int contourLength = blueprint.ContourLength;
        IdlePositions = new Vector2[contourLength];
        Array.Copy(blueprint.Positions, IdlePositions, contourLength);
        AnimatedPositions = new Vector2[contourLength];
        // Read animation parameters
        ContourAnimationMaterial material = blueprint.material as ContourAnimationMaterial;
        xCurve = material.xAnimation;
        yCurve = material.yAnimation;
        vMultiplier = material.amplitudeMultiplier;
        fMultiplier = material.frequencyMultiplier;
        outPhasing = material.outPhasing;
        return true;
    }

    public void Animate(float time)
    {
        int positionCount = AnimatedPositions.Length;
        for (int i = 0; i < positionCount; i++)
        {
            AnimatedPositions[i].x = IdlePositions[i].x + vMultiplier * xCurve.Evaluate(fMultiplier * (time - i * outPhasing));
            AnimatedPositions[i].y = IdlePositions[i].y + vMultiplier * yCurve.Evaluate(fMultiplier * (time - i * outPhasing));
        }
    }
}
