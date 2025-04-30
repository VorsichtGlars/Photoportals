using UnityEngine;
using UnityEngine.InputSystem;

public class BasicSteeringInteraction : MonoBehaviour {

    public InputActionProperty steeringInput;
    public Transform viewTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (this.steeringInput.action?.WasPressedThisFrame() == true) {
            Debug.Log("Steering WasPressed");
        }

        if (this.steeringInput.action?.IsPressed() == true) {
            Debug.Log("Steering IsPressed");
            float steeringValue = (float)this.steeringInput.action?.ReadValue<float>();
            this.viewTransform.position += steeringValue * Time.deltaTime * this.viewTransform.forward;
        }

        if (this.steeringInput.action?.WasReleasedThisFrame() == true) {
            Debug.Log("Steering WasReleased");
        }
    }
}