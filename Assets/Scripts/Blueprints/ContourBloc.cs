using System;
using System.Collections.Generic;
using UnityEngine;

public class ContourBloc : MonoBehaviour
{
    [Serializable]
    private struct PointOccurence
    {
        public int contourIndex;
        public int indexInContour;
    }

    [Serializable]
    private struct Point
    {
        public Vector2 position;
        public List<PointOccurence> occurences;

        public int OccurenceCount => occurences == null ? 0 : occurences.Count;
    }

    [SerializeField] private List<Point> points;
    [SerializeField] private Point bufferPoint;

    public ContourBlueprint[] Contours;

    private void Reset()
    {
        points = new List<Point>();
        bufferPoint = new Point() { occurences = new List<PointOccurence>() };
    }

    public int PointCount => points == null ? 0 : points.Count;

    public List<Vector2> GetPositions()
    {
        if (points == null) return null;
        else return points.ConvertAll(pt => pt.position);
    }

    public Vector2 GetPosition(int index)
    {
        if (points == null || index < 0 || index >= points.Count)
        {
            Debug.LogError("Point index is out of bounds.");
            return Vector2.zero;
        }
        
        return points[index].position;
    }

    public int GetContourCount()
    {
        int maxContourIndex = -1;
        // Find max contour index in point occurences
        for (int pti = 0, ptCount = PointCount; pti < ptCount; pti++)
            for (int occi = 0, occCount = points[pti].OccurenceCount; occi < occCount; occi++)
                maxContourIndex = Mathf.Max(maxContourIndex, points[pti].occurences[occi].contourIndex);
        // Also search buffer point
        for (int occi = 0, occCount = bufferPoint.OccurenceCount; occi < occCount; occi++)
            maxContourIndex = Mathf.Max(maxContourIndex, bufferPoint.occurences[occi].contourIndex);
        // Return count
        return maxContourIndex + 1;
    }

    public void AddPoint(Vector2 position)
    {
        // Create point at position, not in any contour (no occurences)
        if (points == null) points = new List<Point>(1);
        Point newPoint = new Point() { position = position, occurences = new List<PointOccurence>() };
        points.Add(newPoint);
    }

    public void MovePoint(int pointIndex, Vector2 position)
    {
        if (points == null || pointIndex < 0 || pointIndex >= points.Count)
        {
            Debug.LogError("Point index is out of bounds.");
            return;
        }
        Point movedPoint = points[pointIndex];
        movedPoint.position = position;
        points[pointIndex] = movedPoint;
    }

    public List<int> GetContoursWithPoint(int pointIndex)
    {
        if (pointIndex < 0 || pointIndex >= PointCount) return null;
        return points[pointIndex].occurences.ConvertAll(occ => occ.contourIndex);
    }

    public List<int> GetPointsInContour(int contourIndex, bool includeUndefinedPoints)
    {
        List<int> pointIndices = new List<int>();
        if (contourIndex < 0) return pointIndices;
        // Find contour index in point occurences
        for (int pti = 0, ptCount = PointCount; pti < ptCount; pti++)
            for (int occi = 0, occCount = points[pti].OccurenceCount; occi < occCount; occi++)
                if (points[pti].occurences[occi].contourIndex == contourIndex)
                {
                    int indexInContour = points[pti].occurences[occi].indexInContour;
                    // Adjust contour length
                    while (pointIndices.Count <= indexInContour) pointIndices.Add(-1);
                    pointIndices[indexInContour] = pti;
                }
        if (includeUndefinedPoints)
        {
            // Also search buffer point
            for (int occi = 0, occCount = bufferPoint.OccurenceCount; occi < occCount; occi++)
                if (bufferPoint.occurences[occi].contourIndex == contourIndex)
                {
                    int indexInContour = bufferPoint.occurences[occi].indexInContour;
                    // Adjust contour length
                    while (pointIndices.Count <= indexInContour) pointIndices.Add(-1);
                }
        }
        else
        {
            // Remove gaps
            pointIndices.RemoveAll(pti => pti == -1);
        }
        return pointIndices;
    }

    public void AddPointToContour(int contourIndex, int pointIndex, bool preserveLoop = true)
    {
        // Add a point at the end of a contour
        if (contourIndex < 0 || pointIndex < 0 || pointIndex >= PointCount) return;
        // Preserve loop - unless specified otherwise
        if (preserveLoop && IsContourLooped(contourIndex))
        {
            RemovePointFromContour(-1, contourIndex);
            List<int> pointsInContour = GetPointsInContour(contourIndex, false);
            int contourLength = pointsInContour.Count;
            ReplacePointInContour(contourIndex, contourLength - 1, pointIndex);
            LoopContour(contourIndex, true);
        }
        else
        {
            List<int> pointsInContour = GetPointsInContour(contourIndex, true);
            // If contour ends with undefined points, replace occurence instead of creating a new one
            int indexInContour = pointsInContour.Count;
            bool replaceUndefinedPoint = false;
            while (indexInContour > 0 && pointsInContour[indexInContour - 1] == -1)
            {
                replaceUndefinedPoint = true;
                indexInContour--;
            }
            if (replaceUndefinedPoint)
                ReplacePointInContour(contourIndex, indexInContour, pointIndex);
            else
                points[pointIndex].occurences.Add(new PointOccurence() { contourIndex = contourIndex, indexInContour = indexInContour });
        }
    }

