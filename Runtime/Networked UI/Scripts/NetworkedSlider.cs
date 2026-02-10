using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

[RequireComponent(typeof(Slider))]
public class NetworkedSlider : NetworkBehaviour
{
    private Slider slider;

    private bool changeAppliedLocally;

    private NetworkVariable<float> sliderState = new NetworkVariable<float>(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );

    void Awake()
    {
        this.slider = this.GetComponent<Slider>();
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        this.slider.onValueChanged.AddListener(this.OnValueChanged);
    }

    private void OnValueChanged(float value) {
        this.slider.value = 1.0f;
    }

}