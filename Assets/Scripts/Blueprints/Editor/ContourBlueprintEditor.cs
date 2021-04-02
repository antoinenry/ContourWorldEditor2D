using UnityEditor;
using System;
using System.Reflection;

[CustomEditor(typeof(ContourBlueprint), true)]
public class ContourBlueprintEditor : Editor
{
    public string label;
    public bool hidePositions;

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
                if (hidePositions && fieldInfo.Name == "positions") continue;
                if (fieldInfo.Name == "changes" || fieldInfo.Name == "changedParameters") continue;
                SerializedProperty fieldProperty = serializedObject.FindProperty(fieldInfo.Name);
                LinkMaterialAndBlueprintAttribute linkAttribute = fieldInfo.GetCustomAttribute(typeof(LinkMaterialAndBlueprintAttribute)) as LinkMaterialAndBlueprintAttribute; ;
                if (linkAttribute != null)
                {
                    object valueInMaterial = contourMaterialType.GetField(linkAttribute.valueField).GetValue(contourMaterial);
                    ContourMaterial.BlueprintMode linkMode = (ContourMaterial.BlueprintMode)contourMaterialType.GetField(linkAttribute.modeField).GetValue(contourMaterial);
                    switch(linkMode)
                    {
                        case ContourMaterial.BlueprintMode.UseMaterialValue:
                            break;
                        case ContourMaterial.BlueprintMode.UseBlueprintValue:
                        case ContourMaterial.BlueprintMode.UseBoth:
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(fieldProperty);
                            if (EditorGUI.EndChangeCheck())
                            {
                                ContourBlueprint targetBlueprint = serializedObject.targetObject as ContourBlueprint;
                                targetBlueprint.OnChangeParameter(fieldProperty.name);
                            }
                            break;
                    }
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