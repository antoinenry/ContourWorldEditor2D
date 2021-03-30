using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(ContourBloc))]
public class ContourBlocInspector : Editor
{
    // Static members are required for sharing values between InspectorGUI and SceneGUI
    // and for continuity between different instances of the inspector
    private static ContourBloc targetBloc;
    private static Component[] targetBlocAndBuilder;
    private static bool sceneGUIEditMode;
    private static bool createContourMode;
    private static bool showDebug;
    private static bool showPointListInspector;
    private static bool[] pointSceneSelection;
    private static PointInspectorState[] pointInspectorStates;
    private static bool showContourListInspector;
    private static bool[] contourSceneSelection;
    private static ContourInspectorState[] contourInspectorStates;
    private static float gridSnap;

    // General parameters
    const float handleScale = .1f;

    private struct PointInspectorState
    {
        public bool selected;
    }

    private struct ContourInspectorState
    {
        public bool selected;
        public bool foldout;
        public float pointScroller;
    }

    private void OnEnable()
    {
        SetTarget();
        Undo.undoRedoPerformed += OnUndoRedo;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;
    }

    private void OnUndoRedo()
    {
        ClearContourSelection();
        ClearPointSelection();
        SetTarget();
        SetTargetDirty();
        targetBloc.changes = ~ContourBloc.BlocChanges.None;
    }

    private void SetTarget()
    {
        // When switching target
        if (target != targetBloc || targetBloc == null)
        {
            targetBloc = target as ContourBloc;
            ContourBlocBuilder attachedBuilder = targetBloc.GetComponent<ContourBlocBuilder>();
            targetBlocAndBuilder = attachedBuilder != null ? new Component[] { targetBloc, attachedBuilder } : new Component[] { targetBloc };
            createContourMode = false;
            CorrectPointSelectionArrayLengths();
            CorrectContourSelectionArrayLengths();
        }
    }

    private void SetTargetDirty()
    {
        foreach (Component c in targetBlocAndBuilder) EditorUtility.SetDirty(c);
    }

    // -------------------------------------------
    // SELECTION: to select points and contours, keeping everything synchronised between SceneGUI and InspectorGUI
    // -------------------------------------------
    #region Selection

    private bool IsPointSelected(int pointIndex)
    {
        CorrectPointSelectionArrayLengths();
        if (pointIndex < 0 || pointIndex > pointSceneSelection.Length) return false;
        return pointSceneSelection[pointIndex];
    }

    private bool IsContourSelected(int contourIndex)
    {
        CorrectContourSelectionArrayLengths();
        if (contourIndex < 0 || contourIndex > contourSceneSelection.Length) return false;
        return contourSceneSelection[contourIndex];
    }

    private void SelectPoint(int pointIndex, bool select)
    {
        if (pointIndex < 0 || pointIndex > targetBloc.PointCount) return;
        CorrectPointSelectionArrayLengths();
        // Select point in selection arrays
        pointSceneSelection[pointIndex] = select;
        pointInspectorStates[pointIndex].selected = select;
        // Update InspectorGUI and SceneGUI
        showPointListInspector = true;
        SetTargetDirty();
        SceneView.RepaintAll();
    }

    private void SelectContour(int contourIndex, bool select)
    {
        CorrectContourSelectionArrayLengths();
        if (contourIndex < 0 || contourIndex > contourSceneSelection.Length) return;
        // Select contour in selection arrays
        contourSceneSelection[contourIndex] = select;
        contourInspectorStates[contourIndex].selected = select;
        // Update InspectorGUI and SceneGUI
        showContourListInspector = true;
        SetTargetDirty();
        SceneView.RepaintAll();
    }

    private void SetInspectorStateFromSceneSelection()
    {
        // Signal selected points and contours in inspectorGUI
        if (pointSceneSelection == null) ClearPointSelection();
        else pointInspectorStates = Array.ConvertAll(pointSceneSelection, s => new PointInspectorState() { selected = s });
        if (contourSceneSelection == null) ClearContourSelection();
        else contourInspectorStates = Array.ConvertAll(contourSceneSelection, s => new ContourInspectorState() { selected = s });
    }

    private void SetSceneSelectionFromInspector()
    {
        // Keep current inspector state but modify selection so it matches the inspector
        if (pointInspectorStates == null) ClearPointSelection();
        else pointSceneSelection = Array.ConvertAll(pointInspectorStates, state => state.selected);
        if (contourInspectorStates == null) ClearContourSelection();
        else contourSceneSelection = Array.ConvertAll(contourInspectorStates, state => state.selected);
        // Update SceneGUI
        SceneView.RepaintAll();
    }

