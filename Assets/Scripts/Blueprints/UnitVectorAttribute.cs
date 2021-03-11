using UnityEngine;

public class UnitVectorAttribute : PropertyAttribute
{
    public AutoAdjustBehaviour autoAdjust;
    public enum AutoAdjustBehaviour { Normalize, AdjustX, AdjustY, AdjustZ }

    public UnitVectorAttribute()
    {
        autoAdjust = AutoAdjustBehaviour.Normalize;
    }

    public UnitVectorAttribute(AutoAdjustBehaviour adjustMode)
    {
        autoAdjust = adjustMode;
    }
}
