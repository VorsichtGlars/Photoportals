using UnityEditor;
using UnityEngine;

namespace Vrsys.Photoportals{
    [CustomEditor(typeof(PortalExit))]
    public class PortalExitEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PortalExit portalEntrance = (PortalExit)target;
            return;
            
            if (GUILayout.Button("Generate Portal Exit"))
            {
                throw new System.NotImplementedException();
            }
        }
    }
}