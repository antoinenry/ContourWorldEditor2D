using UnityEngine;
using UnityEditor;
using System;

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
        //SetBlueprintsVisible(showBluePrintComponents);
    }

    public override void OnInspectorGUI()
    {
        // When bloc is defined, contour are set by the ContourBloc component
        if (targetBuilder.bloc != null) GUI.enabled = false;
        DrawDefaultInspector();
        GUI.enabled = true;
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
        // Edit blueprints
        showBlueprintList = EditorGUILayout.Foldout(showBlueprintList, "Blueprints");
        if (showBlueprintList)
        {
            EditorGUI.indentLevel++;
            // Show blue prints as a list, integrated in this inspector
            ContourListInspectorGUI();
            // Option to show all blueprints as individual components
            //EditorGUI.BeginChangeCheck();
            //showBluePrintComponents = EditorGUILayout.Toggle("Show components", showBluePrintComponents);
            //if (EditorGUI.EndChangeCheck()) SetBlueprintsVisible(showBluePrintComponents);
            EditorGUI.indentLevel--;
        }
        else
            SetBlueprintsVisible(false);
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

    private void ContourListInspectorGUI()
    {
        // Adjust inspector capacity to contour count
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
            if (targetBuilder.palette != null)
            {
                EditorGUI.BeginChangeCheck();
                int paletteIndex = EditorGUILayout.Popup(targetBuilder.GetPaletteIndex(cti), paletteOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(targetBuilder, "Set contour material");
                    targetBuilder.SetPaletteIndex(cti, paletteIndex);
                    // Apply modifications in inspector
                    EditorUtility.SetDirty(targetBuilder);
                }
            }
            EditorGUILayout.EndHorizontal();
            // Expanded inspector: contour blueprints      
            ContourBlueprint[] blueprints = targetBuilder.GetContourBlueprints(cti);
            foreach (ContourBlueprint bp in blueprints)
                bp.hideFlags = expand ? HideFlags.None : HideFlags.HideInInspector;
            //if (expand)
            //{
            //    // Adjust inspector to blueprint count
            //    ContourBlueprint[] blueprints = targetBuilder.GetContourBlueprints(cti);
            //    int bpCount = blueprints.Length;
            //    if (contourInspectors[cti].blueprintEditors == null) contourInspectors[cti].blueprintEditors = new Editor[bpCount];
            //    else Array.Resize(ref contourInspectors[cti].blueprintEditors, bpCount);
            //    // Show editor for each blueprint of this contour
            //    EditorGUI.indentLevel++;
            //    if (bpCount > 0)
            //    {
            //        for (int bpi = 0; bpi < bpCount; bpi++)
            //        {
            //            EditorGUILayout.BeginVertical("box");
            //            ContourBlueprint bp = blueprints[bpi];
            //            if (bp == null)
            //                EditorGUILayout.HelpBox("Null blueprint", MessageType.Error);
            //            else
            //            {
            //                EditorGUILayout.LabelField(bpi.ToString(), bp.GetType().Name);
            //                Editor[] bpEditors = contourInspectors[cti].blueprintEditors;
            //                CreateCachedEditor(bp, typeof(ContourBlueprintEditor), ref bpEditors[bpi]);
            //                ContourBlueprintEditor bpEditor = (bpEditors[bpi] as ContourBlueprintEditor);
            //                bpEditor.positionsDisplay = ContourBlueprintEditor.FieldDisplay.Hidden;
            //                bpEditor.materialDisplay = ContourBlueprintEditor.FieldDisplay.ReadOnly;
            //                bpEditor.OnInspectorGUI();
            //            }
            //            EditorGUILayout.EndVertical();
            //        }
            //    }
            //    else
            //        EditorGUILayout.HelpBox("This contour doesn't generate blueprints", MessageType.Info);
            //    EditorGUI.indentLevel--;
            //}
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
