using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UIAudioManager))]
public class UIAudioManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }
} 