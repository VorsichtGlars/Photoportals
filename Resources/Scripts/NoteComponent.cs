using Unity.VisualScripting;
using UnityEngine;

namespace VRVIS.Photoportals{
    
    [AddComponentMenu("Photoportals/Note")]
    public class NoteComponent : MonoBehaviour
    {
        public string note;

        public void Awake(){
            #if !UNITY_EDITOR
            Destroy(this);
            #endif
        }
    }
}