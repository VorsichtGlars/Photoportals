using UnityEditor;
using UnityEngine;
using VRVIS.Photoportals;

[CustomEditor(typeof(ScreenProperties))]
public class ScreenPropertiesEditor : Editor
{
    SerializedProperty width;
    SerializedProperty height;
    void OnEnable()
    {
        width = this.serializedObject.FindProperty("width");
        height = this.serializedObject.FindProperty("height");
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        serializedObject.Update();
        EditorGUILayout.LabelField("Screen Properties");
        EditorGUILayout.PropertyField(width, new GUIContent ("Width"));
        EditorGUILayout.PropertyField(height, new GUIContent ("Height"));
        serializedObject.ApplyModifiedProperties();
    }
}