using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ContourBlueprint : MonoBehaviour
{

    public Vector2[] positions;
    public ContourMaterial material;
    public BlueprintChanges changes;
    public string changedParameters;

    [Flags]
    public enum BlueprintChanges { None = 0, PositionMoved = 1, LengthChanged = 2, ParameterChanged = 4 }

    public void SetPositions(List<Vector2> newPositions)
    {
        if (positions == null)
        {
            positions = newPositions != null ? newPositions.ToArray() : new Vector2[0];
            changes |= BlueprintChanges.LengthChanged;
        }
        else
        {
            int newLength = newPositions != null ? newPositions.Count : 0;
            if (positions.Length == newLength)
            {
                if (Enumerable.SequenceEqual(newPositions, positions))
                    return;
                else
                    changes |= BlueprintChanges.PositionMoved;
            }
            else
            {
                changes |= BlueprintChanges.LengthChanged;
            }
            positions = newLength != 0 ? newPositions.ToArray() : new Vector2[0];
        }        
    }
}