using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RiverAudioManager))]
public class RiverAudioManagerEditor : Editor
{
    private SerializedProperty riverProfiles;

    private void OnEnable()
    {
        riverProfiles = serializedObject.FindProperty("riverProfiles");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("River Management", EditorStyles.boldLabel);
        
        // Draw array size field
        EditorGUI.BeginChangeCheck();
        int arraySize = EditorGUILayout.IntField("Size", riverProfiles.arraySize);
        if (EditorGUI.EndChangeCheck())
        {
            riverProfiles.arraySize = arraySize;
        }

        // Draw each profile with fixed names
        for (int i = 0; i < riverProfiles.arraySize; i++)
        {
            SerializedProperty profile = riverProfiles.GetArrayElementAtIndex(i);
            string label = $"River Area {i + 1}";
            EditorGUILayout.PropertyField(profile, new GUIContent(label), true);
        }

        serializedObject.ApplyModifiedProperties();
    }
} 