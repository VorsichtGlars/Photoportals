using UnityEngine;

namespace VRVIS.Photoportals {

    [AddComponentMenu("Photoportals/Note")]
    public class NoteComponent : MonoBehaviour {

        [TextAreaAttribute(minLines: 5, maxLines: 20)]
        public string note;

        public void Awake() {
#if !UNITY_EDITOR
            Destroy(this);
#endif
        }
    }
}