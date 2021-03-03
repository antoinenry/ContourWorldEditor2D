using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(ContourBlocBuilder))]
public class ContourBlocBuilderInspector : Editor
{
    private ContourBlocBuilder targetBuilder;
    private bool showMaterials;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        targetBuilder = target as ContourBlocBuilder;
        targetBuilder.Build();
        // Material list inspector
        showMaterials = EditorGUILayout.Foldout(showMaterials, "Materials");
        if (showMaterials)
        {
            int tagCount = targetBuilder.UpdateMaterialListSize();
            List<string> tags = targetBuilder.bloc != null ? targetBuilder.bloc.contourTagNames : new List<string>();
            if (tagCount != tags.Count)
                EditorGUILayout.HelpBox("Tag count mismatch", MessageType.Error);
            else
            {
                EditorGUILayout.BeginVertical("box");
                for (int i = 0; i < tagCount; i++)
                {
                    // Edit list size
                    List<ContourMaterial> cms = targetBuilder.contourMaterialBundles[i].contourMaterials;
                    int materialCount = cms != null ? cms.Count : 0;
                    EditorGUI.BeginChangeCheck();
                    materialCount = EditorGUILayout.IntField(tags[i], materialCount);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (materialCount < 0) materialCount = 0;
                        if (cms != null)
                        {
                            ContourMaterial[] editMaterials = cms.ToArray();
                            Array.Resize(ref editMaterials, materialCount);
                            ContourBlocBuilder.ContourMaterialBundle materialBundle = targetBuilder.contourMaterialBundles[i];
                            materialBundle.contourMaterials = new List<ContourMaterial>(editMaterials);
                            targetBuilder.contourMaterialBundles[i] = materialBundle;
                        }
                        else
                            targetBuilder.contourMaterialBundles[i] = new ContourBlocBuilder.ContourMaterialBundle() { contourMaterials = new List<ContourMaterial>(new ContourMaterial[materialCount]) };
                    }
                    // List detail
                    List<ContourMaterial> contourMaterials = targetBuilder.contourMaterialBundles[i].contourMaterials;
                    EditorGUI.indentLevel++;
                    for (int j = 0, jend = contourMaterials != null ? contourMaterials.Count : 0; j < jend; j++)
                    {
                        ContourMaterial cm = contourMaterials[j];
                        EditorGUI.BeginChangeCheck();
                        cm = EditorGUILayout.ObjectField("Material " + j, cm, typeof(ContourMaterial), false) as ContourMaterial;
                        if (EditorGUI.EndChangeCheck()) contourMaterials[j] = cm;
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();
            }        
        }
        // Build button
        if (GUILayout.Button("Build")) targetBuilder.Build();
    }
}
