using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioOcclusion))]
public class AudioOcclusionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector except for transitionSpeed, occludedVolume, and occludedFrequency
        DrawPropertiesExcluding(serializedObject, "transitionSpeed", "occludedVolume", "occludedFrequency");

        // Get a reference to the target script
        AudioOcclusion script = (AudioOcclusion)target;

        // Add space before the sliders
        GUILayout.Space(10);

        // Draw the occludedVolume slider
        script.occludedVolume = EditorGUILayout.Slider("Occluded Volume", script.occludedVolume, 0f, 1f);

        // Adjust vertical space between the sliders
        GUILayout.Space(10);

        // Draw the occludedFrequency slider with the specified range
        script.occludedFrequency = EditorGUILayout.Slider("Occluded Frequency", script.occludedFrequency, 3000f, 20000f);

        // Adjust vertical space between the sliders
        GUILayout.Space(10);

        // Draw the transitionSpeed slider with the new range
        script.transitionSpeed = EditorGUILayout.Slider("Transition Speed", script.transitionSpeed, 1f, 10f);

        // Adjust vertical space before the labels
        GUILayout.Space(-5);

        // Add a horizontal group with two labels below the transitionSpeed slider
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(EditorGUIUtility.labelWidth)); // Empty label for alignment
        GUILayout.Label("Slow", EditorStyles.miniLabel, GUILayout.Width(40));
        GUILayout.FlexibleSpace(); // Add flexible space to adjust the "Fast" label position
        GUILayout.Label("Fast", EditorStyles.miniLabel, GUILayout.Width(80)); // Adjust width as needed
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);


        // Apply any changes to the serializedObject
        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}
