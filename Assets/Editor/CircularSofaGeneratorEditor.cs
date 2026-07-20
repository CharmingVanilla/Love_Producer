using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CircularSofaGenerator))]
public sealed class CircularSofaGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CircularSofaGenerator generator = (CircularSofaGenerator)target;
        EditorGUILayout.Space(10f);

        if (generator.SofaPiecePrefab == null)
        {
            EditorGUILayout.HelpBox(
                "Assign a Sofa Piece Prefab before building, or create a scene-only placeholder segment.",
                MessageType.Warning);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Build Sofa", GUILayout.Height(30f)))
                generator.BuildSofa();

            if (GUILayout.Button("Rebuild Sofa", GUILayout.Height(30f)))
                generator.RebuildSofa();
        }

        if (GUILayout.Button("Clear Sofa", GUILayout.Height(26f)))
            generator.ClearSofa();

        if (generator.SofaPiecePrefab == null &&
            GUILayout.Button("Create Placeholder Segment", GUILayout.Height(26f)))
        {
            generator.CreatePlaceholderSegment();
        }
    }
}