    public void InsertPointInContour(int contourIndex, int insertAt, int pointIndex)
    {
        // Add a point at a specific place in a contour
        if (pointIndex < 0 || pointIndex >= PointCount) return;
        List<int> contourPoints = GetPointsInContour(contourIndex, true);
        if (insertAt < 0 || insertAt >= contourPoints.Count)
        {
            Debug.LogError("Index out of bounds");
            return;
        }
        // Unless contour has undefined point at this place, we need to make place in contour for new point
        if (contourPoints[insertAt] != -1)
        {
            for (int pti = 0, pointCount = PointCount; pti < pointCount; pti++)
            {
                int occurenceCount = points[pti].OccurenceCount;
                for (int occi = 0; occi < occurenceCount; occi++)
                {
                    // Make place = change occurences that are later in the contour
                    PointOccurence occ = points[pti].occurences[occi];
                    if (occ.contourIndex == contourIndex && occ.indexInContour >= insertAt)
                    {
                        occ.indexInContour++;
                        points[pti].occurences[occi] = occ;
                    }
                }
            }
        }
        // Add contour to point
        points[pointIndex].occurences.Add(new PointOccurence() { contourIndex = contourIndex, indexInContour = insertAt });
    }

    public void RemovePointFromContour(int pointIndex, int contourIndex, bool preserveLoop = true)
    {
        // Remove all point occurence in contour
        if (points == null || pointIndex < -1 || pointIndex >= points.Count) return;
        // Removing one point from one contour impacts all the points that are following (higher index) in this contour, including undefined points
        List<int> pointIndices = GetPointsInContour(contourIndex, true);
        if (pointIndices.Count == 0) return;
        // If contour is looped and we break it by removing the point, restore it later - unless specified otherwise
        bool restoreLoop = preserveLoop && IsContourLooped(contourIndex) && pointIndex == pointIndices[0];
        // Ensure we get all occurences of the removed point in the selected contour, as there can be several
        int findPointIndexInContour = pointIndices.IndexOf(pointIndex);
        while (findPointIndexInContour != -1)
        {
            // Update following points in contour
            for (int i = findPointIndexInContour, ptCount = pointIndices.Count; i < ptCount; i++)
            {
                int pti = pointIndices[i];
                Point follow_pt = pti == -1 ? bufferPoint : points[pti];
                for (int occi = 0, occCount = follow_pt.OccurenceCount; occi < occCount; occi++)
                {
                    PointOccurence occ = follow_pt.occurences[occi];
                    if (occ.contourIndex == contourIndex && occ.indexInContour > findPointIndexInContour)
                    {
                        occ.indexInContour--;
                        if (pti != -1) points[pti].occurences[occi] = occ;
                        else bufferPoint.occurences[occi] = occ;
                    }
                }
            }
            // Remove contour from point
            if (pointIndex != -1)
                points[pointIndex].occurences.RemoveAll(occ => occ.contourIndex == contourIndex);
            else
                bufferPoint.occurences.RemoveAll(occ => occ.contourIndex == contourIndex);
            // Get updated contour to see if the point to removed is still mentionned in it
            pointIndices = GetPointsInContour(contourIndex, true);
            findPointIndexInContour = pointIndices.IndexOf(pointIndex);
        }
        // Loop contour back again if needed
        if (restoreLoop) LoopContour(contourIndex, true);
    }

    public void RemovePointFromAllContours(int pointIndex)
    {
        if (points == null || pointIndex < 0 || pointIndex >= points.Count) return;
        List<int> contourIndices = GetContoursWithPoint(pointIndex);
        RemovePointFromContours(pointIndex, contourIndices);
    }

    public void RemovePointFromContours(int pointIndex, List<int> contourIndices)
    {
        if (contourIndices == null) return;
        foreach (int contourIndex in contourIndices)
            RemovePointFromContour(pointIndex, contourIndex);
    }

