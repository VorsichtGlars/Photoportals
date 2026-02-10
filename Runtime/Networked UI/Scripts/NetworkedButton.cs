using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

//https://docs.unity3d.com/Packages/com.unity.ugui@1.0/api/UnityEngine.UI.Toggle.html

[RequireComponent(typeof(Button))]
public class NetworkedButton : NetworkBehaviour {
    private Button button;

    private bool pressedLocally = false;
    private NetworkVariable<bool> toggleState = new NetworkVariable<bool>(
        false,
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );

    void Awake() {
        this.button = this.GetComponent<Button>();
    }

    public override void OnNetworkSpawn() {
        this.button.onClick.AddListener(this.OnButtonClickServerRpc);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    private void OnButtonClickServerRpc() {
        Debug.Log("OnButtonClickServerRpc", this);

        //this workaround is necessary to avoid infinite recursion
        this.button.onClick.RemoveListener(this.OnButtonClickServerRpc);
        this.button.onClick.Invoke();
        this.button.onClick.AddListener(this.OnButtonClickServerRpc);
    }
}