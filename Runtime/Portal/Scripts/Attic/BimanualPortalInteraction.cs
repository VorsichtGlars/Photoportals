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

public class BimanualPortalInteraction : MonoBehaviour, INetworkUserCallbacks {

    public Transform inputTransform;
    public Transform displayInputTransform;
    public AvatarHMDAnatomy avatar;

    public Transform inputStart;
    public Transform inputEnd;
    public Transform objectStart;
    public Transform objectEnd;

    public Transform objectToManipulate;
    public XRGrabInteractable interactable;
    public InputActionProperty clutchInput;
    public InputActionProperty leftClutchInput;
    public InputActionProperty rightClutchInput;

    public bool invertOffset = true;

    public void Update() {

        //todo: identify bimanual distant
        //todo: identify hand or controller usage to expose widgets
        if (this.leftClutchInput.action?.WasPressedThisFrame() == true) {
            ExtendedLogger.LogInfo(this.GetType().Name, "Left clutch input was pressed", this);
            if (this.interactable.interactorsSelecting.Count == 1) {
                if (this.interactable.interactorsSelecting[0].handedness == InteractorHandedness.Left) {
                    ExtendedLogger.LogInfo(this.GetType().Name, "And left contoller also has the portal grabbed -> Unimanual direct interaction", this);
                    this.inputTransform = this.displayInputTransform;
                    this.invertOffset = true;
                }
                else if (this.interactable.interactorsSelecting[0].handedness == InteractorHandedness.Right) {
                    ExtendedLogger.LogInfo(this.GetType().Name, "And right controller has the portal grabbed -> Bimanual mixed interaction", this);
                    this.inputTransform = this.avatar.leftHand;
                    this.invertOffset = false;
                }
            }
            else {
                ExtendedLogger.LogInfo(this.GetType().Name, "And nothing touches the portal -> Unimanual distant interaction", this);
                //todo retreive the right avatar/interactor for multiuser support
                this.inputTransform = this.avatar.leftHand;
                this.invertOffset = false;
            }
        }

        if (this.rightClutchInput.action?.WasPressedThisFrame() == true) {
            ExtendedLogger.LogInfo(this.GetType().Name, "Right clutch input was pressed", this);
            if (this.interactable.interactorsSelecting.Count == 1) {
                if (this.interactable.interactorsSelecting[0].handedness == InteractorHandedness.Left) {
                    ExtendedLogger.LogInfo(this.GetType().Name, "And left contoller also has the portal grabbed -> Bimanual mixed interaction", this);
                    this.inputTransform = this.avatar.rightHand;
                    this.invertOffset = false;
                }
                else if (this.interactable.interactorsSelecting[0].handedness == InteractorHandedness.Right) {
                    ExtendedLogger.LogInfo(this.GetType().Name, "And right controller has the portal grabbed -> Unimanual direct interaction", this);
                    this.inputTransform = this.displayInputTransform;
                    this.invertOffset = true;
                }
            }
            else {
                ExtendedLogger.LogInfo(this.GetType().Name, "And nothing touches the portal -> Unimanual distant interaction", this);
                //todo retreive the right avatar/interactor for multiuser support
                this.inputTransform = this.avatar.rightHand;
                this.invertOffset = false;
            }
        }

        if (this.inputTransform == null)
            return;

        if (this.clutchInput.action?.WasPressedThisFrame() == true) {
            ExtendedLogger.LogInfo(this.GetType().Name, "Setting up clutching", this);
            this.inputStart.position = this.inputTransform.position;
            this.inputStart.rotation = this.inputTransform.rotation;
            //this.inputStart.DOMove(this.inputTransform.position, 0.1f);
            //this.inputStart.DORotate(this.inputTransform.rotation.eulerAngles, 0.1f);
            this.objectStart.position = this.objectEnd.position = this.objectToManipulate.position;
            this.objectStart.rotation = this.objectEnd.rotation = this.objectToManipulate.rotation;
        }

        if (this.clutchInput.action?.IsPressed() == true) {
            ExtendedLogger.LogInfo(this.GetType().Name, "Performing clutching", this);

            //this might be the grab interactable or the controller root
            this.inputEnd.position = this.inputTransform.position;
            this.inputEnd.rotation = this.inputTransform.rotation;

            Matrix4x4 offsetMat = this.inputEnd.GetMatrix4x4().inverse * this.inputStart.GetMatrix4x4();

            if (this.invertOffset)
                offsetMat = offsetMat.inverse;

            Matrix4x4 finalMat = this.objectStart.GetMatrix4x4() * Matrix4x4.Scale(this.objectToManipulate.lossyScale) * offsetMat;
            this.objectEnd.position = finalMat.GetPosition();
            this.objectEnd.rotation = finalMat.rotation;
            this.objectToManipulate.position = this.objectEnd.position;
            this.objectToManipulate.rotation = this.objectEnd.rotation;
        }

        if (this.clutchInput.action?.WasReleasedThisFrame() == true) {
            ExtendedLogger.LogInfo(this.GetType().Name, "Cleaning up clutching", this);
        }
        //only on one screen
        //UGizmos.DrawWireCube(this.transformToManipulate.position, this.transformToManipulate.rotation, this.transformToManipulate.lossyScale, Color.white);
    }

    public void OnLocalNetworkUserSetup() {
        this.avatar = NetworkUser.LocalInstance.avatarAnatomy as AvatarHMDAnatomy;
        this.displayInputTransform = GameObject.Find("Portal #0 Display").transform;
        this.objectToManipulate = GameObject.Find("Portal #0 View").transform;
        this.interactable = this.displayInputTransform.GetComponent<XRGrabInteractable>();
    }

    public void OnRemoteNetworkUserSetup(NetworkUser user) {
        throw new System.NotImplementedException();
    }
}