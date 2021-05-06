using UnityEngine;

public class ContourAnimationReader : ContourReader
{
    public Vector2[] animationPositions;

    private ContourShape contour;
    //private Vector2[] basePositions;
    private float radius;
    private float period;
    private float phase;
    private bool contourIsLoop;

    public override bool Clear()
    {
        animationPositions = new Vector2[0];
        radius = 0f;
        period = 0f;
        phase = 0f;
        contourIsLoop = false;
        if (contour == null) return false;
        contour = null;
        return true;
    }

    public override void ReadBlueprintPositions(ContourBlueprint blueprint)
    {
        animationPositions = new Vector2[blueprint.Positions.Length];
    }

    public override bool TryReadBlueprint(ContourBlueprint blueprint)
    {
        // Read if possible
        if (blueprint != null && blueprint.material is ContourAnimationMaterial)
        {
            contour = blueprint.shape;
            ReadBlueprintPositions(blueprint);
            ContourAnimationMaterial animationMaterial = blueprint.material as ContourAnimationMaterial;
            radius = animationMaterial.amplitude / 2f;
            period = animationMaterial.freq != 0f ? 1f / animationMaterial.freq : 0f;
            phase = animationMaterial.phase;
            if (contour != null && contour.Length > 2)
                contourIsLoop = contour.GetPosition(0) == contour.GetPosition(contour.Length - 1);
            else
                contourIsLoop = false;
            return true;
        }
        return false;
    }

    public void Animate(float time)
    {
        if (animationPositions != null)
        {
            int length = animationPositions.Length;
            if (length > 0)
            {
                for (int i = 0, iend = length - 1; i < iend; i++)
                {
                    animationPositions[i] = radius * new Vector2(Mathf.Cos(i * phase + time * period), Mathf.Sin(i * phase + time * period));
                }
                animationPositions[length - 1] = contourIsLoop ? animationPositions[0] : radius * new Vector2(Mathf.Cos((length - 1) * phase + time * period), Mathf.Sin((length - 1) * phase + time * period));
            }
        }
    }
}
