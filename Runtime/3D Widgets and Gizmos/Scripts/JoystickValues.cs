using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

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


    public void Awake() {
        this.root = this.transform.Find("Root");
        this.handle = this.transform.Find("Handle");
        this.handleInteractable = this.handle.GetComponent<XRGrabInteractable>();
        this.handleInteractable.selectEntered.AddListener(() => OnJoystickGrabbed.Invoke());
        this.handleInteractable.selectExited.AddListener(() => OnJoystickReleased.Invoke());
        this.handleInteractable.selectExited.AddListener(this.ResetValues);
    }

    public void Update() {
        this.translation = handle.position - root.position;
        this.rotation = handle.localRotation.eulerAngles;
    }

    public Vector3 GetTranslationValue() {
        return this.translation;
    }

    public Vector3 GetRotationValue() {
        return this.rotation;
    }

    private void ResetValues() {
        this.translation = this.rotation = Vector3.zero;
    }
}