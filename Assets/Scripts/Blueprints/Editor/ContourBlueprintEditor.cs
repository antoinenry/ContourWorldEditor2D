//using UnityEditor;
//using UnityEngine;
//using System;
//using System.Reflection;

//[CustomEditor(typeof(ContourBlueprint), true)]
//public class ContourBlueprintEditor : Editor
//{
//    public FieldDisplay positionsDisplay;
//    public FieldDisplay materialDisplay;

//    public enum FieldDisplay { Default, ReadOnly, Hidden }

//    public override void OnInspectorGUI()
//    {
//        Type blueprintType = target.GetType();
//        FieldInfo[] fieldInfos = blueprintType.GetFields();
//        FieldInfo materialFieldInfo = blueprintType.GetField("material");
//        ContourMaterial contourMaterial = materialFieldInfo.GetValue(target) as ContourMaterial;
//        // Inspector showing only needed field (based on material)
//        if (fieldInfos != null && contourMaterial != null)
//        {
//            Type contourMaterialType = contourMaterial.GetType();
//            foreach (FieldInfo fieldInfo in fieldInfos)
//            {
//                string fieldName = fieldInfo.Name;
//                switch (fieldName)
//                {
//                    case "changes":
//                    case "changedParameters":
//                        continue;
//                    case "positions":
//                        if (positionsDisplay == FieldDisplay.Hidden) continue;
//                        if (positionsDisplay == FieldDisplay.ReadOnly) GUI.enabled = false;
//                        break;
//                    case "material":
//                        if (materialDisplay == FieldDisplay.Hidden) continue;
//                        if (materialDisplay == FieldDisplay.ReadOnly) GUI.enabled = false;
//                        break;
//                }
//                SerializedProperty fieldProperty = serializedObject.FindProperty(fieldName);
//                LinkMaterialAndBlueprintAttribute linkAttribute = fieldInfo.GetCustomAttribute(typeof(LinkMaterialAndBlueprintAttribute)) as LinkMaterialAndBlueprintAttribute; ;
//                if (linkAttribute != null)
//                {
//                    ContourMaterial.BlueprintMode linkMode = (ContourMaterial.BlueprintMode)contourMaterialType.GetField(linkAttribute.modeField).GetValue(contourMaterial);
//                    switch(linkMode)
//                    {
//                        case ContourMaterial.BlueprintMode.UseMaterialValue:
//                            break;
//                        case ContourMaterial.BlueprintMode.UseBlueprintValue:
//                        case ContourMaterial.BlueprintMode.UseBoth:
//                            EditorGUI.BeginChangeCheck();
//                            EditorGUILayout.PropertyField(fieldProperty);
//                            if (EditorGUI.EndChangeCheck())
//                            {
//                                serializedObject.FindProperty("blueprintChanges").enumValueIndex |= (int)ContourBlueprint.BlueprintChange.ParameterChanged;
//                                string[] changedParameters = serializedObject.FindProperty("changedParameters").stringValue.Split(' ');
//                                if (Array.IndexOf(changedParameters, fieldProperty.name) == -1)
//                                    serializedObject.FindProperty("changedParameters").stringValue += " " + fieldProperty.name;
//                            }
//                            break;
//                    }
//                }
//                else
//                {
//                    EditorGUILayout.PropertyField(fieldProperty);
//                }
//                GUI.enabled = true;
//            }
//        }
//        serializedObject.ApplyModifiedProperties();
//    }
//}