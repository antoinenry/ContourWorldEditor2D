using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ContourShape))]
public class ContourShapeDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty pointsArrayProperty = property.FindPropertyRelative("points");
        int pointCount = pointsArrayProperty != null ? pointsArrayProperty.arraySize : 0;
        return (2 + pointCount) * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
    }

    public override void OnGUI(Rect position, SerializedProperty contourShapeProperty, GUIContent label)
    {
        // Create rect for drawing each field
        Rect rect = position;
        rect.height = EditorGUIUtility.singleLineHeight;
        // Get points array
        SerializedProperty pointsArrayProperty = contourShapeProperty.FindPropertyRelative("points");
        int pointCount = pointsArrayProperty != null ? pointsArrayProperty.arraySize : 0;
        // Length field
        int editedPointCount = EditorGUI.IntField(rect, "Contour Length", pointCount);
        rect.y += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
        if (editedPointCount < 0) editedPointCount = 0;
        if (editedPointCount != pointCount)
        {
            pointsArrayProperty.arraySize = editedPointCount;
            pointCount = editedPointCount;
            SerializedProperty changesProperty = contourShapeProperty.FindPropertyRelative("changes");
            changesProperty.enumValueFlag = changesProperty.enumValueFlag | (int)ContourShape.Changes.LengthChanged;
        }
        // Points fields
        EditorGUI.indentLevel++;
        for (int i = 0; i < pointCount; i++)
        {
            SerializedProperty pointProperty = pointsArrayProperty.GetArrayElementAtIndex(i);
            if (pointProperty != null)
            {
                SerializedProperty positionProperty = pointProperty.FindPropertyRelative("position");
                Vector2 editedPointPosition = positionProperty.vector2Value;
                editedPointPosition = EditorGUI.Vector2Field(rect, "Point " + i, editedPointPosition);
                rect.y += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
                if (editedPointPosition != positionProperty.vector2Value)
                {
                    positionProperty.vector2Value = editedPointPosition;
                    SerializedProperty changesProperty = contourShapeProperty.FindPropertyRelative("changes");
                    changesProperty.enumValueFlag = changesProperty.enumValueFlag | (int)ContourShape.Changes.PositionMoved;
                }
            }
            else
            {
                EditorGUI.LabelField(rect, "NULL Point " + i);
                rect.y += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            }
        }
        EditorGUI.indentLevel--;     
        // Normal field
        SerializedProperty normalProperty = contourShapeProperty.FindPropertyRelative("normal");
        Vector3 editedNormal = EditorGUI.Vector3Field(rect, "Normal", normalProperty.vector3Value);
        if (editedNormal != normalProperty.vector3Value)
        {
            normalProperty.vector3Value = editedNormal;
            SerializedProperty changesProperty = contourShapeProperty.FindPropertyRelative("changes");
            changesProperty.enumValueFlag = changesProperty.enumValueFlag | (int)ContourShape.Changes.NormalChanged;
        }
    }
}