    private void ClearPointSelection()
    {
        CorrectPointSelectionArrayLengths();
        for (int pti = 0, ptCount = targetBloc.PointCount; pti < ptCount; pti++)
        {
            pointSceneSelection[pti] = false;
            pointInspectorStates[pti].selected = false;
        }
        // Update InspectorGUI and SceneGUI
        SetTargetDirty();
        SceneView.RepaintAll();
    }

    private void ClearContourSelection()
    {
        CorrectContourSelectionArrayLengths();
        for (int cti = 0, ctCount = targetBloc.GetContourCount(); cti < ctCount; cti++)
        {
            contourSceneSelection[cti] = false;
            contourInspectorStates[cti].selected = false;
        }
        // Update InspectorGUI and SceneGUI
        SetTargetDirty();
        SceneView.RepaintAll();
    }    

    private void SelectAllPointsInContour(int contourIndex)
    {
        List<int> pointsInContour = targetBloc.GetContour(contourIndex, false);
        if (pointsInContour == null) return;
        foreach (int pti in pointsInContour)
            SelectPoint(pti, true);
    }

    private void SelectAllContoursWithPoint(int pointIndex)
    {
        List<int> contourIndices = targetBloc.GetContoursWithPoint(pointIndex);
        if (contourIndices == null) return;
        foreach (int cti in contourIndices) SelectContour(cti, true);
    }

    private void UnselectPointsIfNoSelectedContour(List<int> pointIndices)
    {
        if (pointIndices == null) return;
        List<int> selectedContourIndices = GetSelectedContourIndices();
        foreach(int pti in pointIndices)
        {
            List<int> contoursWithPoint = targetBloc.GetContoursWithPoint(pti);
            if (contoursWithPoint == null || contoursWithPoint.FindIndex(cti => IsContourSelected(cti)) != -1)
                SelectPoint(pti, false);
        }
    }

    private void UnselectContoursIfNoSelectedPoints(List<int> contourIndices)
    {
        if (contourIndices == null) return;
        foreach(int cti in contourIndices)
        {
            bool selectContour = false;
            List<int> pointIndices = targetBloc.GetContour(cti, false);
            foreach(int pti in pointIndices)
            {
                if (pointSceneSelection[pti] == true)
                {
                    selectContour = true;
                    break;
                }
            }
            SelectContour(cti, selectContour);
        }
    }

    private void CorrectPointSelectionArrayLengths()
    {
        int pointCount = targetBloc.PointCount;
        if (pointSceneSelection == null) pointSceneSelection = new bool[pointCount];
        else if (pointCount != pointSceneSelection.Length) Array.Resize(ref pointSceneSelection, pointCount);
        if (pointInspectorStates == null) pointInspectorStates = new PointInspectorState[pointCount];
        else if (pointCount != pointInspectorStates.Length) Array.Resize(ref pointInspectorStates, pointCount);
    }

    private void CorrectContourSelectionArrayLengths()
    {
        int contourCount = targetBloc.GetContourCount();
        if (contourSceneSelection == null) contourSceneSelection = new bool[contourCount];
        else if (contourCount != contourSceneSelection.Length) Array.Resize(ref contourSceneSelection, contourCount);
        if (contourInspectorStates == null) contourInspectorStates = new ContourInspectorState[contourCount];
        else if (contourCount != contourInspectorStates.Length) Array.Resize(ref contourInspectorStates, contourCount);
    }

    private void SelectAll()
    {
        for (int i = 0, iend = targetBloc.PointCount; i < iend; i++) SelectPoint(i, true);
        for (int i = 0, iend = targetBloc.GetContourCount(); i < iend; i++) SelectContour(i, true);
    }

    private void UnselectAll()
    {
        ClearPointSelection();
        ClearContourSelection();
    }

    private List<int> GetSelectedPointsIndices()
    {
        List<int> selected = new List<int>(targetBloc.PointCount);
        for (int i = 0, iend = pointSceneSelection.Length; i < iend; i++) if (pointSceneSelection[i]) selected.Add(i);
        return selected;
    }

    private List<int> GetSelectedContourIndices()
    {
        List<int> selected = new List<int>(targetBloc.GetContourCount());
        for (int i = 0, iend = contourSceneSelection.Length; i < iend; i++) 
            if (contourSceneSelection[i]) 
                selected.Add(i);
        return selected;
    }

    private List<int> GetUnselectedContourIndices()
    {
        List<int> selected = new List<int>(targetBloc.GetContourCount());
        for (int i = 0, iend = contourSceneSelection.Length; i < iend; i++) if (!contourSceneSelection[i]) selected.Add(i);
        return selected;
    }
    #endregion

    // -------------------------------------------
    // INSPECTOR GUI: inspector fields to edit contours
    // -------------------------------------------
    #region InspectorGUI

