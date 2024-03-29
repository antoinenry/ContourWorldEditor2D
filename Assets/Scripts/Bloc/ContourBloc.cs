﻿using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(2)]
[ExecuteAlways]
public class ContourBloc : MonoBehaviour
{
    [Serializable]
    private struct PointOccurence
    {
        public int contourIndex;
        public int indexInContour;
    }

    [Serializable]
    private struct BlocPoint
    {
        public ContourPoint pointInstance;
        public List<PointOccurence> occurences;

        public Vector2 Position => pointInstance.position;
        public int OccurenceCount => occurences == null ? 0 : occurences.Count;
    }

    [Flags]
    public enum BlocChanges { None = 0, ContourAdded = 1, ContourRemoved = 2 }

    [SerializeField] private List<BlocPoint> points;
    [SerializeField] private BlocPoint bufferPoint;
    [SerializeField] private List<ContourShape> contourShapes;

    public BlocChanges changes { get; private set; }

    public List<ContourShape> ContourShapes => contourShapes != null ? contourShapes : new List<ContourShape>();

    public int PointCount => points == null ? 0 : points.Count;

    private void Reset()
    {
        points = new List<BlocPoint>();
        bufferPoint = new BlocPoint() { pointInstance = null, occurences = new List<PointOccurence>() };
        contourShapes = new List<ContourShape>();
    }

    private void OnEnable()
    {
        SetPointInstances();
    }

    private void LateUpdate()
    {
        changes = BlocChanges.None;
        //if (contourShapes != null)
        //    foreach (ContourShape shape in contourShapes)
        //        if (shape != null) shape.changes = ContourShape.Changes.None;
    }

    public void SetPointInstances()
    {
        if (points == null) return;
        foreach (BlocPoint pt in points)
        {
            ContourPoint blocPointInstance = pt.pointInstance;
            foreach (PointOccurence occ in pt.occurences)
            {
                ContourPoint contourPointInstance = contourShapes[occ.contourIndex].GetPoint(occ.indexInContour);
                if (contourPointInstance != blocPointInstance)
                    contourShapes[occ.contourIndex].ReplacePoint(occ.indexInContour, blocPointInstance);
            }
        }
    }

    public List<Vector2> GetLocalPositions()
    {
        if (points == null) return new List<Vector2>();
        else return points.ConvertAll(pt => pt.Position);
    }

    public List<Vector3> GetWorldPositions()
    {
        if (points == null) return new List<Vector3>();
        else
        {
            Vector3 blocPosition = transform.position;
            Quaternion blocRotation = transform.rotation;
            return points.ConvertAll(pt => blocPosition + blocRotation * pt.Position);
        }
    }

    public Vector2 GetLocalPosition(int index)
    {
        if (points == null || index < 0 || index >= points.Count)
        {
            Debug.LogError("Point index is out of bounds.");
            return Vector2.zero;
        }
        
        return points[index].Position;
    }

