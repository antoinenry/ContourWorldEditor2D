using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ContourBlocBuilder))]
public class ContourBlocBuilderInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Build"))
        {
            (target as ContourBlocBuilder).Build();
        }
    }
}
