using System.Collections.Generic;
using UnityEngine;

public class ContourAnimationBuilder : ContourBuilder
{
    private AnimatedContour[] animatedContours;
    private AnimatedPoint[] animatedPoints;

    private struct AnimatedContour
    {
        private ContourShape shape;
        private AnimatedPoint[] animatedPoints;

        public AnimatedContour(ContourShape shape, ref List<AnimatedPoint> pointPool)
        {
            // Create contour animation using existing points, creates new points if needed
            this.shape = shape;
            int shapeLength = shape != null ? shape.Length : 0;
            this.animatedPoints = new AnimatedPoint[shapeLength];
            if (pointPool == null) pointPool = new List<AnimatedPoint>(shapeLength);
            for (int i = 0; i < shapeLength; i++)
            {
                // Find existing point matching position
                Vector2 pointPosition = shape.GetPosition(i);
                AnimatedPoint pointAnimation = pointPool.Find(pta => pta.position == pointPosition);
                // If no point was found, create one
                if (pointAnimation == null)
                {
                    pointAnimation = new AnimatedPoint(pointPosition);
                    pointPool.Add(pointAnimation);
                }
                // Contour animation now contains the point reference, that can be shared with other contour animations
                this.animatedPoints[i] = pointAnimation;
            }
        }

        public void AddPointOffset(Vector2[] offset)
        {
            if (offset == null || animatedPoints == null) return;
            for (int i = 0, iend = animatedPoints.Length; i < iend; i++)
            {
                if (i >= offset.Length) break;
                if (animatedPoints[i] == null) continue;
                animatedPoints[i].position += offset[i];
            }
        }

        public void AnimationUpdate()
        {
            if (shape == null) return;
            for (int i = 0, iend = shape.Length; i < iend; i++)
            {
                if (shape.CanAnimatePoint(i))
                    shape.SetPosition(i, animatedPoints[i].position);
            }
        }
    }

    private class AnimatedPoint
    {
        public Vector2 idlePosition;
        public Vector2 position;

        public AnimatedPoint(Vector2 position)
        {
            this.idlePosition = position;
            this.position = position;
        }
    }

    public override void RebuildAll()
    {
        // Reread all blueprints
        ResetReaders();
        // Set all animated point and contour
        UpdatePositions();
    }

    protected override bool CanBuildFrom(ContourReader reader)
    {
        return reader != null && reader is ContourAnimationReader;
    }

    protected override void UpdatePositions()
    {
        List<AnimatedContour> newAnimatedContours = new List<AnimatedContour>();
        List<AnimatedPoint> newAnimatedPoints = new List<AnimatedPoint>();
        if (blueprints != null) newAnimatedContours = blueprints.ConvertAll(bp => new AnimatedContour(bp != null ? bp.shape : null, ref newAnimatedPoints));
        animatedContours = newAnimatedContours.ToArray();
        animatedPoints = newAnimatedPoints.ToArray();
    }

    public override void Build()
    {
        return;
    }

    private void Start()
    {
        RebuildAll();
    }

    private void FixedUpdate()
    {
        if (blueprints != null && animatedPoints != null && animatedContours != null)
        {
            float time = Time.fixedTime;
            // Set every point to its default position
            foreach (AnimatedPoint apt in animatedPoints)
                apt.position = apt.idlePosition;
            // Start from here to add each movement
            int blueprintCount = blueprints.Count;
            for (int bpi = 0; bpi < blueprintCount; bpi++)
            {
                // Play each animation
                if (readers[bpi] == null || blueprints[bpi] == null) continue;
                ContourShape shape = blueprints[bpi].shape;
                if (shape == null) continue;
                ContourAnimationReader reader = readers[bpi] as ContourAnimationReader;
                reader.Animate(time);
                // Add movement to each concerned point
                AnimatedContour act = animatedContours[bpi];
                act.AddPointOffset(reader.animationPositions);
            }
            // Apply movement to each contour
            foreach (AnimatedContour act in animatedContours)
                act.AnimationUpdate();
        }
    }
}