    public Vector3 GetWorldPosition(int index)
    {
        if (points == null || index < 0 || index >= points.Count)
        {
            Debug.LogError("Point index is out of bounds.");
            return Vector2.zero;
        }

        return transform.rotation * points[index].Position + transform.position;
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

    public List<List<int>> GetAllContours(bool includeUndefinedPoints)
    {
        List<List<int>> contours = new List<List<int>>();
        for (int pti = includeUndefinedPoints ? -1 : 0, ptCount = PointCount; pti < ptCount; pti++)
        {
            List<PointOccurence> occurences = pti == -1 ? bufferPoint.occurences : points[pti].occurences;
            if (occurences == null) continue;
            foreach(PointOccurence occ in occurences)
            {
                int contourIndex = occ.contourIndex, indexInContour = occ.indexInContour;
                // Adjust contour list size if needed
                if (contours.Count <= contourIndex)
                {
                    contours.Capacity = contourIndex + 1;
                    while (contours.Count <= contourIndex) contours.Add(new List<int>());
                }
                // Current contour in list
                List<int> contour = contours[contourIndex];
                // Adjust contour size if needed
                if (contour.Count <= indexInContour)
                {
                    contour.Capacity = indexInContour + 1;
                    while (contour.Count <= indexInContour) contour.Add(-1);
                }
                // Add point index to contour
                contour[indexInContour] = pti;
                contours[contourIndex] = contour;
            }
        }
        // Remove gaps if specified
        if (!includeUndefinedPoints)
            foreach (List<int> contour in contours)
                contour.RemoveAll(pti => pti == -1);
        return contours;
    }

    public List<int> GetContour(int contourIndex, bool includeUndefinedPoints)
    {
        List<int> pointIndices = new List<int>();
        if (contourIndex < 0) return pointIndices;
        // Find contour index in point occurences
        for (int pti = includeUndefinedPoints ? -1 : 0, ptCount = PointCount; pti < ptCount; pti++)
        {
            BlocPoint point = pti == -1 ? bufferPoint : points[pti];
            for (int occi = 0, occCount = point.OccurenceCount; occi < occCount; occi++)
                if (point.occurences[occi].contourIndex == contourIndex)
                {
                    int indexInContour = point.occurences[occi].indexInContour;
                    // Adjust contour length
                    if (pointIndices.Count <= indexInContour)
                    {
                        pointIndices.Capacity = indexInContour + 1;
                        while (pointIndices.Count <= indexInContour) pointIndices.Add(-1);
                    }
                    // Set contour
                    pointIndices[indexInContour] = pti;
                }
        }
        // Remove gaps if specified
        if (!includeUndefinedPoints) pointIndices.RemoveAll(pti => pti == -1);
        return pointIndices;
    }

    public void AddPoint(Vector2 pointPosition, bool isWorldSpace = false)
    {
        // Create point at position, not in any contour (no occurences)
        if (points == null) points = new List<BlocPoint>(1);
        Vector2 localPointPosition = isWorldSpace ? (Vector2)(Quaternion.Inverse(transform.rotation) * (pointPosition - (Vector2)transform.position)) : pointPosition;
        BlocPoint newPoint = new BlocPoint() { pointInstance = new ContourPoint(localPointPosition), occurences = new List<PointOccurence>() };
        points.Add(newPoint);
    }

    public void MovePoint(int pointIndex, Vector2 position)
    {
        // Set point position in bloc
        if (points == null || pointIndex < 0 || pointIndex >= points.Count)
        {
            Debug.LogError("Point index is out of bounds.");
            return;
        }
        BlocPoint movedPoint = points[pointIndex];
        movedPoint.pointInstance.position = position;
        //points[pointIndex] = movedPoint;
        // Set point position in contours
        if (movedPoint.OccurenceCount > 0)
            foreach (PointOccurence occ in movedPoint.occurences)
                contourShapes[occ.contourIndex].changes |= ContourShape.Changes.PositionMoved;
                //contourShapes[occ.contourIndex].SetPosition(occ.indexInContour, position);
    }

    public List<int> GetContoursWithPoint(int pointIndex)
    {
        if (pointIndex < 0 || pointIndex >= PointCount) return null;
        return points[pointIndex].occurences.ConvertAll(occ => occ.contourIndex);
    }
    
    public void AddPointToContour(int contourIndex, int pointIndex, bool preserveLoop = true)
    {
        if (contourIndex < 0 || pointIndex < 0 || pointIndex >= PointCount) return;        
        // Looped contours are processed differently
        if (preserveLoop && IsContourLooped(contourIndex))
        {
            // Insert point in a loop
            RemovePointFromContour(-1, contourIndex);
            List<int> pointsInContour = GetContour(contourIndex, false);
            int contourLength = pointsInContour.Count;
            ReplacePointInContour(contourIndex, contourLength - 1, pointIndex);
            LoopContour(contourIndex, true);
        }
        else
        {
            // Add point at the end of a contour
            List<int> pointsInContour = GetContour(contourIndex, true);
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
            {
                // Add contour to point
                points[pointIndex].occurences.Add(new PointOccurence() { contourIndex = contourIndex, indexInContour = indexInContour });
                // Add position to contour
                contourShapes[contourIndex].AddPoint(points[pointIndex].pointInstance);
            }
        }
    }

    public void InsertPointInContour(int contourIndex, int insertAt, int pointIndex)
    {
        // Add a point at a specific place in a contour
        if (pointIndex < 0 || pointIndex >= PointCount) return;
        List<int> contourPoints = GetContour(contourIndex, true);
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
        // Add position to contour
        contourShapes[contourIndex].InsertPoint(insertAt, points[pointIndex].pointInstance);
    }

    public void InsertPointInContours(List<int> contourIndices, int pointA, int pointB)
    {
        // Insert a point between to points
        if (contourIndices == null) return;
        int pointCount = PointCount;
        if (pointA == -1 || pointA >= pointCount || pointB < 0 || pointB >= pointCount) return;
        int newPointIndex = -1;
        foreach(int contourIndex in contourIndices)
        {
            // Find index in contour between points A and B
            int insertAt;
            do
            {
                List<PointOccurence> occurencesA = points[pointA].occurences;
                List<PointOccurence> occurencesB = points[pointB].occurences;
                if (occurencesA == null || occurencesB == null) return;
                insertAt = -1;
                foreach (PointOccurence occA in occurencesA)
                {
                    // Search point A occurence in contour
                    if (occA.contourIndex == contourIndex)
                    {
                        // Find point B occurence that's next to point A in contour
                        int findOccB = occurencesB.FindIndex(occ => occ.contourIndex == contourIndex
                        && (occ.indexInContour == occA.indexInContour + 1 || occ.indexInContour == occA.indexInContour - 1));
                        if (findOccB != -1)
                        {
                            // Insert index is higher index between indices A and B
                            PointOccurence occB = occurencesB[findOccB];
                            insertAt = occB.indexInContour > occA.indexInContour ? occB.indexInContour : occA.indexInContour;
                            // If inserted point has not been created yet, create it
                            if (newPointIndex == -1)
                            {
                                newPointIndex = PointCount;
                                AddPoint((points[pointA].Position + points[pointB].Position) / 2f);
                            }
                            // Insert new point in contour
                            InsertPointInContour(contourIndex, insertAt, newPointIndex);
                            break;
                        }
                    }
                }
            }
            while (insertAt != -1);
            
        }
    }

    public void RemovePointFromContour(int pointIndex, int contourIndex, bool preserveLoop = true)
    {
        // Remove all point occurence in contour
        if (points == null || pointIndex < -1 || pointIndex >= points.Count) return;
        // Removing one point from one contour impacts all the points that are following (higher index) in this contour, including undefined points
        List<int> pointIndices = GetContour(contourIndex, true);
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
                BlocPoint follow_pt = pti == -1 ? bufferPoint : points[pti];
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
            // Remove contour from point, and position from contour
            if (pointIndex != -1)
            {
                points[pointIndex].occurences.RemoveAll(occ => occ.contourIndex == contourIndex);
                contourShapes[contourIndex].RemovePoint(findPointIndexInContour);
            }
            // Or remove point from buffer
            else
                bufferPoint.occurences.RemoveAll(occ => occ.contourIndex == contourIndex);
            // Get updated contour to see if the point to removed is still mentionned in it
            pointIndices = GetContour(contourIndex, true);
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
        // Note: this has no impact on contour positions so we just need to work on point occurences
        if (pointIndex < 0 || pointIndex > PointCount || contourIndices == null) return;
        BlocPoint originalPoint = points[pointIndex];
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
        BlocPoint duplicatePoint = new BlocPoint() { pointInstance = new ContourPoint(originalPoint.Position), occurences = duplicateOccurences };
        points.Add(duplicatePoint);
        // Replace point instance in detached contour with new point instance
        foreach (PointOccurence occ in duplicateOccurences)
            contourShapes[occ.contourIndex].ReplacePoint(occ.indexInContour, duplicatePoint.pointInstance);
    }

    public void SetContourLength(int contourIndex, int newLength, bool preserveLoop = true)
    {
        if (newLength < 0 || contourIndex < 0) return;
        // Remove undefined points before changing length
        RemovePointFromContour(-1, contourIndex);
        // Get clean contour
        List<int> pointIndices = GetContour(contourIndex, false);
        int currentContourLength = pointIndices.Count;
        // First case: decreasing contour length
        if (newLength < currentContourLength)
        {
            // Remove positions from contour
            contourShapes[contourIndex].RemovePoints(newLength, currentContourLength - newLength);
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
            // Add undefined point ocurrences to match contour length (this doesn't affect actual contour positions)
            for (int i = currentContourLength; i < newLength; i++)
                bufferPoint.occurences.Add(new PointOccurence() { contourIndex = contourIndex, indexInContour = i } );
        }
    }

    public void DestroyPoint(int pointIndex)
    {
        // Destroys a point, removing it from the bloc and affecting contours which contain it
        if (points == null || pointIndex < 0 || pointIndex >= points.Count) return;
        BlocPoint destroyedPoint = points[pointIndex];
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

    public void DestroyAllPointlessContours()
    {
        // Get rid of empty contours
        contourShapes.RemoveAll(contour => contour.Length == 0);
        // Destroy contour indices in bloc
        List<List<int>> contourPointIndices = GetAllContours(false);
        if (contourPointIndices == null) return;
        List<int> pointlessContourIndices = new List<int>();
        for (int cti = 0, ctcount = contourPointIndices.Count; cti < ctcount; cti++)
            if (contourPointIndices[cti].Count == 0) pointlessContourIndices.Add(cti);
        if (pointlessContourIndices.Count == 0) return;
        // Remove occurences and shift indices
        for (int pti = -1, ptCount = PointCount; pti < ptCount; pti++)
        {
            BlocPoint pt = pti == -1 ? bufferPoint : points[pti];
            List<PointOccurence> occurences = pt.occurences;
            if (occurences == null) continue;
            // Remove occurences if in in pointless contour
            occurences.RemoveAll(occ => pointlessContourIndices.Contains(occ.contourIndex));
            // Shift higher contours indices
            for (int occi = 0, occCount = occurences.Count; occi < occCount; occi++)
            {
                PointOccurence occ = occurences[occi];
                int decrementIndex = pointlessContourIndices.FindAll(cti => occ.contourIndex > cti).Count;
                occ.contourIndex -= decrementIndex;
            }
            // Set occurences
            pt.occurences = occurences;
            if (pti == -1) bufferPoint = pt;
            else points[pti] = pt;
        }
    }

    public void ReplacePointInContour(int contourIndex, int indexInContour, int newPointIndex)
    {
        List<int> pointIndices = GetContour(contourIndex, true);
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
            BlocPoint newPoint = points[newPointIndex];
            if (newPoint.occurences == null) newPoint.occurences = new List<PointOccurence>(1);
            newPoint.occurences.Add(new PointOccurence() { contourIndex = contourIndex, indexInContour = indexInContour });
            points[newPointIndex] = newPoint;
            // If point was undefined, add/insert a new point in contour
            if (pointIndex == -1)
            {
                if (indexInContour == contourShapes[contourIndex].Length)
                    contourShapes[contourIndex].AddPoint(newPoint.pointInstance);
                else
                    contourShapes[contourIndex].InsertPoint(indexInContour, newPoint.pointInstance);
            }
            // Set position in contour
            contourShapes[contourIndex].SetPosition(indexInContour, GetLocalPosition(newPointIndex));
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
        int oldContourCount = GetContourCount();
        bufferPoint.occurences.Add(new PointOccurence() { contourIndex = oldContourCount, indexInContour = 0 });
        if (contourShapes == null) contourShapes = new List<ContourShape>();
        ContourShape newContour = new ContourShape();
        contourShapes.Add(newContour);
        changes |= BlocChanges.ContourAdded;
    }

    public void RemoveContourAt(int contourIndex)
    {
        // Remove all points from the contour
        List<int> pointIndices = GetContour(contourIndex, true);
        foreach(int pti in pointIndices) RemovePointFromContour(pti, contourIndex);
        // Decrement higher contour indices in all point occurences, including buffer point
        for (int pti = -1, ptCount = PointCount; pti < ptCount; pti++)
        {
            BlocPoint point = pti == -1 ? bufferPoint : points[pti];
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
        // Remove contour
        contourShapes.RemoveAt(contourIndex);
        changes |= BlocChanges.ContourRemoved;
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
        changes |= BlocChanges.ContourRemoved;
    }

    public void MergePoints(List<int> pointIndices)
    {
        if (points == null) return;
        // Merge 2+ points into one point
        int mergeCount = pointIndices == null ? 0 : pointIndices.Count;
        if (mergeCount < 2) return;
        BlocPoint newPoint = new BlocPoint() { pointInstance = new ContourPoint(points[pointIndices[0]].Position), occurences = new List<PointOccurence>() };
        foreach (int pti in pointIndices)
        {
            List<PointOccurence> occurences = points[pti].occurences;
            // Copy all occurences to one same point
            newPoint.occurences.AddRange(occurences);
            // Set all corresponding points in contours to the same instance
            foreach (PointOccurence occ in occurences)
                contourShapes[occ.contourIndex].ReplacePoint(occ.indexInContour, newPoint.pointInstance);
        }
        // Add new point
        points.Add(newPoint);
        // Destroy merged points
        DestroyPoints(pointIndices);
    }

    public void ReverseContour(int contourIndex)
    {
        List<int> pointIndices = GetContour(contourIndex, true);
        for (int i = 0, iend = pointIndices.Count; i < iend; i++)
        {
            if (i == iend - 1 - i) continue;
            ReplacePointInContour(contourIndex, i, pointIndices[iend - 1 - i]);
        }
    }

    public bool IsContourLooped(int contourIndex)
    {
        List<int> pointIndices = GetContour(contourIndex, false);
        int contourLength = pointIndices.Count;
        if (contourLength < 3) return false;
        else return pointIndices[0] == pointIndices[contourLength - 1];
    }

    public void LoopContour(int contourIndex, bool loop)
    {
        RemovePointFromContour(-1, contourIndex);
        List<int> pointIndices = GetContour(contourIndex, false);
        int contourLength = pointIndices.Count;
        if (contourLength < 3 || IsContourLooped(contourIndex) == loop) return;
        if (loop) AddPointToContour(contourIndex, pointIndices[0], false);
        else SetContourLength(contourIndex, contourLength - 1, false);
    }

    public Vector3 GetContourNormal(int contourIndex)
    {
        return contourShapes[contourIndex].Normal;
    }

    public void SetContourNormal(int contourIndex, Vector3 normalValue)
    {
        contourShapes[contourIndex].Normal = normalValue;
    }

    #region Gizmos
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.gray;
    //    DrawContourGizmos();
    //}

    //private void DrawContourGizmos()
    //{
    //    Vector3 blocPosition = transform.position;
    //    Quaternion blocRotation = transform.rotation;
    //    // Draw contours
    //    if (contourShapes != null)
    //        foreach(ContourShape contour in contourShapes)
    //            for(int pti = 0, ptCount = contour.Length; pti < ptCount - 1; pti++)
    //                Gizmos.DrawLine(blocRotation * contour.GetPosition(pti) + blocPosition, blocRotation * contour.GetPosition(pti + 1) + blocPosition);
    //    // Draw unused points
    //    if (points != null)
    //        foreach(Point pt in points)
    //            if (pt.OccurenceCount == 0)
    //                Gizmos.DrawIcon(blocRotation * pt.position + blocPosition, "cross.png");
    //}
    #endregion
}