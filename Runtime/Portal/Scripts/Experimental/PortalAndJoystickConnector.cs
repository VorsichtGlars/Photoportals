using UnityEngine;

using Unity.XR.CoreUtils;

using VRSYS.Photoportals;

public class PortalAndJoystickConnector : MonoBehaviour {
    [SerializeField]
    private JoystickValues joystickValues;
    [SerializeField]
    private PortalControl portalControl;
    [SerializeField]
    bool steeringIsActive = false;

    public void Awake() {
        this.joystickValues.OnJoystickGrabbed.AddListener(StartSteering);
    }

    private void StartSteering() {
        this.steeringIsActive = true;
    }

    public void Update() {
        if(this.steeringIsActive == false)
            return;
        var vector = this.joystickValues.GetTranslationValue();
        portalControl.ApplySteeringVector(vector.normalized.Inverse(), vector.magnitude * (1f/0.075f), Space.Self);
    }
}