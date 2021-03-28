using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ContourMaterial), true)]
public class ContourMaterialInspector : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck())
            (target as ContourMaterial).ChangeVersion();
    }
}