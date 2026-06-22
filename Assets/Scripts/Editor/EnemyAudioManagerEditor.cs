using UnityEngine;
using UnityEditor;
using static EnemyAudioManager;

[CustomEditor(typeof(EnemyAudioManager))]
public class EnemyAudioManagerEditor : Editor
{
    SerializedProperty enemyProfiles;
    SerializedProperty audioPoolSize;
    SerializedProperty maxHearingDistance;
    private bool[] foldoutStates;
    private bool[] audioSetupFoldouts;

    private void OnEnable()
    {
        enemyProfiles = serializedObject.FindProperty("enemyProfiles");
        audioPoolSize = serializedObject.FindProperty("audioPoolSize");
        maxHearingDistance = serializedObject.FindProperty("maxHearingDistance");
        InitializeFoldoutStates();
    }

    private void InitializeFoldoutStates()
    {
        foldoutStates = new bool[enemyProfiles.arraySize];
        audioSetupFoldouts = new bool[enemyProfiles.arraySize];
        for (int i = 0; i < foldoutStates.Length; i++)
        {
            foldoutStates[i] = true;
            audioSetupFoldouts[i] = true;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Enemy Profiles section
        EditorGUILayout.LabelField("Enemy Sound Profiles", EditorStyles.boldLabel);

        for (int i = 0; i < enemyProfiles.arraySize; i++)
        {
            EditorGUI.indentLevel = 0;
            SerializedProperty profile = enemyProfiles.GetArrayElementAtIndex(i);
            SerializedProperty enemyName = profile.FindPropertyRelative("enemyName");
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Header
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(12);
            string displayName = !string.IsNullOrEmpty(enemyName.stringValue) 
                ? enemyName.stringValue 
                : $"Profile {i + 1}";
            foldoutStates[i] = EditorGUILayout.Foldout(foldoutStates[i], displayName, true);
            EditorGUILayout.EndHorizontal();

            if (foldoutStates[i])
            {
                EditorGUI.indentLevel++;
                
                // Basic Settings
                EditorGUILayout.PropertyField(enemyName);
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("enemyType"));
                
                EditorGUILayout.Space(5);
                
                // Sounds
                EditorGUILayout.LabelField("Sounds", EditorStyles.boldLabel);
                SerializedProperty vocalSettings = profile.FindPropertyRelative("generalVocalisations");
                
                // Draw black background for array elements
                EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), new Color(0, 0, 0, 0.1f));
                EditorGUILayout.PropertyField(vocalSettings.FindPropertyRelative("vocalisationSounds"));
                EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), new Color(0, 0, 0, 0.1f));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("attackSounds"));
                EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), new Color(0, 0, 0, 0.1f));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("deathSounds"));

                EditorGUILayout.Space(5);

                // Audio Setup foldout grouping several properties together
                audioSetupFoldouts[i] = EditorGUILayout.Foldout(audioSetupFoldouts[i], "Audio Setup", true);
                if (audioSetupFoldouts[i])
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(profile.FindPropertyRelative("enemyPrefab"));
                    EditorGUILayout.PropertyField(profile.FindPropertyRelative("minVocalDelay"));
                    EditorGUILayout.PropertyField(profile.FindPropertyRelative("maxVocalDelay"));
                    EditorGUILayout.PropertyField(profile.FindPropertyRelative("vocalisationVolume"));
                    EditorGUILayout.PropertyField(profile.FindPropertyRelative("attackVolume"));
                    EditorGUILayout.PropertyField(profile.FindPropertyRelative("deathVolume"));
                    EditorGUILayout.PropertyField(profile.FindPropertyRelative("vocalisationMixerGroup"));
                    EditorGUILayout.PropertyField(profile.FindPropertyRelative("attackMixerGroup"));
                    EditorGUILayout.PropertyField(profile.FindPropertyRelative("attackSoundDelay"));
                    EditorGUI.indentLevel--;
                }
                
                EditorGUI.indentLevel--;
            }

            // Remove button
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                enemyProfiles.DeleteArrayElementAtIndex(i);
                InitializeFoldoutStates();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        if (GUILayout.Button("Add New Profile"))
        {
            enemyProfiles.arraySize++;
            InitializeFoldoutStates();
        }

        serializedObject.ApplyModifiedProperties();
    }
} 