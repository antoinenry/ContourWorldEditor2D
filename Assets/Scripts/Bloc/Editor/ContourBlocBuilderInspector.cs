using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(ContourBlocBuilder))]
public class ContourBlocBuilderInspector : Editor
{
    private bool showDebug;
    private static ContourBlocBuilder targetBuilder;
    private static ContourInspectorState[] contourInspectors;
    private static Editor[] blueprintEditors;

    [Flags]
    private enum ContourInspectorState { None = 0, Expand = 1, ShowPositions = 2, Customize = 4 }

    public override void OnInspectorGUI()
    {
        showDebug = EditorGUILayout.Toggle("Show debug", showDebug);
        if (showDebug) base.OnInspectorGUI();
        targetBuilder = target as ContourBlocBuilder;
        if (GUILayout.Button("Build")) targetBuilder.Build();
        ContourListInspectorGUI();
        BlueprintListInspectorGUI();
    }

    private void BlueprintListInspectorGUI()
    {
        List<ContourBlueprint> blueprints = targetBuilder.Blueprints;
        if (blueprints == null) return;
        int bpCount = blueprints.Count;
        if (blueprintEditors == null) blueprintEditors = new Editor[bpCount];
        else if (blueprintEditors.Length != bpCount) Array.Resize(ref blueprintEditors, bpCount);
        //SerializedProperty blueprintArrayProperty = serializedObject.FindProperty("blueprints");
        for (int bpi = 0; bpi < bpCount; bpi++)
        {
            EditorGUILayout.BeginVertical("box");
            //SerializedProperty blueprintProperty = blueprintArrayProperty.GetArrayElementAtIndex(bpi);
            //if (blueprintProperty == null) EditorGUILayout.HelpBox("Missing blueprint property", MessageType.Error);
            //else EditorGUILayout.PropertyField(blueprintProperty);
            ContourBlueprint bp = blueprints[bpi];
            if (bp == null) EditorGUILayout.HelpBox("Null blueprint", MessageType.Error);
            CreateCachedEditor(bp, null, ref blueprintEditors[bpi]);
            blueprintEditors[bpi].OnInspectorGUI();
            EditorGUILayout.EndVertical();
        }
    }

    private void ContourListInspectorGUI()
    {
        // Adjust to contour count
        int ctCount = targetBuilder.ContourCount;
        if (contourInspectors == null) contourInspectors = new ContourInspectorState[ctCount];
        else Array.Resize(ref contourInspectors, ctCount);
        // Display each contour with expandable inspector
        string[] paletteOptions = targetBuilder.GetPaletteOptionNames();
        for (int cti = 0; cti < ctCount; cti++)
        {
            // Minimal inspector: palette option name
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            bool expand = EditorGUILayout.Foldout(contourInspectors[cti].HasFlag(ContourInspectorState.Expand), "Contour " + cti);
            if (EditorGUI.EndChangeCheck())
            {
                if (expand) contourInspectors[cti] |= ContourInspectorState.Expand;
                else contourInspectors[cti] &= ~ContourInspectorState.Expand;
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
                EditorGUI.indentLevel++;
                // Show contour length and foldout to show all positions
                Vector2[] positions = targetBuilder.GetPositions(cti);
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                bool showPositions = EditorGUILayout.Foldout(contourInspectors[cti].HasFlag(ContourInspectorState.ShowPositions), "Positions");
                if (EditorGUI.EndChangeCheck())
                {
                    if (showPositions) contourInspectors[cti] |= ContourInspectorState.ShowPositions;
                    else contourInspectors[cti] &= ~ContourInspectorState.ShowPositions;
                }
                int posCount = positions != null ? positions.Length : 0;
                EditorGUI.BeginChangeCheck();
                int newPosCount = EditorGUILayout.IntField(posCount);
                Vector2[] newPositions = new Vector2[newPosCount];
                if (positions != null) Array.Copy(positions, newPositions, Math.Min(posCount, newPosCount));
                if (EditorGUI.EndChangeCheck()) targetBuilder.SetPositions(cti, newPositions);
                EditorGUILayout.EndHorizontal();
                // Show all positions
                if (showPositions)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical("box");
                    for (int pi = 0; pi < newPosCount; pi++)
                    {
                        EditorGUI.BeginChangeCheck();
                        Vector2 newPosition = EditorGUILayout.Vector2Field(pi.ToString(), newPositions[pi]);
                        if (EditorGUI.EndChangeCheck())
                        {
                            newPositions[pi] = newPosition;
                            targetBuilder.SetPositions(cti, newPositions);
                            break;
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
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
            for (int cti = 0, ctCount = contourInspectors.Length; cti < ctCount; cti++)
            {
                if (contourInspectors[cti].HasFlag(ContourInspectorState.Expand))
                {
                    Vector2[] positions = targetBuilder.GetPositions(cti);
                    for (int pi = 0, pCount = positions != null ? positions.Length : 0; pi < pCount - 1; pi++)
                        Handles.DrawLine(positions[pi], positions[pi + 1]);
                }
            }
        }
    }
}
