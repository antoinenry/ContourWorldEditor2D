//using UnityEngine;
//using UnityEditor;

//[CustomPropertyDrawer(typeof(LinkedToBlueprintAttribute))]
//public class LinkedToBlueprintDrawer : PropertyDrawer
//{
//    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    {
//        return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
//    }

//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        position.height = base.GetPropertyHeight(property, label);
//        // Property field
//        EditorGUI.PropertyField(position, property);
//        // Attribute mode field
//        position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
//        LinkedToBlueprintAttribute linkAttribute = attribute as LinkedToBlueprintAttribute;
//        SerializedProperty modeProperty = property.serializedObject.FindProperty(linkAttribute.modeFieldName);
//        ContourMaterial.BlueprintMode modeEnum = (ContourMaterial.BlueprintMode)modeProperty.enumValueIndex;
//        modeEnum = (ContourMaterial.BlueprintMode)EditorGUI.EnumPopup(position, property.displayName + " mode", modeEnum);
//        modeProperty.enumValueIndex = (int)modeEnum;
//    }
//}
