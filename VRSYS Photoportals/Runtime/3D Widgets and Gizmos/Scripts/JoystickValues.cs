using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

using VRSYS.Photoportals.Extensions;

public class JoystickValues : MonoBehaviour {
    private Transform root;
    private Transform handle;

    [SerializeField]
    private Vector3 translation;
    [SerializeField]
    private Vector3 rotation;

    public UnityEvent OnJoystickGrabbed;
    public UnityEvent OnJoystickReleased;
    private XRGrabInteractable handleInteractable;
    private float sphereRadius;


    public void Awake() {
        this.root = this.transform.Find("Root");
        this.handle = this.transform.Find("Handle");
        this.handleInteractable = this.handle.GetComponent<XRGrabInteractable>();
        this.handleInteractable.selectEntered.AddListener(() => OnJoystickGrabbed.Invoke());
        this.handleInteractable.selectExited.AddListener(() => OnJoystickReleased.Invoke());
        this.handleInteractable.selectExited.AddListener(this.ResetValues);
        this.sphereRadius = this.transform.Find("Interaction Volume").localScale.x / 2f;
    }

    public void Update() {
        this.translation = handle.position - root.position;
        this.rotation = handle.localRotation.eulerAngles;
    }

    public Vector3 Direction(Space space = Space.World) {
        if (space == Space.Self) {
            return this.transform.InverseTransformDirection(this.translation.normalized);
        }
        return this.translation.normalized;
    }

    public float Magnitude() {
        return this.translation.magnitude / this.sphereRadius;
    }

    public Vector3 Rotation() {
        return this.rotation;
    }

    private void ResetValues() {
        this.translation = this.rotation = Vector3.zero;
    }
}