    public void DetachPointFromContours(int pointIndex, List<int> contourIndices)
    {
        if (pointIndex < 0 || pointIndex > PointCount || contourIndices == null) return;
        Point originalPoint = points[pointIndex];
        List<PointOccurence> occurences = originalPoint.occurences;
        List<PointOccurence> originalOccurences = new List<PointOccurence>();
        List<PointOccurence> duplicateOccurences = new List<PointOccurence>();
        // Move occurences with specified contourIndex
        foreach(PointOccurence occ in occurences)
        {
            if (contourIndices.Contains(occ.contourIndex)) duplicateOccurences.Add(occ);
            else originalOccurences.Add(occ);
        }
        // Case where no point is detached
        if (duplicateOccurences.Count == 0) return;
        // Duplicate point in bloc
        originalPoint.occurences = originalOccurences;
        points[pointIndex] = originalPoint;
        Point duplicatePoint = new Point() { position = originalPoint.position, occurences = duplicateOccurences };
        points.Add(duplicatePoint);
    }

    public void SetContourLength(int contourIndex, int newLength, bool preserveLoop = true)
    {
        if (newLength < 0 || contourIndex < 0) return;
        // Remove undefined points before changing length
        RemovePointFromContour(-1, contourIndex);
        // Get clean contour
        List<int> pointIndices = GetPointsInContour(contourIndex, false);
        int currentContourLength = pointIndices.Count;
        // First case: decreasing contour length
        if (newLength < currentContourLength)
        {
            // If contour is looped, preserve loop - unless specified otherwise
            if (preserveLoop && IsContourLooped(contourIndex))
            {
                // Remove points from contour when index in contour exceeds new length (-1 to take the repeted point in loop)
                foreach (int pti in pointIndices)
                    points[pti].occurences.RemoveAll(occ => occ.contourIndex == contourIndex && occ.indexInContour >= newLength - 1);
                // Restore loop
                LoopContour(contourIndex, true);
            }
            else
            {
                // Remove points from contour when index in contour exceeds new length
                foreach (int pti in pointIndices)
                    points[pti].occurences.RemoveAll(occ => occ.contourIndex == contourIndex && occ.indexInContour >= newLength);
            }
        }
        // Second case: increasing contour length 
        else if (newLength > currentContourLength)
        {
            // Add undefined point ocurrences to match contour length
            for (int i = currentContourLength; i < newLength; i++)
                bufferPoint.occurences.Add(new PointOccurence() { contourIndex = contourIndex, indexInContour = i } );
        }
    }

    public void DestroyPoint(int pointIndex)
    {
        // Destroys a point, removing it from the bloc and affecting contours which contain it
        if (points == null || pointIndex < 0 || pointIndex >= points.Count) return;
        Point destroyedPoint = points[pointIndex];
        // Remove point from contours
        if (destroyedPoint.OccurenceCount > 0)
            RemovePointFromAllContours(pointIndex);
        // Remove point from bloc
        points.RemoveAt(pointIndex);
    }

    public void DestroyPoints(List<int> pointIndices)
    {
        // Destroying multiple points
        if (points == null || pointIndices == null) return;
        for(int i = 0, iend = pointIndices.Count; i < iend; i++)
        {
            DestroyPoint(pointIndices[i]);
            for (int j = i; j < iend; j++)
            {
                // Avoid doublons
                if (pointIndices[j] == pointIndices[i]) pointIndices[j] = -1;
                // Shift indices that are higher than the one we destroyed
                else if (pointIndices[j] > pointIndices[i]) pointIndices[j]--;
            }
        }
    }

    public void DestroyAllContourlessPoints()
    {
        if (PointCount == 0) return;
        points.RemoveAll(pt => pt.OccurenceCount == 0);
    }

    public void ReplacePointInContour(int contourIndex, int indexInContour, int newPointIndex)
    {
        List<int> pointIndices = GetPointsInContour(contourIndex, true);
        if (pointIndices == null || indexInContour < 0 || indexInContour >= pointIndices.Count)
        {
            Debug.LogError("Index out of bounds");
            return;
        }
        if (newPointIndex < -1 || newPointIndex >= PointCount) return;
        // Remove current point in contour
        int pointIndex = pointIndices[indexInContour];
        if (pointIndex != -1)
        {
            // Defined point
            if (points[pointIndex].occurences != null) points[pointIndex].occurences.RemoveAll(occ => occ.contourIndex == contourIndex && occ.indexInContour == indexInContour);
        }
        else
        {
            // Undefined point
            if (bufferPoint.occurences != null) bufferPoint.occurences.RemoveAll(occ => occ.contourIndex == contourIndex && occ.indexInContour == indexInContour);
        }
        // Set new point
        if (newPointIndex != -1)
        {
            // Defined point
            Point newPoint = points[newPointIndex];
            if (newPoint.occurences == null) newPoint.occurences = new List<PointOccurence>(1);
            newPoint.occurences.Add(new PointOccurence() { contourIndex = contourIndex, indexInContour = indexInContour });
            points[newPointIndex] = newPoint;
        }
        else
        {
            // Undefined point
            if (bufferPoint.occurences == null) bufferPoint.occurences = new List<PointOccurence>(1);
            bufferPoint.occurences.Add(new PointOccurence() { contourIndex = contourIndex, indexInContour = indexInContour });
        }
    }  

