﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(ContourBlocBuilder))]
public class ContourBlocBuilderInspector : Editor
{
    private static bool showBlueprintList;
    private static bool showBluePrintComponents;
    private static ContourBlocBuilder targetBuilder;
    private static ContourInspector[] contourInspectors;
    //private static Editor[] blueprintEditors;

    private struct ContourInspector
    {
        public State inspectorState;
        public Editor[] blueprintEditors;

        [Flags]
        public enum State { None = 0, Expand = 1, ShowPositions = 2, Customize = 4 }
    }

    private void OnEnable()
    {
        targetBuilder = target as ContourBlocBuilder;
        SetBlueprintsVisible(showBluePrintComponents);
    }

    public override void OnInspectorGUI()
    {
        // When bloc is defined, contour are set by the ContourBloc component
        if (targetBuilder.bloc != null) GUI.enabled = false;
        DrawDefaultInspector();
        GUI.enabled = true;
        // Set bloc and palette
        targetBuilder.bloc = EditorGUILayout.ObjectField("Bloc", targetBuilder.bloc, typeof(ContourBloc), true) as ContourBloc;
        targetBuilder.palette = EditorGUILayout.ObjectField("Palette", targetBuilder.palette, typeof(ContourPalette), true) as ContourPalette;
        // Edit blueprints
        showBlueprintList = EditorGUILayout.Foldout(showBlueprintList, "Blueprints");
        if (showBlueprintList)
        {
            EditorGUI.indentLevel++;
            // Show blue prints as a list, integrated in this inspector
            BlueprintListInspectorGUI();
            // Option to show all blueprints as individual components
            EditorGUI.BeginChangeCheck();
            showBluePrintComponents = EditorGUILayout.Toggle("Show components", showBluePrintComponents);
            if (EditorGUI.EndChangeCheck()) SetBlueprintsVisible(showBluePrintComponents);
            EditorGUI.indentLevel--;
        }
    }

    private void SetBlueprintsVisible(bool visible)
    {
        ContourBlueprint[] blueprints = targetBuilder.GetComponents<ContourBlueprint>();
        foreach (ContourBlueprint bp in blueprints)
        {
            bp.hideFlags = visible ? HideFlags.None : HideFlags.HideInInspector;
            EditorUtility.SetDirty(bp);
        }
        
    }

    private void BlueprintListInspectorGUI()
    {
        // Adjust to contour count
        int ctCount = targetBuilder.ContourCount;
        if (contourInspectors == null) contourInspectors = new ContourInspector[ctCount];
        else Array.Resize(ref contourInspectors, ctCount);
        // Display each contour with expandable inspector
        string[] paletteOptions = targetBuilder.GetPaletteOptionNames();
        for (int cti = 0; cti < ctCount; cti++)
        {
            // Minimal inspector: palette option name
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            bool expand = EditorGUILayout.Foldout(contourInspectors[cti].inspectorState.HasFlag(ContourInspector.State.Expand), "Contour " + cti);
            if (EditorGUI.EndChangeCheck())
            {
                if (expand) contourInspectors[cti].inspectorState |= ContourInspector.State.Expand;
                else contourInspectors[cti].inspectorState &= ~ContourInspector.State.Expand;
                SceneView.RepaintAll();
                // Apply modifications in inspector
                EditorUtility.SetDirty(targetBuilder);
                return;
            }
            EditorGUI.BeginChangeCheck();
            int paletteIndex = EditorGUILayout.Popup(targetBuilder.GetPaletteIndex(cti), paletteOptions);
            if (EditorGUI.EndChangeCheck())
            {
                targetBuilder.SetPaletteIndex(cti, paletteIndex);
                // Apply modifications in inspector
                EditorUtility.SetDirty(targetBuilder);
            }
            EditorGUILayout.EndHorizontal();
            // Expanded inspector: contour blueprints
            if (expand)
            {
                // Adjust inspector to blueprint count
                ContourBlueprint[] blueprints = targetBuilder.GetContourBlueprints(cti);
                int bpCount = blueprints.Length;
                if (contourInspectors[cti].blueprintEditors == null) contourInspectors[cti].blueprintEditors = new Editor[bpCount];
                else Array.Resize(ref contourInspectors[cti].blueprintEditors, bpCount);
                // Show editor for each blueprint of this contour
                EditorGUI.indentLevel++;
                for(int bpi = 0; bpi < bpCount; bpi++)
                {
                    EditorGUILayout.BeginVertical("box");
                    ContourBlueprint bp = blueprints[bpi];
                    if (bp == null)
                        EditorGUILayout.HelpBox("Null blueprint", MessageType.Error);
                    else
                    {
                        Editor[] bpEditors = contourInspectors[cti].blueprintEditors;
                        CreateCachedEditor(bp, typeof(ContourBlueprintEditor), ref bpEditors[bpi]);
                        ContourBlueprintEditor bpEditor = (bpEditors[bpi] as ContourBlueprintEditor);
                        bpEditor.label = bpi.ToString();
                        bpEditor.hidePositions = true;
                        EditorGUI.BeginChangeCheck();
                        bpEditor.OnInspectorGUI();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }
        }
    }

    private void OnSceneGUI()
    {
        if (targetBuilder == null) return;
        // Highlight expanded contours
        Handles.color = Color.blue;
        if (contourInspectors != null)
        {
            Vector2 builderPosition = targetBuilder.transform.position;
            for (int cti = 0, ctCount = contourInspectors.Length; cti < ctCount; cti++)
            {
                if (contourInspectors[cti].inspectorState.HasFlag(ContourInspector.State.Expand))
                {
                    Vector2[] positions = targetBuilder.GetPositions(cti);
                    for (int pi = 0, pCount = positions != null ? positions.Length : 0; pi < pCount - 1; pi++)
                        Handles.DrawLine(builderPosition + positions[pi], builderPosition + positions[pi + 1]);
                }
            }
        }
    }
}