using UnityEditor;
using UnityEngine;

namespace Vrsys.Photoportals{
    [CustomEditor(typeof(PortalEntrance))]
    public class PortalEntranceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            PortalEntrance portalEntrance = (PortalEntrance)target;
            return;

            if (GUILayout.Button("Generate Portal Entrance"))
            {
                throw new System.NotImplementedException();
            }
        }
    }
}