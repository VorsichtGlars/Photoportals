using UnityEngine;
using VRSYS.Core.Networking;

public class InterpolationToggle : MonoBehaviour
{
    public bool interpolate = true;

    public void Start()
    {
        this.SetInterpolation(this.interpolate);
    }

    public void SetInterpolation(bool value)
    {
        this.interpolate = value;
        ClientNetworkTransform[] clientNetworkTransforms = this.gameObject.GetComponentsInChildren<ClientNetworkTransform>();
        foreach (ClientNetworkTransform clientNetworkTransform in clientNetworkTransforms)
        {
            clientNetworkTransform.Interpolate = this.interpolate;
        }
    }

    public void ActivateInterpolation()
    {
        this.SetInterpolation(true);
    }

    public void DeactivateInterpolation()
    {
        this.SetInterpolation(false);
    }
}