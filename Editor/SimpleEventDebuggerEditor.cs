#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace VRSYS.Photoportals {

    [CustomEditor(typeof(SimpleEventDebugger))]
    public class SimpleEventDebuggerEditor : Editor {
        Vector2 scrollPos;
        int maxLines = 10;
        int prevHistoryCount = 0;

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            var targetScript = (SimpleEventDebugger)target;

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Event History", EditorStyles.boldLabel);
            if (GUILayout.Button("Copy To Clipboard", GUILayout.Width(150))) {
                if (targetScript.History != null && targetScript.History.Count > 0) {
                    string historyText = string.Join("\n", targetScript.History);
                    EditorGUIUtility.systemCopyBuffer = historyText;
                } else {
                    EditorGUIUtility.systemCopyBuffer = "No events logged yet.";
                }
            }
            if (GUILayout.Button("Clear", GUILayout.Width(70))) {
                targetScript.ClearHistory();
                prevHistoryCount = 0;
            }
            EditorGUILayout.EndHorizontal();

            float scrollViewHeight = EditorGUIUtility.singleLineHeight * maxLines + 8;
            int historyCount = targetScript.History != null ? targetScript.History.Count : 0;
            
            // Only auto-scroll to bottom if new entries were added
            if (historyCount > prevHistoryCount) {
                scrollPos.y = float.MaxValue;
            }
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(scrollViewHeight));
            if (historyCount > 0) {
                for (int i = 0; i < historyCount; i++) {
                    EditorGUILayout.LabelField(targetScript.History[i], EditorStyles.miniLabel);
                }
            } else {
                EditorGUILayout.LabelField("No events logged yet.", EditorStyles.miniLabel);
                for (int i = 1; i < maxLines; i++) {
                    EditorGUILayout.LabelField("", EditorStyles.miniLabel);
                }
            }
            EditorGUILayout.EndScrollView();
            
            prevHistoryCount = historyCount;
        }
    }
}
#endif
