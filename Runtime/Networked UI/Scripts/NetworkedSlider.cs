using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

/**
    This script distributes a sliders value to its remote clients
    Caution: It can potentially be a lot of calls depending on the slider configuration
**/
[RequireComponent(typeof(Slider))]
public class NetworkedSlider : NetworkBehaviour {
    private Slider slider;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        this.slider = this.GetComponent<Slider>();
        this.slider.onValueChanged.AddListener(this.OnValueChangedServerRpc);
    }

    [Rpc(SendTo.NotMe, RequireOwnership = false)]
    private void OnValueChangedServerRpc(float value) {
        Debug.Log("OnValueChangedServerRpc", this);
        //Removing the listener is necessary to avoid infinite recursion
        this.slider.onValueChanged.RemoveListener(this.OnValueChangedServerRpc);
        this.slider.value = value;
        this.slider.onValueChanged.AddListener(this.OnValueChangedServerRpc);
    }
}