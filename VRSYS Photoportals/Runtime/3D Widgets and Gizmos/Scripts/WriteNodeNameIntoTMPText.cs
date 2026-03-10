using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class WriteNodeNameIntoTMPText : MonoBehaviour {
    public Transform node;
    public TMP_Text text;

    void Awake() {
        this.UpdateText();
    }

    void OnValidate() {
        this.UpdateText();
    }

    [ContextMenu("UpdateText")]
    void UpdateText() {
        if(this.node == null) {
            this.text.text = "";
            return;
        }
        this.text.text = this.node.name;
    }

    [ContextMenu("ParseForFields")]
    public void ParseForFields() {
        this.node = this.gameObject.transform;
        this.text = this.gameObject.transform.GetComponentInChildren<TMP_Text>();
    }
}