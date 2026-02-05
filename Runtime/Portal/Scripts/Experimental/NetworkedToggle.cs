using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

//https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.UI.Toggle.html

[RequireComponent(typeof(Toggle))]
public class NetworkedToggle : NetworkBehaviour {
    private Toggle toggle;

    public bool distributeVisualsOnly;
    private NetworkVariable<bool> toggleState = new NetworkVariable<bool>(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );

    void Awake() {
        this.toggle = this.GetComponent<Toggle>();
    }

    public override void OnNetworkSpawn() {
        this.toggle.onValueChanged.AddListener(this.UpdateToggleStateServerRpc);
        this.toggleState.OnValueChanged += (oldValue, newValue) => {
            if(this.toggle.isOn == newValue) return;
            this.toggle.isOn = newValue;
            this.toggle.graphic.SetAllDirty();
        };
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateToggleStateServerRpc(bool newValue) {
        this.toggleState.Value = newValue;
    }

    [ContextMenu("Set Toggle On")]
    public void SetToggleOn() {
        this.toggle.isOn = true;
    }

    [ContextMenu("Set Toggle Off")]
    public void SetToggleOff() {
        this.toggle.isOn = false;
    }
}