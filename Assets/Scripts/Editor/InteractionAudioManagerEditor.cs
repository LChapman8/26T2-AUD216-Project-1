using UnityEngine;
using UnityEditor;
using static InteractionAudioManager;

[CustomEditor(typeof(InteractionAudioManager))]
public class InteractionAudioManagerEditor : Editor
{
    SerializedProperty interactionProfiles;
    private bool[] foldoutStates;

    private void OnEnable()
    {
        interactionProfiles = serializedObject.FindProperty("interactionProfiles");
        InitializeFoldoutStates();
    }

    private void InitializeFoldoutStates()
    {
        foldoutStates = new bool[interactionProfiles.arraySize];
        for (int i = 0; i < foldoutStates.Length; i++)
        {
            foldoutStates[i] = true;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        for (int i = 0; i < interactionProfiles.arraySize; i++)
        {
            EditorGUI.indentLevel = 0;
            SerializedProperty profile = interactionProfiles.GetArrayElementAtIndex(i);
            SerializedProperty interactionName = profile.FindPropertyRelative("interactionName");
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Header
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(12);
            string displayName = !string.IsNullOrEmpty(interactionName.stringValue) 
                ? interactionName.stringValue 
                : $"Profile {i + 1}";
            foldoutStates[i] = EditorGUILayout.Foldout(foldoutStates[i], displayName, true);
            EditorGUILayout.EndHorizontal();

            if (foldoutStates[i])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(interactionName);
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("interactionType"));
                
                var interactionType = (InteractionType)profile.FindPropertyRelative("interactionType").enumValueIndex;
                
                switch (interactionType)
                {
                    case InteractionType.SimpleInteraction:
                        EditorGUILayout.LabelField("Simple Audio", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(profile.FindPropertyRelative("interactionSound"));
                        EditorGUILayout.PropertyField(profile.FindPropertyRelative("customAudioSource"));
                        break;

                    case InteractionType.ElectricalBox:
                        EditorGUILayout.LabelField("Electrical Box Audio", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(profile.FindPropertyRelative("leverSound"));
                        EditorGUILayout.Space(5);
                        EditorGUILayout.PropertyField(profile.FindPropertyRelative("sparksSound"));
                        EditorGUILayout.PropertyField(profile.FindPropertyRelative("customAudioSource"));
                        break;

                    case InteractionType.GeneralInteraction:
                        EditorGUILayout.LabelField("General Audio", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(profile.FindPropertyRelative("interactionSounds"));
                        break;

                    case InteractionType.Firesticks:
                    case InteractionType.Campfire:
                        EditorGUILayout.LabelField("Fire Audio", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(profile.FindPropertyRelative("igniteSounds"));
                        EditorGUILayout.PropertyField(profile.FindPropertyRelative("fireLoop"));
                        EditorGUILayout.PropertyField(profile.FindPropertyRelative("fireLoopStartDelay"));
                        break;

                    case InteractionType.MixingDesk:
                        EditorGUILayout.LabelField("Mixing Desk Audio", EditorStyles.boldLabel);
                        
                        EditorGUILayout.LabelField("Left Speaker", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(profile.FindPropertyRelative("leftSpeakerAudio"));
                        EditorGUILayout.PropertyField(profile.FindPropertyRelative("leftSpeakerSource"));
                        
                        EditorGUILayout.Space(5);
                        
                        EditorGUILayout.LabelField("Right Speaker", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(profile.FindPropertyRelative("rightSpeakerAudio"));
                        EditorGUILayout.PropertyField(profile.FindPropertyRelative("rightSpeakerSource"));
                        
                        EditorGUILayout.Space(5);
                        break;
                }

                EditorGUILayout.LabelField("Audio Routing", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("mixerGroupOutput"));
            }

            // Remove button at bottom
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                interactionProfiles.DeleteArrayElementAtIndex(i);
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
            interactionProfiles.arraySize++;
            InitializeFoldoutStates();
        }

        serializedObject.ApplyModifiedProperties();
    }
} 