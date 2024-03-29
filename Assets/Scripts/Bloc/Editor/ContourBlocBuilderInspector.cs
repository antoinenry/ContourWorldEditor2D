﻿using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(ContourBlocBuilder))]
public class ContourBlocBuilderInspector : Editor
{
    private static bool showContourList;
    private static ContourBlocBuilder targetBuilder;
    private static int selectedContourIndex;

    private void OnEnable()
    {
        targetBuilder = target as ContourBlocBuilder;
    }

    public override void OnInspectorGUI()
    {
        // When bloc is defined, contour are set by the ContourBloc component
        //if (targetBuilder.bloc == null) DrawDefaultInspector();
        // Set bloc and palette
        EditorGUI.BeginChangeCheck();
        ContourBloc bloc = EditorGUILayout.ObjectField("Bloc", targetBuilder.bloc, typeof(ContourBloc), true) as ContourBloc;
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Set bloc");
            targetBuilder.bloc = bloc;
            EditorUtility.SetDirty(targetBuilder);
        }
        EditorGUI.BeginChangeCheck();
        ContourPalette palette = targetBuilder.palette = EditorGUILayout.ObjectField("Palette", targetBuilder.palette, typeof(ContourPalette), true) as ContourPalette;
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Set palette");
            targetBuilder.palette = palette;
            EditorUtility.SetDirty(targetBuilder);
        }
        // Defaut material selector (when creating new contour)
        string[] paletteOptions = targetBuilder.GetPaletteOptionNames();
        targetBuilder.defaultPaletteIndex = EditorGUILayout.Popup("Default contour material", targetBuilder.defaultPaletteIndex, paletteOptions);
        // Contour list
        showContourList = EditorGUILayout.Foldout(showContourList, "Contours");
        if (showContourList)
        {
            EditorGUI.indentLevel++;
            ContourListInspectorGUI();
            EditorGUI.indentLevel--;
        }
        else
            selectedContourIndex = -1;
    }

    private void ContourListInspectorGUI()
    {
        // Adjust inspector capacity to contour count
        int ctCount = targetBuilder.ContourCount;
        // Display each contour with expandable inspector
        string[] paletteOptions = targetBuilder.GetPaletteOptionNames();
        for (int cti = 0; cti < ctCount; cti++)
        {
            // Minimal inspector: palette option name
            EditorGUILayout.BeginHorizontal();
            GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
            foldoutStyle.focused.textColor = Color.blue;
            bool contourIsSelected = cti == selectedContourIndex;
            EditorGUI.BeginChangeCheck();
            contourIsSelected = EditorGUILayout.Foldout(contourIsSelected, "Contour " + cti, foldoutStyle);
            if (EditorGUI.EndChangeCheck())
            {
                selectedContourIndex = contourIsSelected ? cti : -1;
                SceneView.RepaintAll();
                // Apply modifications in inspector
                EditorUtility.SetDirty(targetBuilder);
                return;
            }
            if (targetBuilder.palette != null)
            {
                //EditorGUI.BeginChangeCheck();
                int paletteIndex = targetBuilder.GetPaletteIndex(cti);
                paletteIndex = EditorGUILayout.Popup(paletteIndex, paletteOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(targetBuilder, "Set contour material");
                    targetBuilder.SetPaletteIndex(cti, paletteIndex);
                    // Apply modifications in inspector
                    EditorUtility.SetDirty(targetBuilder);
                }
            }
            EditorGUILayout.EndHorizontal();
            // Expanded contour inspector
            if (contourIsSelected)
            {
                EditorGUILayout.BeginVertical("box");
                GUI.enabled = false;
                ContourBlocBuilder.ContourBuildInfos contourInfos = targetBuilder.GetContourInfos(cti);
                EditorGUILayout.IntField("Length:", contourInfos.shape.Length);
                EditorGUILayout.IntField("Blueprints:", contourInfos.blueprints.Length);
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
            }
        }
    }

    private void OnSceneGUI()
    {
        // Highlight selected contour
        if (selectedContourIndex != -1)
        {
            Handles.color = Color.blue;
            Vector2[] contourPoints = targetBuilder.GetContourInfos(selectedContourIndex).GetPositions();
            contourPoints = Array.ConvertAll(contourPoints, pos => (Vector2)targetBuilder.transform.TransformPoint(pos));
            int contourLength = contourPoints.Length;
            for (int i = 0; i < contourLength - 1; i++)
                Handles.DrawLine(contourPoints[i], contourPoints[i+1]);
        }
    }
}
