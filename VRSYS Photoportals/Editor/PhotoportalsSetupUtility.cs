#if UNITY_EDITOR

using UnityEditor;
using VRSYS.Core.Logging;

namespace VRSYS.Photoportals.Editor {
    public static class PhotoportalsSetupUtility
    {
        #region Variables

        private static string _logTag = "PhotoportalsSetupUtility";
        
        #endregion

        #region MenuItems

        [MenuItem("VRSYS/Photoportals/Setup/Default Layers")]
        public static void SetupLayers(MenuCommand menuCommand) {
            CreateLayer(7, "DirectUI");
            CreateLayer(8, "PokeUI");
        }
        
        #endregion

        #region Private Methods
        //TODO this exact same code is in VRSYSBaseSetupUtility but set to private
        private static void CreateLayer(int layerIdx, string layerName)
        {
            // Open tag manager
            SerializedObject tagManager =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            
            // Layer property
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            if (!PropertyExists(layersProp, layerIdx, layerName))
            {
                SerializedProperty newLayer = layersProp.GetArrayElementAtIndex(layerIdx);
                newLayer.stringValue = layerName;
                
                ExtendedLogger.LogInfo(_logTag, $"Layer {layerName} successfully created as layer {layerIdx}.");

                tagManager.ApplyModifiedProperties();
            }
            else
            {
                ExtendedLogger.LogError(_logTag, $"Layer {layerName} could not be created, since layer {layerIdx} is already used.");
            }
        }

        private static bool PropertyExists(SerializedProperty property, int idx, string value)
        {
            SerializedProperty p = property.GetArrayElementAtIndex(idx);

            return p.stringValue.Equals(value);
        }

        #endregion
    }
}

#endif