    public override void OnInspectorGUI()
    {
        SetTarget();
        bool changeCheck = false;
        // Remove later
        showDebug = EditorGUILayout.Toggle("Show debug", showDebug);
        if (showDebug)
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
        }
        // Toggle scene edit mode: enables edit bloc with handles in SceneGUI
        if (sceneGUIEditMode != EditorGUILayout.Foldout(sceneGUIEditMode, "EDIT"))
        {
            sceneGUIEditMode = !sceneGUIEditMode;
            // Cancel selection and minimize inspector when closing edit mode
            if (!sceneGUIEditMode)
            {
                UnselectAll();
                showPointListInspector = false;
                showContourListInspector = false;
            }
            // Handles are rendered in scene view so we need to update it
            SceneView.RepaintAll();
        }
        // If scene edit mode is active, show toolbar in inspectorGUI
        if (sceneGUIEditMode)
        {
            if (createContourMode)
                CreateContourInspectorGUI(out changeCheck);// Special toolbar when creating a new contour
            else
                HandleInspectorGUI(out changeCheck); // Normal toolbar to handle points
        }
        if (changeCheck) return;
        // Inspect points in bloc
        PointListInspectorGUI(out changeCheck);
        if (changeCheck) return;
        // Inspect contours in bloc
        ContourListInspectorGUI(out changeCheck);
        if (changeCheck) return;
    }

    // Point list inspector (returns true if a change is made)
    private void PointListInspectorGUI(out bool changeCheck)
    {
        changeCheck = false;
        EditorGUILayout.BeginHorizontal();
        // Toggle list view
        showPointListInspector = EditorGUILayout.Foldout(showPointListInspector, "Points");
        // Add point button
        if (GUILayout.Button("Add"))
        {
            targetBloc.AddPoint(Vector2.zero);
            showPointListInspector = true;
            // Cancel current selection and select added point
            UnselectAll();
            SelectPoint(targetBloc.PointCount - 1, true);
            changeCheck = true;
        }
        EditorGUILayout.EndHorizontal();
        if (changeCheck) return;
        if (showPointListInspector)
        {
            // Show list content
            EditorGUILayout.BeginVertical("box");
            for (int i = 0, iend = targetBloc.PointCount; i < iend; i++)
            {
                PointInspectorGUI(i, out changeCheck);
                if (changeCheck) break;
            }
            EditorGUILayout.EndVertical();
        }
    }

    // Single point inspector (returns true if a change is made)
    private void PointInspectorGUI(int pointIndex, out bool changeCheck)
    {
        bool GUI_enabled = GUI.enabled;
        changeCheck = false;
        EditorGUILayout.BeginHorizontal();
        // In scene edit mode, show a button to select the point
        if (sceneGUIEditMode)
        {
            Color normalBackgroundColor = GUI.backgroundColor;
            CorrectPointSelectionArrayLengths();
            if (pointInspectorStates[pointIndex].selected) GUI.backgroundColor = createContourMode ? Color.green : Color.yellow;
            if (GUILayout.Button("Point " + pointIndex.ToString()))
            {
                // Keep current selection only if Ctrl is held
                if (!Event.current.modifiers.HasFlag(EventModifiers.Control))
                    UnselectAll();

                if (IsPointSelected(pointIndex))
                {
                    SelectPoint(pointIndex, false);
                    // Hold Alt to keep current contour selection
                    if (!Event.current.modifiers.HasFlag(EventModifiers.Alt))
                        UnselectContoursIfNoSelectedPoints(targetBloc.GetContoursWithPoint(pointIndex));
                }
                else
                {
                    SelectPoint(pointIndex, true);
                    // Hold Alt to keep current contour selection
                    if (!Event.current.modifiers.HasFlag(EventModifiers.Alt))
                        SelectAllContoursWithPoint(pointIndex);
                }                
            }
            GUI.backgroundColor = normalBackgroundColor;
        }
        // Otherwise, show a simple label
        else
            EditorGUILayout.LabelField("Point " + pointIndex.ToString());        
        // Field to edit point's position
        Vector2 getPosition = targetBloc.GetPosition(pointIndex);
        Vector2 editedPosition = EditorGUILayout.Vector2Field("", getPosition);
        if (editedPosition != getPosition)
        {
            // If positions has been changed, update bloc
            Undo.RecordObjects(targetBlocAndBuilder, "Edit Point In Bloc");
            targetBloc.MovePoint(pointIndex, editedPosition);
            SceneView.RepaintAll();
        }
        // Button to remove point
        if (sceneGUIEditMode && GUILayout.Button("Remove"))
        {
            Undo.RecordObjects(targetBlocAndBuilder, "Remove point");
            targetBloc.DestroyPoint(pointIndex);
            targetBloc.DestroyAllPointlessContours();
            UnselectAll();
            SceneView.RepaintAll();
            changeCheck = true;
            return;
        }
        GUI.enabled = GUI_enabled;
        EditorGUILayout.EndHorizontal();
    }

    // Contour list inspector (returns true if a change is made)
    private void ContourListInspectorGUI(out bool changeCheck)
    {
        int countourCount = targetBloc.GetContourCount();
        changeCheck = false;
        EditorGUILayout.BeginHorizontal();
        // Toggle list view
        showContourListInspector = EditorGUILayout.Foldout(showContourListInspector, "Contours");
        // Button to add a contour
        if (GUILayout.Button("Add"))
        {
            targetBloc.AddContour();
            UnselectAll();
            SelectContour(countourCount - 1, true);
            changeCheck = true;
        }
        EditorGUILayout.EndHorizontal();
        if (changeCheck) return;
        if (showContourListInspector)
        {
            // Show contour list
            EditorGUILayout.BeginVertical("box");
            for (int cti = 0; cti < countourCount; cti++)
            {
                ContourInspectorGUI(cti, out changeCheck);
                if (changeCheck) break;
            }
            EditorGUILayout.EndVertical();
        }
    }

    // Single contour inspector (returns true if a change is made)
    private void ContourInspectorGUI(int contourIndex, out bool changeCheck)
    {
        changeCheck = false;
        EditorGUILayout.BeginHorizontal();
        // In scene edit mode, show a button to select the contour and a foldout to show contour details
        if (sceneGUIEditMode)
        {
            Color normalBackgroundColor = GUI.backgroundColor;
            if (IsContourSelected(contourIndex)) GUI.backgroundColor = createContourMode ? Color.green : Color.yellow;
            GUIStyle foldOutStyle = new GUIStyle("foldout");
            foldOutStyle.fixedWidth = 1f;
            contourInspectorStates[contourIndex].foldout = EditorGUILayout.Foldout(contourInspectorStates[contourIndex].foldout, GUIContent.none, foldOutStyle);
            // Button to select contour
            if (GUILayout.Button("Contour " + contourIndex.ToString()))
            {
                if (!IsContourSelected(contourIndex))
                {
                    // Hold Ctrl to keep current contour selection (multi-select)
                    if (!Event.current.modifiers.HasFlag(EventModifiers.Control))
                        UnselectAll();
                    SelectContour(contourIndex, true);
                    // Also select point provided Alt is held
                    if (!Event.current.modifiers.HasFlag(EventModifiers.Alt))
                        SelectAllPointsInContour(contourIndex);
                }
                else
                {
                    // If contour is already selected, hold Ctrl to multi-unselect it (you lose the rest of your selection otherwise)
                    if (Event.current.modifiers.HasFlag(EventModifiers.Control))
                    {
                        // Hold Alt to avoid affecting point selection
                        if (!Event.current.modifiers.HasFlag(EventModifiers.Alt))
                            UnselectPointsIfNoSelectedContour(targetBloc.GetContour(contourIndex, false));
                        // Unselect contour
                        SelectContour(contourIndex, false);
                    }
                    // Without Ctrl, current selection is cancelled
                    else
                    {
                        UnselectAll();
                        SelectContour(contourIndex, true);
                        // Also select point provided Alt is held
                        if (!Event.current.modifiers.HasFlag(EventModifiers.Alt))
                            SelectAllPointsInContour(contourIndex);
                    }
                }
                
            }
            GUI.backgroundColor = normalBackgroundColor;
        }
        // Otherwise, just show the foldout control
        else
        {
            CorrectContourSelectionArrayLengths();
            contourInspectorStates[contourIndex].foldout = EditorGUILayout.Foldout(contourInspectorStates[contourIndex].foldout, "Contour " + contourIndex.ToString());
        }
        // Get contour and points that are in it
        List<int> contourPoints = targetBloc.GetContour(contourIndex, true);
        // Button to remove contour
        if (sceneGUIEditMode && GUILayout.Button("Remove"))
        {
            Undo.RecordObjects(targetBlocAndBuilder, "Remove contour");
            targetBloc.RemoveContourAt(contourIndex);
            targetBloc.DestroyAllContourlessPoints();
            UnselectAll();
            SceneView.RepaintAll();
            changeCheck = true;
            return;
        }
        EditorGUILayout.EndHorizontal();
        if (changeCheck) return;
        // Expanded contour inspector
        if (contourInspectorStates[contourIndex].foldout == true)
        {
            // Toggle contour loop
            bool loop = targetBloc.IsContourLooped(contourIndex);
            if (EditorGUILayout.Toggle("Loop", loop) != loop)
            {
                // If loop field is edited, apply changes
                Undo.RecordObjects(targetBlocAndBuilder, "Set loop");
                targetBloc.LoopContour(contourIndex, !loop);
                SceneView.RepaintAll();
                changeCheck = true;
                return;
            }
            // Field to monitor and manually set contour length
            int contourLength = contourPoints.Count;
            int editContourLength = EditorGUILayout.IntField("Length", contourLength);
            if (editContourLength < 1) editContourLength = 1;
            if (editContourLength != contourLength)
            {
                SelectContour(contourIndex, true);
                Undo.RecordObjects(targetBlocAndBuilder, "Set Contour Length");
                targetBloc.SetContourLength(contourIndex, editContourLength);
                SceneView.RepaintAll();
                changeCheck = true;
                return;
            }
            // To edit each point in contour
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Positions indices:");
            if (GUILayout.Button("Reverse")) targetBloc.ReverseContour(contourIndex);
            EditorGUILayout.EndHorizontal();
            if (contourLength == 0)
                EditorGUILayout.HelpBox("Contour is empty", MessageType.Warning);
            else
            {
                // Horizontal point list (displays point indices in bloc)
                // Setup a scroll bar
                float scrollx = contourInspectorStates[contourIndex].pointScroller;
                contourInspectorStates[contourIndex].pointScroller = EditorGUILayout.BeginScrollView(new Vector2(scrollx, 0f)).x;
                // An int field for each point index
                EditorGUILayout.BeginHorizontal();
                for (int pointIndexInContour = 0; pointIndexInContour < contourLength; pointIndexInContour++)
                {
                    int pointIndexInBloc = contourPoints[pointIndexInContour];
                    int editPointIndex = EditorGUILayout.IntField(pointIndexInBloc, GUILayout.MaxWidth(30));
                    if (editPointIndex != pointIndexInBloc)
                    {
                        // If field is edited, apply changes
                        Undo.RecordObjects(targetBlocAndBuilder, "Set Contour Point");
                        targetBloc.ReplacePointInContour(contourIndex, pointIndexInContour, editPointIndex);
                        SceneView.RepaintAll();
                        changeCheck = true;
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();
            }
        }
    }

    #endregion

    // -------------------------------------------
    // HANDLE TOOLBAR: shown in inspector to do operations on handle selection in SceneGUI  
    // -------------------------------------------
    #region Handle toolbar in inspector

    private void HandleInspectorGUI(out bool changeCheck)
    {
        changeCheck = false;
        //Show a handle toolbar in inspectorGUI
        bool GUI_enabled = GUI.enabled;
        EditorGUILayout.BeginHorizontal("box");
        // Get current selection
        List<int> selectedPointsIndices = GetSelectedPointsIndices();
        int selectedPointCount = selectedPointsIndices.Count;
        List<int> selectedContourIndices = GetSelectedContourIndices();
        int selectedContourCount = selectedContourIndices.Count;
        // Button toggle to create a new contour (disables current selection)
        GUI.enabled = GUI_enabled;
        if (GUILayout.Button("New contour"))
        {
            Undo.RecordObjects(targetBlocAndBuilder, "Create contour");
            createContourMode = true;
            targetBloc.AddContour();
            // Select new contour only
            UnselectAll();
            SelectContour(targetBloc.GetContourCount() - 1, true);
            SceneView.RepaintAll();
            changeCheck = true;
            return;
        }
        // Button to select all points and contours
        GUI.enabled = GUI_enabled;
        if (GUILayout.Button("Select all")) SelectAll();
        // Button to clear selection (disabled if no selection)
        GUI.enabled = selectedPointCount >= 1;
        if (GUILayout.Button("Clear selection")) UnselectAll();
        // Button for removing selected points (enabled only if at least one point is selected)
        GUI.enabled = selectedPointCount >= 1;
        if (GUILayout.Button("Remove"))
        {
            Undo.RecordObjects(targetBlocAndBuilder, selectedPointCount == 1 ? "Delete position" : "Delete positions");
            // Remove point only in selected contours
            for (int i = 0; i < selectedPointCount; i++)
            {
                int removedPointIndex = selectedPointsIndices[i];
                targetBloc.RemovePointFromContours(removedPointIndex, selectedContourIndices);
            }
            // Destroy points that are no longer in any contour
            List<int> destroyPoints = selectedPointsIndices.FindAll(pti => targetBloc.GetContoursWithPoint(pti).Count == 0);
            if (destroyPoints.Count > 0) targetBloc.DestroyPoints(destroyPoints);
            // Destroy contours that no longer have points
            List<int> emptyContourIndices = selectedContourIndices.FindAll(cti => targetBloc.GetContour(cti, false).Count == 0);
            targetBloc.RemoveContoursAt(emptyContourIndices);
            // Clear selection
            UnselectAll();
            changeCheck = true;
            SceneView.RepaintAll();
            return;
        }
        // Button for merging selected points (to link contours that share some points, disabled if less thant 2 points are selected)
        GUI.enabled = selectedPointCount >= 2;
        if (GUILayout.Button("Merge"))
        {
            Undo.RecordObjects(targetBlocAndBuilder, "Merge points");
            // Merge selected points
            targetBloc.MergePoints(selectedPointsIndices);
            UnselectAll();
            int newPointIndex = targetBloc.PointCount - 1;
            SelectPoint(newPointIndex, true);
            SelectAllContoursWithPoint(newPointIndex);
            SceneView.RepaintAll();
            changeCheck = true;
            return;
        }
        EditorGUILayout.EndHorizontal();
        // Restore GUI.enabled
        GUI.enabled = GUI_enabled;
    }

    // Show a special toolbar in inspectorGUI when creating a new contour
    private void CreateContourInspectorGUI(out bool changeCheck)
    {
        changeCheck = false;
        bool GUIenabled = GUI.enabled;
        Color GUIbackgroundColor = GUI.backgroundColor;
        // New contour is the only selected contour, stop contour creation if contour selection changes
        List<int> selectedContourIndices = GetSelectedContourIndices();
        if (selectedContourIndices.Count != 1)
        {
            createContourMode = false;
            return;
        }
        int newContourIndex = selectedContourIndices[0];
        // Begin special toolbar
        GUI.enabled = true;
        EditorGUILayout.BeginHorizontal("box");
        EditorGUILayout.LabelField("New contour");
        // Button to confirm contour creation (enabled if at least one point has been added)
        List<int> selectedPointIndices = GetSelectedPointsIndices();
        GUI.enabled = selectedPointIndices != null && selectedPointIndices.Count > 0;
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("OK"))
        {
            createContourMode = false;
            ClearPointSelection();
            SceneView.RepaintAll();
            changeCheck = true;
        }
        // Button to cancel contour creation (remove last contour)
        GUI.enabled = true;
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Cancel"))
        {
            targetBloc.RemoveContourAt(newContourIndex);
            targetBloc.DestroyAllContourlessPoints();
            createContourMode = false;
            UnselectAll();
            SceneView.RepaintAll();
            changeCheck = true;
        }
        EditorGUILayout.EndHorizontal();
        // Restore GUI color
        GUI.backgroundColor = GUIbackgroundColor;
        // Toggle grid mode
        gridSnap = EditorGUILayout.FloatField("Grid snap", gridSnap);
        // Restore GUI.enabled
        GUI.enabled = GUIenabled;
    }
    #endregion
    
    // -------------------------------------------
    // SCENE GUI: shows handles in scene view to select, add an move points with mouse
    // -------------------------------------------
    #region SceneGUI

    private void OnSceneGUI()
    {
        SetTarget();
        if (sceneGUIEditMode)
        {
            if (createContourMode)
                // Special handles when creating new contour
                CreateContourSceneGUI();
            else
                // Normal handles for selecting and moving points
                HandlesSceneGUI();
        }
        if (showContourListInspector)
        {
            // Contour gizmos
            List<List<int>> contours = targetBloc.GetAllContours(false);
            int contourCount = contours.Count;
            // Draw label for each contour and higlight selected contours
            Handles.color = Color.white;
            for (int cti = 0, ctCount = contours != null ? contours.Count : 0; cti < ctCount; cti++)
                if (!IsContourSelected(cti)) DrawContour(contours[cti], cti.ToString());
            // Higlight selected contours
            Handles.color = Color.yellow;
            foreach (int cti in GetSelectedContourIndices())
                if (cti < contourCount)
                    DrawContour(contours[cti], cti.ToString());
        }
    }    

    private void HandlesSceneGUI()
    {
        // Synchronize with inspector
        SetSceneSelectionFromInspector();
        // General size and position values for displaying handles
        Vector3 blocPosition = targetBloc.transform.position;
        float handleSize = .15f * HandleUtility.GetHandleSize(blocPosition);
        // Get selection
        List<int> selectedPointsIndices = GetSelectedPointsIndices();
        int selectedPointCount = selectedPointsIndices.Count;
        List<int> selectedContourIndices = GetSelectedContourIndices();
        int selectedContourCount = selectedContourIndices.Count;
        // Display a handle for each point position
        List<Vector2> targetPositions = targetBloc.GetPositions();
        if (targetPositions == null) return;
        for (int i = 0, iend = targetPositions.Count; i < iend; i++)
        {
            // Handle position is in world space
            Vector3 worldPos = blocPosition + (Vector3)targetPositions[i];
            // Handle if point is already selected
            if (IsPointSelected(i))
            {
                Handles.color = Color.yellow;
                if (Handles.Button(worldPos, Quaternion.identity, 2f * handleSize, handleSize, Handles.SphereHandleCap))
                {
                    // If clicked while holding Ctrl, remove point from selection
                    if (Event.current.modifiers.HasFlag(EventModifiers.Control))
                    {
                        SelectPoint(i, false);
                        // If holding Alt, point selection doesn't affect contour selection
                        if (!Event.current.modifiers.HasFlag(EventModifiers.Alt))
                            UnselectContoursIfNoSelectedPoints(targetBloc.GetContoursWithPoint(i));
                    }
                    // If clicked without Ctrl, cancel current selection and select only this point
                    else
                    {
                        UnselectAll();
                        SelectPoint(i, true);
                        // If holding Alt, point selection doesn't affect contour selection
                        if (!Event.current.modifiers.HasFlag(EventModifiers.Alt))
                            SelectAllContoursWithPoint(i);
                    }
                    // Apply modifications in inspector
                    SetTargetDirty();
                    return;
                }
            }
            // Handle if point is not currently selected
            else
            {
                Handles.color = Color.white;
                if (Handles.Button(worldPos, Quaternion.identity, handleSize, handleSize, Handles.CircleHandleCap))
                {
                    // If clicked without Ctrl, cancel current selection
                    if (!Event.current.modifiers.HasFlag(EventModifiers.Control)) UnselectAll();
                    // Select clicked point
                    SelectPoint(i, true);
                    // If holding Alt, point selection doesn't affect contour selection
                    if (!Event.current.modifiers.HasFlag(EventModifiers.Alt))
                        SelectAllContoursWithPoint(i);
                    // Apply modifications in inspector
                    SetTargetDirty();
                    return;
                }

            }
        }
        // Position handle for selected point(s)
        if (selectedPointCount > 0)
        {
            // Place a handle on one selected point
            Vector3 handlePosition = (Vector3)targetBloc.GetPosition(selectedPointsIndices[0]) + blocPosition;
            // Create position handle and detect if it's moved
            Vector2 handleMove = Handles.PositionHandle(handlePosition, Quaternion.identity) - handlePosition;
            if (handleMove != Vector2.zero)
            {
                // Translate all selected points, following the handle
                Undo.RecordObjects(targetBlocAndBuilder, "Move position");
                // Detach selected points from unselected contour
                List<int> unselectedContourIndices = GetUnselectedContourIndices();
                foreach (int selectedPointIndex in selectedPointsIndices)
                    targetBloc.DetachPointFromContours(selectedPointIndex, unselectedContourIndices);
                // Move selected points
                foreach (int selectedPointIndex in selectedPointsIndices)
                    targetBloc.MovePoint(selectedPointIndex, targetPositions[selectedPointIndex] + handleMove);
                // Apply modifications in inspector
                SetTargetDirty();
            }
        }
        // Handles on segments for inserting points (in selected contours only)
        List<List<int>> contours = targetBloc.GetAllContours(false);  
        Handles.color = Color.yellow;
        foreach(int cti in selectedContourIndices)
        {
            List<int> contourPoints = contours[cti];
            if (contourPoints == null) continue;
            int contourLength = contourPoints.Count;
            for (int i = 0; i < contourLength - 1; i++)
            {
                // Draw handle in the middle of each segment
                Vector3 handlePosition = ((Vector3)targetPositions[contourPoints[i]] + (Vector3)targetPositions[contourPoints[i + 1]]) / 2f + blocPosition;
                if (Handles.Button(handlePosition, Quaternion.identity, .5f * handleSize, handleSize, Handles.CubeHandleCap))
                {
                    Undo.RecordObjects(targetBlocAndBuilder, "Insert point");
                    // Insert point on segment, in selected contour, and select only this point
                    int newPointIndex = targetBloc.PointCount;
                    targetBloc.InsertPointInContours(selectedContourIndices, contourPoints[i], contourPoints[i + 1]);
                    ClearPointSelection();
                    SelectPoint(newPointIndex, true);
                    // Apply modifications in inspector
                    SetTargetDirty();
                }
            }
        }
    }

    private void CreateContourSceneGUI()
    {
        // New contour is the only selected contour
        List<int> selectedContourIndices = GetSelectedContourIndices();
        if (selectedContourIndices.Count != 1)
        {
            createContourMode = false;
            return;
        }
        int newContourIndex = selectedContourIndices[0];
        // Get current contour
        List<int> pointIndicesInBloc = targetBloc.GetContour(newContourIndex, false);
        int newContourLength = pointIndicesInBloc.Count;
        // General size and position values for displaying handles
        Vector3 blocPosition = targetBloc.transform.position;
        float handleSize = .15f * HandleUtility.GetHandleSize(blocPosition);
        // To create contour, we can select existing points and create new ones
        // First, a handle that follows the mouse, to create new points
        Handles.color = Color.green;
        Vector2 mousePosition = Event.current.mousePosition;
        mousePosition.y = SceneView.currentDrawingSceneView.camera.pixelHeight - mousePosition.y;
        mousePosition = SceneView.currentDrawingSceneView.camera.ScreenToWorldPoint(mousePosition);
        Vector2 createPointPosition = mousePosition;
        // Snap position to grid if enabled
        if (gridSnap > 0f)
        {
            createPointPosition = (Vector2)(Vector2Int.RoundToInt(mousePosition / gridSnap)) * gridSnap;
            Handles.CircleHandleCap(0, createPointPosition, Quaternion.identity, .5f * handleSize, EventType.Repaint);
        }
        if (Handles.Button(mousePosition, Quaternion.identity, .5f * handleSize, handleSize, Handles.CircleHandleCap))
        {
            Undo.RecordObjects(targetBlocAndBuilder, "Create contour");
            // Add new point to bloc and to new contour
            targetBloc.AddPoint(createPointPosition - (Vector2)blocPosition);
            int newPointIndex = targetBloc.PointCount - 1;
            targetBloc.AddPointToContour(newContourIndex, newPointIndex);
            // Select added point
            SelectPoint(newPointIndex, true);
            SetTargetDirty();
            return;
        }
        // Then, handles to select existing points
        List<Vector2> targetPositions = targetBloc.GetPositions();
        if (targetPositions != null)
        {
            // One handle for every existing point
            for (int pti = 0, iend = targetPositions.Count; pti < iend; pti++)
            {
                // Handles are in world space
                Vector3 pointPositionWorldSpace = blocPosition + (Vector3)targetPositions[pti];
                // Last point in contour is treated differently
                if (newContourLength > 0 && pti == pointIndicesInBloc[newContourLength - 1])
                {
                    // Last point can't be added but can be removed to easily cancel adding a point
                    if (Handles.Button(pointPositionWorldSpace, Quaternion.identity, 2f * handleSize, handleSize, Handles.SphereHandleCap))
                    {
                        Undo.RecordObjects(targetBlocAndBuilder, "Create contour");
                        // Remove point by trimming contour
                        targetBloc.SetContourLength(newContourIndex, newContourLength - 1);
                        SetTargetDirty();
                        return;
                    }
                    // Draw a dotted line to show future segment
                    Handles.DrawDottedLine(pointPositionWorldSpace, createPointPosition, handleSize * 10f);
                }
                // First point in contour is also treated differently
                else if(newContourLength > 0 && pti == pointIndicesInBloc[0])
                {
                    Handles.color = Color.green;
                    // If first point is clicked again, it ends contour creation and set contour as a loop
                    if (Handles.Button(pointPositionWorldSpace, Quaternion.identity, handleSize, handleSize, Handles.CircleHandleCap))
                    {
                        Undo.RecordObjects(targetBlocAndBuilder, "Create contour");
                        targetBloc.LoopContour(newContourIndex, true);
                        SetTargetDirty();
                        // Select new contour only
                        UnselectAll();
                        SelectContour(newContourIndex, true);
                        createContourMode = false;
                        return;
                    }
                }
                // Other points
                else
                {
                    Handles.color = IsPointSelected(pti) ? Color.green : Color.white;
                    if (Handles.Button(pointPositionWorldSpace, Quaternion.identity, handleSize, handleSize, Handles.CircleHandleCap))
                    {
                        Undo.RecordObjects(targetBlocAndBuilder, "Create contour");
                        // Add point to selection
                        SelectPoint(pti, true);
                        // Add point to contour (again)
                        targetBloc.AddPointToContour(newContourIndex, pti);
                        SetTargetDirty();
                        return;
                    }
                }
            }
        }
        // Higlight created contour
        Handles.color = Color.green;
        DrawContour(targetBloc.GetContour(newContourIndex, false));
    }

    private void DrawContour(List<int> pointIndices, string label = null)
    {
        Vector3 blocPosition = targetBloc.transform.position;
        List<Vector2> pointPositions = targetBloc.GetPositions();
        float handleSize = handleScale * HandleUtility.GetHandleSize(blocPosition);
        int contourLength = pointIndices.Count;
        if (contourLength == 0) return;
        if (contourLength == 1)
            Handles.SphereHandleCap(0, (Vector3)pointPositions[pointIndices[0]] + blocPosition, Quaternion.identity, handleSize, EventType.Repaint);
        else
        {
            Vector3 labelPosition = Vector3.zero;
            for (int i = 0; i < contourLength - 1; i++)
            {
                Handles.DrawLine((Vector3)pointPositions[pointIndices[i]] + blocPosition, (Vector3)pointPositions[pointIndices[i + 1]] + blocPosition);
                if (label != null) labelPosition += (Vector3)pointPositions[pointIndices[i]];
            }
            if (label != null)
            {
                labelPosition += (Vector3)pointPositions[pointIndices[contourLength - 1]];
                labelPosition /= contourLength;
                labelPosition += blocPosition;
                Handles.Label(labelPosition, label);
            }
        }
    }
    #endregion
}
