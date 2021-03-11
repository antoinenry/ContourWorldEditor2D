using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(UnitVectorAttribute))]
public class UnitVectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.Vector3)
        {
            Vector3 vectorValue = property.vector3Value;
            EditorGUI.BeginChangeCheck();
            vectorValue = EditorGUI.Vector3Field(position, label.text, property.vector3Value);
            if (EditorGUI.EndChangeCheck())
                property.vector3Value = AdjustVector(vectorValue);
        }
        else
            EditorGUI.HelpBox(position, "Use UnitVector attribute with Vector3", MessageType.Error);
    }

    private Vector3 AdjustVector(Vector3 value)
    {
        UnitVectorAttribute unitVector = attribute as UnitVectorAttribute;
        Vector3 adjustedValue = value;
        Vector3 clamped = value;
        switch (unitVector.autoAdjust)
        {
            case UnitVectorAttribute.AutoAdjustBehaviour.Normalize:
                adjustedValue = value.normalized;
                break;
            case UnitVectorAttribute.AutoAdjustBehaviour.AdjustX:
                clamped.x = 0f;
                clamped = Vector3.ClampMagnitude(clamped, 1f);
                adjustedValue = new Vector3(AdjustCoordinate(adjustedValue.x, clamped.y, clamped.z), clamped.y, clamped.z);
                break;
            case UnitVectorAttribute.AutoAdjustBehaviour.AdjustY:
                clamped.y = 0f;
                clamped = Vector3.ClampMagnitude(clamped, 1f);
                adjustedValue = new Vector3(clamped.x, AdjustCoordinate(adjustedValue.y, clamped.x, clamped.z), clamped.z);
                break;
            case UnitVectorAttribute.AutoAdjustBehaviour.AdjustZ:
                clamped.z = 0f;
                clamped = Vector3.ClampMagnitude(clamped, 1f);
                adjustedValue = new Vector3(clamped.x, clamped.y, AdjustCoordinate(adjustedValue.z, clamped.x, clamped.y));
                break;
        }
        return adjustedValue;
    }

    private float AdjustCoordinate(float coord, float other1, float other2)
    {
        return Mathf.Sign(coord) * Mathf.Sqrt(1f -Mathf.Pow(other1, 2f) - Mathf.Pow(other2, 2f));
    }
}
