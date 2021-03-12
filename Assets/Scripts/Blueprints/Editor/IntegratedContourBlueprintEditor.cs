﻿using UnityEditor;
using System;
using System.Reflection;

//[CustomEditor(typeof(ContourBlueprint), true)]
public class IntegratedContourBlueprintEditor : Editor
{
    public string label;

    public override void OnInspectorGUI()
    {
        Type blueprintType = target.GetType();
        EditorGUILayout.LabelField(label, blueprintType.Name);
        FieldInfo[] fieldInfos = blueprintType.GetFields();
        FieldInfo materialFieldInfo = blueprintType.GetField("material");
        ContourMaterial contourMaterial = materialFieldInfo.GetValue(target) as ContourMaterial;
        // Inspector showing only needed field (based on material)
        if (fieldInfos != null && contourMaterial != null)
        {
            Type contourMaterialType = contourMaterial.GetType();
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                SerializedProperty fieldProperty = serializedObject.FindProperty(fieldInfo.Name);
                LinkMaterialAndBlueprintAttribute linkAttribute = fieldInfo.GetCustomAttribute(typeof(LinkMaterialAndBlueprintAttribute)) as LinkMaterialAndBlueprintAttribute; ;
                if (linkAttribute != null)
                {
                    object valueInMaterial = contourMaterialType.GetField(linkAttribute.valueField).GetValue(contourMaterial);
                    ContourMaterial.BlueprintMode linkMode = (ContourMaterial.BlueprintMode)contourMaterialType.GetField(linkAttribute.modeField).GetValue(contourMaterial);

                    if (linkMode != ContourMaterial.BlueprintMode.UseMaterialValue)
                        EditorGUILayout.PropertyField(fieldProperty);
                }
                else
                {
                    EditorGUILayout.PropertyField(fieldProperty);
                }
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}