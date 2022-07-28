using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ContourBlueprint))]
public class ContourBlueprintDrawer : PropertyDrawer
{
    private ContourShapeDrawer csd;

    public override float GetPropertyHeight(SerializedProperty bluePrintProperty, GUIContent label)
    {
        SerializedProperty shapeProperty = bluePrintProperty.FindPropertyRelative("shape");
        float shapePropertyHeight = csd != null ? csd.GetPropertyHeight(shapeProperty, null) : 0f;
        return shapePropertyHeight + (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
    }

    public override void OnGUI(Rect position, SerializedProperty bluePrintProperty, GUIContent label)
    {
        // Create rect for drawing each field
        Rect rect = position;
        rect.height = EditorGUIUtility.singleLineHeight;
        // Shape field
        SerializedProperty shapeProperty = bluePrintProperty.FindPropertyRelative("shape");
        csd = new ContourShapeDrawer();
        csd.OnGUI(rect, shapeProperty, GUIContent.none);
        rect.y += csd.GetPropertyHeight(shapeProperty, null);
        // Material field
        SerializedProperty contourMaterialProperty = bluePrintProperty.FindPropertyRelative("material");
        Object editedContourMaterial = EditorGUI.ObjectField(rect, contourMaterialProperty.objectReferenceValue, typeof(ContourMaterial), false);
        if (editedContourMaterial != contourMaterialProperty.objectReferenceValue)
        {
            contourMaterialProperty.objectReferenceValue = editedContourMaterial;
            SerializedProperty changesProperty = bluePrintProperty.FindPropertyRelative("materialHasChanged");
            changesProperty.boolValue = true;
        }
    }
}