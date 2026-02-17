using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class JoystickSummoning : MonoBehaviour {

    public UnityEvent SummoningGestureDetected;
    public UnityEvent RevocingGestureDetected;
    private float lastSelectionEnter;

    private bool isSummoned = false;

    private GameObject joystick;
    private GameObject joystickRoot;

    public float threshold = 0.2f;


    private void Awake() {
        this.joystick = this.transform.Find("Six DOF Joystick Portal Grab").gameObject;
        this.joystickRoot = this.transform.Find("JoystickRoot").gameObject;
        this.SetInteractableState(this.isSummoned);
    }

    public void OnSelectionEnterEvent(SelectEnterEventArgs args) {
        this.lastSelectionEnter = Time.time;
    }

    public void OnSelectionExitEvent(SelectExitEventArgs args) {
        float delta = Time.time - this.lastSelectionEnter;
        if(delta > this.threshold) return;
        this.isSummoned = !this.isSummoned;
        // TODO
        // maybe it is necessary to also en/dis-able the interactable to prevent accidental grabs of an invisible handle
        if(this.isSummoned == true){
            this.joystick.transform.DOFollowTransform(args.interactorObject.transform, 0.25f).
            OnComplete(() => {
                this.SetInteractableState(true);
            });
        }

        if(this.isSummoned == false){
            this.joystick.transform.DOFollowTransform(this.joystickRoot.transform, 0.25f).
            OnStart(() => {
                this.SetInteractableState(false);
            });
        }
    }

    private void SetRendererState(bool state) {
        var renderers = this.joystick.GetComponentsInChildren<Renderer>();
        foreach(var renderer in renderers) {
            renderer.enabled = state;
        }
    }

    private void SetInteractableState(bool state) {
        //TODO this should be distributed to every client,
        // otherwise the interactable is grabbable for the other clients
        var interactables = this.joystick.GetComponentsInChildren<XRBaseInteractable>();
        foreach(var interactable in interactables) {
            interactable.enabled = state;
        }
    }   
}