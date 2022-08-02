using System;
using System.Collections.Generic;
using UnityEngine;

public class ContourAnimationBuilder : ContourBuilder
{
    private struct SubAnimationBuilder
    {
        public ContourShape contour;
        public ContourAnimationReader reader;

        public void ApplyAnimation()
        {
            int contourLength = contour != null ? contour.Length : 0;
            for (int i =  0; i < contourLength; i++)
            {
                if (!contour.GetPoint(i).isStatic)
                    contour.SetPosition(i, reader.AnimatedPositions[i]);
            }
        }
    }

    private SubAnimationBuilder[] subBuilders;

    public override void RebuildAll()
    {
        int blueprintCount = blueprints != null ? blueprints.Count : 0;
        subBuilders = new SubAnimationBuilder[blueprintCount];
        ResetReaders();
        if (blueprintCount != readers.Count) Debug.LogError("Blueprints and readers mismatch");
        for (int bpi = 0; bpi < blueprintCount; bpi++)
        {
            ContourBlueprint bp = blueprints[bpi];
            subBuilders[bpi] = new SubAnimationBuilder()
            {
                reader = readers[bpi] != null ? readers[bpi] as ContourAnimationReader : null,
                contour = bp != null ? bp.shape : null
            };
        }
    }

    protected override bool CanBuildFrom(ContourReader reader)
    {
        return reader != null && reader is ContourAnimationReader && reader.ReadSuccess;
    }

    public override void Build()
    {
        if (!Application.isPlaying) base.Build();
    }

    private void FixedUpdate()
    {
        if (readers == null || subBuilders == null) RebuildAll();
        float fixedTime = Time.fixedTime;
        foreach (ContourAnimationReader reader in readers)
            reader.Animate(fixedTime);
        foreach (SubAnimationBuilder sub in subBuilders)
            sub.ApplyAnimation();
    }
}