    public void AddContour()
    {
        // Create empty contour (contour with one undefined point)
        int contourCount = GetContourCount();
        bufferPoint.occurences.Add(new PointOccurence() { contourIndex = contourCount, indexInContour = 0 });
    }

    public void RemoveContourAt(int contourIndex)
    {
        // Remove all points from the contour
        List<int> pointIndices = GetPointsInContour(contourIndex, true);
        foreach(int pti in pointIndices) RemovePointFromContour(pti, contourIndex);
        // Decrement higher contour indices in all point occurences, including buffer point
        for (int pti = -1, ptCount = PointCount; pti < ptCount; pti++)
        {
            Point point = pti == -1 ? bufferPoint : points[pti];
            int occurenceCount = point.OccurenceCount;
            if (occurenceCount == 0) continue;
            List<PointOccurence> occurences = point.occurences;
            bool changeOccurences = false;
            for (int occi = 0; occi < occurenceCount; occi++)
            {
                PointOccurence occ = occurences[occi];
                if (occ.contourIndex > contourIndex)
                {
                    changeOccurences = true;
                    occ.contourIndex--;
                    occurences[occi] = occ;
                }
            }
            if (changeOccurences)
            {
                point.occurences = occurences;
                if (pti == -1) bufferPoint = point;
                else points[pti] = point;
            }
        }
    }

    public void RemoveContoursAt(List<int> contourIndices)
    {
        // Remove multiple contours
        if (contourIndices == null) return;
        for (int i = 0, iend = contourIndices.Count; i < iend; i++)
        {
            RemoveContourAt(contourIndices[i]);
            for (int j = 0; j < iend; j++)
            {
                if (contourIndices[j] > contourIndices[i])
                    contourIndices[j]--;
            }
        }
    }

    public void MergePoints(List<int> pointIndices)
    {
        if (points == null) return;
        // Merge 2+ points into one point
        int mergeCount = pointIndices == null ? 0 : pointIndices.Count;
        if (mergeCount < 2) return;
        Point newPoint = new Point() { position = points[pointIndices[0]].position, occurences = new List<PointOccurence>() };
        foreach(int pti in pointIndices) newPoint.occurences.AddRange(points[pti].occurences);
        // Add new point
        points.Add(newPoint);
        // Destroy merged points
        DestroyPoints(pointIndices);
    }

    public void ReverseContour(int contourIndex)
    {
        List<int> pointIndices = GetPointsInContour(contourIndex, true);
        for (int i = 0, iend = pointIndices.Count; i < iend; i++)
        {
            if (i == iend - 1 - i) continue;
            ReplacePointInContour(contourIndex, i, pointIndices[iend - 1 - i]);
        }
    }

    public bool IsContourLooped(int contourIndex)
    {
        List<int> pointIndices = GetPointsInContour(contourIndex, false);
        int contourLength = pointIndices.Count;
        if (contourLength < 3) return false;
        else return pointIndices[0] == pointIndices[contourLength - 1];
    }

    public void LoopContour(int contourIndex, bool loop)
    {
        RemovePointFromContour(-1, contourIndex);
        List<int> pointIndices = GetPointsInContour(contourIndex, false);
        int contourLength = pointIndices.Count;
        if (contourLength < 3 || IsContourLooped(contourIndex) == loop) return;
        if (loop) AddPointToContour(contourIndex, pointIndices[0], false);
        else SetContourLength(contourIndex, contourLength - 1, false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Vector3 blocPosition = transform.position;
        int pointCount = PointCount;
        int contourCount = GetContourCount();
        bool[] pointIsUsed = new bool[pointCount];
        for (int cti = 0; cti < contourCount; cti++)
        {
            List<int> pointIndices = GetPointsInContour(cti, false);
            int contourLength = pointIndices.Count;
            if (contourLength == 0) continue;
            if (contourLength > 1)
                for (int i = 0; i < contourLength - 1; i++)
                {
                    Gizmos.DrawLine((Vector3)points[pointIndices[i]].position + blocPosition, (Vector3)points[pointIndices[i+1]].position + blocPosition);
                    pointIsUsed[pointIndices[i]] = true;
                    pointIsUsed[pointIndices[i+1]] = true;
                }
        }
        for (int pti = 0; pti < pointCount; pti++)
            if (!pointIsUsed[pti])
                Gizmos.DrawIcon((Vector3)points[pti].position + blocPosition, "cross.png");
    }
}