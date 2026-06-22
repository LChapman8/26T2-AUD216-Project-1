using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WeaponAudioManager))]
public class WeaponAudioManagerEditor : Editor
{
    private SerializedProperty weaponProfiles;
    private SerializedProperty weaponAudioSource;
    private SerializedProperty audioMixerGroup;
    
    // Add foldout states array
    private bool[] profileFoldouts;

    private void OnEnable()
    {
        weaponProfiles = serializedObject.FindProperty("m_WeaponProfiles");
        weaponAudioSource = serializedObject.FindProperty("m_WeaponAudioSource");
        audioMixerGroup = serializedObject.FindProperty("m_AudioMixerGroup");
        
        // Initialize foldout states
        InitializeFoldoutStates();
    }

    private void InitializeFoldoutStates()
    {
        if (profileFoldouts == null || profileFoldouts.Length != weaponProfiles.arraySize)
        {
            profileFoldouts = new bool[weaponProfiles.arraySize];
            for (int i = 0; i < profileFoldouts.Length; i++)
            {
                profileFoldouts[i] = true; // Default to expanded
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Weapon Profiles", EditorStyles.boldLabel);

        // Draw each profile
        for (int i = 0; i < weaponProfiles.arraySize; i++)
        {
            SerializedProperty profile = weaponProfiles.GetArrayElementAtIndex(i);
            SerializedProperty weaponName = profile.FindPropertyRelative("weaponName");
            SerializedProperty weaponType = profile.FindPropertyRelative("weaponType");
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Header with indented foldout
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(12);
            string label = string.IsNullOrEmpty(weaponName.stringValue) ? $"Weapon {i}" : weaponName.stringValue;
            profileFoldouts[i] = EditorGUILayout.Foldout(profileFoldouts[i], label, true);
            EditorGUILayout.EndHorizontal();

            if (profileFoldouts[i])
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.PropertyField(weaponName);
                EditorGUILayout.PropertyField(weaponType);
                
                var type = (WeaponType)weaponType.enumValueIndex;
                DrawWeaponTypeProperties(profile, type);
                
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        if (GUILayout.Button("Add New Weapon Profile"))
        {
            weaponProfiles.arraySize++;
            InitializeFoldoutStates();
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Audio Setup", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(weaponAudioSource, new GUIContent("Weapon Audiosource"));
        EditorGUILayout.PropertyField(audioMixerGroup, new GUIContent("Mixer Group Output"));

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawWeaponTypeProperties(SerializedProperty profile, WeaponType type)
    {
        // Draw properties with type-specific labels
        switch (type)
        {
            case WeaponType.Gun:
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("shootSounds"), new GUIContent("Shoot Sounds"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("shootVolume"), new GUIContent("Shoot Volume"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("minPitchVariation"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("maxPitchVariation"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("raiseWeaponSound"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("lowerWeaponSound"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("toggleSoundVolume"), new GUIContent("Raise/Lower Volume"));
                break;

            case WeaponType.Axe:
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("shootSounds"), new GUIContent("Axe Swing Sounds"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("shootVolume"), new GUIContent("Axe Volume"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("minPitchVariation"), new GUIContent("Min Swing Pitch"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("maxPitchVariation"), new GUIContent("Max Swing Pitch"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("raiseWeaponSound"), new GUIContent("Draw Axe Sound"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("lowerWeaponSound"), new GUIContent("Sheathe Axe Sound"));
                EditorGUILayout.PropertyField(profile.FindPropertyRelative("toggleSoundVolume"), new GUIContent("Draw/Sheathe Volume"));
                break;
        }
        
        // Commented out reload properties for future implementation
        /*
        EditorGUILayout.PropertyField(profile.FindPropertyRelative("reloadStartSound"));
        EditorGUILayout.PropertyField(profile.FindPropertyRelative("reloadEndSound"));
        EditorGUILayout.PropertyField(profile.FindPropertyRelative("reloadActionSounds"));
        EditorGUILayout.PropertyField(profile.FindPropertyRelative("reloadVolume"));
        */
    }
} 