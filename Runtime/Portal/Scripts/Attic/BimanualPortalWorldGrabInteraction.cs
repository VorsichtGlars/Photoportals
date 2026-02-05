using System;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using VRSYS.Core.Avatar;
using VRSYS.Core.Logging;
using VRSYS.Core.Networking;
using Mu.UnityExtensions;

public class BimanualPortalWorldGrabInteraction : MonoBehaviour, INetworkUserCallbacks {

    public Transform controller;
    public Transform display;
    public Transform view;
    public AvatarHMDAnatomy avatar;

    public Transform userRoot;
    public XRGrabInteractable interactable;

    public InputActionProperty clutchInput;
    public InputActionProperty leftClutchInput;
    public InputActionProperty rightClutchInput;
    public bool invertOffset = true;

    public Transform controller_initial;
    public Transform controller_current;

    public Transform display_initial;
    public Transform display_current; //may have moved due to grabbing

    public Transform view_initial;
    public Transform view_current; //the position we actually want to define

    public Transform controller_current_in_view_space;
    public Transform controller_initial_in_view_space;

    public Matrix4x4 inital_display_to_controller_offset;

    public void Update() {
        if (this.clutchInput.action?.WasPressedThisFrame() == true) {
            ExtendedLogger.LogInfo(this.GetType().Name, "Setting up clutching", this);
            this.controller_initial.position = this.controller.position;
            this.controller_initial.rotation = this.controller.rotation;
            this.display_initial.position = this.display.position;
            this.display_initial.rotation = this.display.rotation;
            this.view_initial.position = this.view_current.position = this.view.position;
            this.view_initial.rotation = this.view_current.rotation = this.view.rotation;

            this.inital_display_to_controller_offset = this.display_initial.GetMatrix4x4().inverse * this.controller_initial.GetMatrix4x4();
            Matrix4x4 tmp1 = this.view_initial.GetMatrix4x4() * this.inital_display_to_controller_offset;

            this.controller_initial_in_view_space.transform.position = tmp1.GetPosition();
            this.controller_initial_in_view_space.transform.rotation = tmp1.rotation;
        }

        if (this.clutchInput.action?.IsPressed() == true) {
            ExtendedLogger.LogInfo(this.GetType().Name, "Performing clutching", this);
            this.controller_current.position = this.controller.position;
            this.controller_current.rotation = this.controller.rotation;
            this.display_current.position = this.display.position;
            this.display_current.rotation = this.display.rotation;

            //this offset should be recalculated every time the the display position changes
            Matrix4x4 current_display_to_controller_offset = this.display_current.GetMatrix4x4().inverse * this.controller_current.GetMatrix4x4();

            Matrix4x4 tmp2 = this.view_current.GetMatrix4x4() * Matrix4x4.Scale(this.view.lossyScale) * current_display_to_controller_offset;
            this.controller_current_in_view_space.transform.position = tmp2.GetPosition();
            this.controller_current_in_view_space.transform.rotation = tmp2.rotation;

            current_display_to_controller_offset = current_display_to_controller_offset.inverse;

            Matrix4x4 test = this.view_initial.GetMatrix4x4() * Matrix4x4.Scale(this.view.lossyScale) * this.inital_display_to_controller_offset * current_display_to_controller_offset;
            this.view_current.position = test.GetPosition();
            this.view_current.rotation = test.rotation;
            this.view.position = this.view_current.position;
            this.view.rotation = this.view_current.rotation;
        }

        if (this.clutchInput.action?.WasReleasedThisFrame() == true) {
            ExtendedLogger.LogInfo(this.GetType().Name, "Cleaning up clutching", this);
        }
    }

    public void OnLocalNetworkUserSetup() {
        this.avatar = NetworkUser.LocalInstance.avatarAnatomy as AvatarHMDAnatomy;
        this.userRoot = this.avatar.transform;
        this.controller = this.avatar.rightHand;
        this.display = GameObject.Find("Portal #0 Display").transform;
        this.view = GameObject.Find("Portal #0 View").transform;
        this.interactable = this.display.GetComponent<XRGrabInteractable>();
    }

    public void OnRemoteNetworkUserSetup(NetworkUser user) {
        throw new System.NotImplementedException();
    }
}