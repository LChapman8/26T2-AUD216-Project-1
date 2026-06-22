using UnityEngine;
using UnityEditor;
using Audio;

[CustomEditor(typeof(FootstepAudioManager))]
public class FootstepAudioManagerEditor : Editor
{
    private SerializedProperty surfaceProfiles;
    private SerializedProperty movementProfile;
    private SerializedProperty audioSource;
    private SerializedProperty sfxMixerGroup;

    private bool[] foldoutStates;

    private void OnEnable()
    {
        surfaceProfiles = serializedObject.FindProperty("surfaceProfiles");
        movementProfile = serializedObject.FindProperty("movementProfile");
        audioSource = serializedObject.FindProperty("audioSource");
        sfxMixerGroup = serializedObject.FindProperty("sfxMixerGroup");
        
        InitializeFoldoutStates();
    }

    private void InitializeFoldoutStates()
    {
        if (foldoutStates == null || foldoutStates.Length != surfaceProfiles.arraySize)
        {
            bool[] newStates = new bool[surfaceProfiles.arraySize];
            for (int i = 0; i < newStates.Length; i++)
            {
                newStates[i] = true; // Default to expanded
            }
            foldoutStates = newStates;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Footstep Surface Sounds", EditorStyles.boldLabel);
        
        // Resize foldout states if array size changes
        if (foldoutStates == null || foldoutStates.Length != surfaceProfiles.arraySize)
        {
            bool[] newStates = new bool[surfaceProfiles.arraySize];
            if (foldoutStates != null)
            {
                for (int i = 0; i < Mathf.Min(foldoutStates.Length, newStates.Length); i++)
                    newStates[i] = foldoutStates[i];
            }
            foldoutStates = newStates;
        }

        // Draw array elements with foldouts
        for (int i = 0; i < surfaceProfiles.arraySize; i++)
        {
            SerializedProperty element = surfaceProfiles.GetArrayElementAtIndex(i);
            SerializedProperty surfaceType = element.FindPropertyRelative("surfaceType");
            SerializedProperty footstepSounds = element.FindPropertyRelative("footstepSounds");
            SerializedProperty baseVolume = element.FindPropertyRelative("baseVolume");

            string label = string.IsNullOrEmpty(surfaceType.stringValue) ? $"Surface {i}" : surfaceType.stringValue;
            
            // Use stored foldout state
            foldoutStates[i] = EditorGUILayout.Foldout(foldoutStates[i], label, true);
            
            if (foldoutStates[i])
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(surfaceType, new GUIContent("Surface Type"));
                
                // Just draw the footstep sounds elements
                for (int j = 0; j < footstepSounds.arraySize; j++)
                {
                    SerializedProperty soundElement = footstepSounds.GetArrayElementAtIndex(j);
                    EditorGUILayout.PropertyField(soundElement, new GUIContent($"Sound {j + 1}"));
                }

                EditorGUILayout.PropertyField(baseVolume);
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
        }

        EditorGUILayout.Space(10);
        // Remove headers and update labels
        SerializedProperty jumpSounds = movementProfile.FindPropertyRelative("jumpSounds");
        SerializedProperty jumpVolume = movementProfile.FindPropertyRelative("jumpVolume");
        SerializedProperty landSounds = movementProfile.FindPropertyRelative("landSounds");
        SerializedProperty landVolume = movementProfile.FindPropertyRelative("landVolume");

        EditorGUILayout.PropertyField(jumpSounds, new GUIContent("Jump Sounds"));
        EditorGUILayout.PropertyField(jumpVolume, new GUIContent("Jump Volume"));
        EditorGUILayout.PropertyField(landSounds, new GUIContent("Land Sounds"));
        EditorGUILayout.PropertyField(landVolume, new GUIContent("Land Volume"));
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Audio Setup", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(audioSource, new GUIContent("Audiosource"));
        EditorGUILayout.PropertyField(sfxMixerGroup, new GUIContent("Mixer Group Output"));

        serializedObject.ApplyModifiedProperties();
    }
} 