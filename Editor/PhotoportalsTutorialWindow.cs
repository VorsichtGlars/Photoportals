using UnityEditor;
using UnityEngine;

namespace VRSYS.Photoportals.Editor
{
    /// <summary>
    /// Tutorial window providing links and resources for the Photoportals package
    /// </summary>
    public class PhotoportalsTutorialWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private GUIStyle headerStyle;
        private GUIStyle sectionStyle;
        private GUIStyle descriptionStyle;
        private GUIStyle buttonStyle;
        private bool stylesInitialized = false;

        private const string DOCUMENTATION_URL = "https://github.com/VorsichtGlars/Photoportals/wiki";
        private const string SETUP_URL = "https://github.com/VorsichtGlars/Photoportals/wiki/Setup";
        private const string REPOSITORY_URL = "https://github.com/VorsichtGlars/Photoportals";
        private const string RESEARCH_PAPER_URL = "https://www.researchgate.net/publication/262161093_Photoportals_Shared_references_in_space_and_time";
        private const string ISSUES_URL = "https://github.com/VorsichtGlars/Photoportals/issues";

        [MenuItem("Window/VRSYS/Photoportals Tutorial")]
        public static void ShowWindow()
        {
            PhotoportalsTutorialWindow window = GetWindow<PhotoportalsTutorialWindow>("Photoportals Tutorial");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void InitializeStyles()
        {
            if (stylesInitialized) return;

            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(10, 10, 10, 10)
            };

            sectionStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft,
                margin = new RectOffset(10, 10, 5, 5)
            };

            descriptionStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                fontSize = 11,
                margin = new RectOffset(10, 10, 5, 5)
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 8, 8),
                fontSize = 12
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitializeStyles();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Header
            GUILayout.Space(10);
            GUILayout.Label("VRSYS Photoportals", headerStyle);
            
            // Welcome section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Welcome to Photoportals", sectionStyle);
            GUILayout.Label(
                "Implementation of Photoportals - a shared reference system in space and time for Virtual Reality. " +
                "Based on research by Kunert et al., this package enables collaborative VR experiences with spatial anchors.",
                descriptionStyle
            );
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);

            // Quick Start Guide
            DrawSection(
                "Quick Start Guide",
                "Learn how to set up and use the Photoportals package in your Unity project.",
                "Open Setup Guide",
                () => Application.OpenURL(SETUP_URL)
            );

            // Documentation
            DrawSection(
                "Documentation",
                "Comprehensive documentation including API reference, tutorials, and usage examples.",
                "Open Documentation",
                () => Application.OpenURL(DOCUMENTATION_URL)
            );

            // Research Paper
            DrawSection(
                "Research Paper",
                "Read the original research paper by Kunert et al. on Photoportals: Shared references in space and time.",
                "Open Research Paper",
                () => Application.OpenURL(RESEARCH_PAPER_URL)
            );

            // Example Scene
            DrawSection(
                "Example Scene",
                "Import the example scene to see Photoportals in action with sample implementations.",
                "Import Example Scene",
                () => {
                    // Open Package Manager to Photoportals package samples
                    UnityEditor.PackageManager.UI.Window.Open("com.vrsys.photoportals");
                }
            );

            // GitHub Repository
            DrawSection(
                "GitHub Repository",
                "Visit the source code repository to contribute, explore the codebase, or stay updated.",
                "Open Repository",
                () => Application.OpenURL(REPOSITORY_URL)
            );

            // Bug Reporting
            DrawSection(
                "Bug Reporting",
                "Found an issue? Report bugs or request features on our GitHub Issues page.",
                "Report Bug",
                () => Application.OpenURL(ISSUES_URL)
            );

            GUILayout.Space(20);

            EditorGUILayout.EndScrollView();
        }

        private void DrawSection(string title, string description, string buttonText, System.Action onButtonClick)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUILayout.Label(title, sectionStyle);
            GUILayout.Label(description, descriptionStyle);
            
            GUILayout.Space(5);
            
            if (GUILayout.Button(buttonText, buttonStyle, GUILayout.Height(30)))
            {
                onButtonClick?.Invoke();
            }
            
            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }
    }
}
