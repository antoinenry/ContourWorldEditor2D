using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(ContourBlocBuilder))]
public class ContourBlocBuilderInspector : Editor
{
    private static ContourBlocBuilder targetBuilder;
    private static bool[] contourFoldouts;
    //private bool showMaterials;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        targetBuilder = target as ContourBlocBuilder;
        //targetBuilder.Build();
        // Material list inspector
        //showMaterials = EditorGUILayout.Foldout(showMaterials, "Materials");
        //if (showMaterials)
        //{
        //    int tagCount = targetBuilder.UpdateMaterialListSize();
        //    List<string> tags = targetBuilder.bloc != null ? targetBuilder.bloc.contourTagNames : new List<string>();
        //    if (tagCount != tags.Count)
        //        EditorGUILayout.HelpBox("Tag count mismatch", MessageType.Error);
        //    else
        //    {
        //        EditorGUILayout.BeginVertical("box");
        //        for (int i = 0; i < tagCount; i++)
        //        {
        //            // Edit list size
        //            List<ContourMaterial> cms = targetBuilder.contourMaterialBundles[i].contourMaterials;
        //            int materialCount = cms != null ? cms.Count : 0;
        //            EditorGUI.BeginChangeCheck();
        //            materialCount = EditorGUILayout.IntField(tags[i], materialCount);
        //            if (EditorGUI.EndChangeCheck())
        //            {
        //                if (materialCount < 0) materialCount = 0;
        //                if (cms != null)
        //                {
        //                    ContourMaterial[] editMaterials = cms.ToArray();
        //                    Array.Resize(ref editMaterials, materialCount);
        //                    ContourBlocBuilder.ContourMaterialBundle materialBundle = targetBuilder.contourMaterialBundles[i];
        //                    materialBundle.contourMaterials = new List<ContourMaterial>(editMaterials);
        //                    targetBuilder.contourMaterialBundles[i] = materialBundle;
        //                }
        //                else
        //                    targetBuilder.contourMaterialBundles[i] = new ContourBlocBuilder.ContourMaterialBundle() { contourMaterials = new List<ContourMaterial>(new ContourMaterial[materialCount]) };
        //            }
        //            // List detail
        //            List<ContourMaterial> contourMaterials = targetBuilder.contourMaterialBundles[i].contourMaterials;
        //            EditorGUI.indentLevel++;
        //            for (int j = 0, jend = contourMaterials != null ? contourMaterials.Count : 0; j < jend; j++)
        //            {
        //                ContourMaterial cm = contourMaterials[j];
        //                EditorGUI.BeginChangeCheck();
        //                cm = EditorGUILayout.ObjectField("Material " + j, cm, typeof(ContourMaterial), false) as ContourMaterial;
        //                if (EditorGUI.EndChangeCheck()) contourMaterials[j] = cm;
        //            }
        //            EditorGUI.indentLevel--;
        //        }
        //        EditorGUILayout.EndVertical();
        //    }        
        //}
        // Contour inspector
        ContourListInspectorGUI();
        // Build button
        if (GUILayout.Button("Build")) targetBuilder.Build();
    }

    private void ContourListInspectorGUI()
    {
        // Adjust to contour count
        int ctCount = targetBuilder.ContourCount;
        if (contourFoldouts == null) contourFoldouts = new bool[ctCount];
        else Array.Resize(ref contourFoldouts, ctCount);
        // Display each contour with expandable inspector
        string[] paletteOptions = targetBuilder.GetPaletteOptionNames();
        for (int cti = 0; cti < ctCount; cti++)
        {
            // Minimal inspector: palette option name
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            bool expand = EditorGUILayout.Foldout(contourFoldouts[cti], "Contour " + cti);
            if (EditorGUI.EndChangeCheck())
            {
                contourFoldouts[cti] = expand;
                SceneView.RepaintAll();
                return;
            }
            EditorGUI.BeginChangeCheck();
            int paletteIndex = EditorGUILayout.Popup(targetBuilder.GetPaletteIndex(cti), paletteOptions);
            if (EditorGUI.EndChangeCheck()) targetBuilder.SetPaletteIndex(cti, paletteIndex);
            EditorGUILayout.EndHorizontal();
            // Expanded inspector: positions
            if (expand)
            {
                EditorGUI.BeginChangeCheck();
                Vector2[] positions = PositionArrayInspector("Positions", targetBuilder.GetPositions(cti));
                if (EditorGUI.EndChangeCheck())
                {
                    targetBuilder.SetPositions(cti, positions);
                    return;
                }
            }
        }
    }

    private Vector2[] PositionArrayInspector(string label, Vector2[] positions)
    {
        // Edit array size
        int posCount = positions != null ? positions.Length : 0;
        EditorGUI.BeginChangeCheck();
        int newPosCount = EditorGUILayout.IntField(label, posCount);
        Vector2[] newPositions = new Vector2[newPosCount];
        if (positions != null) Array.Copy(positions, newPositions, Math.Min(posCount, newPosCount));
        if (EditorGUI.EndChangeCheck()) return newPositions;
        // Edit array values
        EditorGUI.indentLevel++;
        EditorGUILayout.BeginVertical("box");
        for (int pi = 0; pi < newPosCount; pi++)
        {
            EditorGUI.BeginChangeCheck();
            Vector2 newPosition = EditorGUILayout.Vector2Field(pi.ToString(), newPositions[pi]);
            if (EditorGUI.EndChangeCheck())
            {
                newPositions[pi] = newPosition;
                break;
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUI.indentLevel--;
        return newPositions;
    }

    private void OnSceneGUI()
    {
        if (targetBuilder == null) return;
        // Highlight expanded contours
        Handles.color = Color.blue;
        if (contourFoldouts != null)
        {
            for (int cti = 0, ctCount = contourFoldouts.Length; cti < ctCount; cti++)
            {
                if (contourFoldouts[cti])
                {
                    Vector2[] positions = targetBuilder.GetPositions(cti);
                    for (int pi = 0, pCount = positions != null ? positions.Length : 0; pi < pCount - 1; pi++)
                        Handles.DrawLine(positions[pi], positions[pi + 1]);
                }
            }
        }
    }
}
