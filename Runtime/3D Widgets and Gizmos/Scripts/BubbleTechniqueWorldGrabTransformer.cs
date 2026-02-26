using System;
using DG.Tweening;
using VRSYS.Photoportals.Extensions;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace VRSYS.Photoportals {    
public class BubbleTechniqueWorldGrabTransformer : XRBaseGrabTransformer {
    [SerializeField]
    private string status;

    [SerializeField]
    private XRGrabInteractable interactable;

    //this reference should not be here. it should be a reference to a transform that is manipulated or an event that is invoced
    [SerializeField]
    private PortalControl portalControl;

    private Vector3 oldPosition;
    private Tweener tween;

    [SerializeField]
    private float radius = 0.2f;
    private float maxGrab = 0.5f;

    [SerializeField]
    private Transform root;
    private Matrix4x4 initialMatrix;

    [SerializeField]
    private TestTransferfunction transferFunction = new TestTransferfunction();

    [SerializeField]
    private float steeringSpeed = 1.0f;

    /**
    //Good to know these exist
    public override void OnLink(XRGrabInteractable grabInteractable) {
        base.OnLink(grabInteractable);
    }
    public override void OnUnlink(XRGrabInteractable grabInteractable) {
        base.OnUnlink(grabInteractable);
    }
    **/

    public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale) {
        var interactor = grabInteractable.firstInteractorSelecting;
        var offset = interactor.transform.position - root.position;
        var direction = offset.normalized;

        var insideSphere = offset.magnitude < this.radius;

        //1:1 mapping inside the sphere
        if (insideSphere == true) {
            status = "inside Sphere stuff";
            targetPose = new Pose(interactor.transform.position, Quaternion.identity);
        }

        //scaled mapping outside the sphere
        if ( insideSphere == false) {
            status = "outside Sphere stuff";
            //var transferValue = transferFunction.Apply(offset.magnitude-this.radius);
            //var scaledOffset = direction * transferValue;
            //var diff = (offset.magnitude - this.radius);
            var newPostion = this.root.position + direction * this.radius;
            newPostion = newPostion + (direction * (float)(Math.Log(offset.magnitude + 1)*2f)) * 0.01f * 7f;
            targetPose = new Pose(newPostion, Quaternion.identity);

            //this.portalControl.EnqueueSteeringVector(direction.normalized * Time.deltaTime);
            var steeringVector = -1f * (offset.magnitude - this.radius) * this.steeringSpeed * direction.normalized * Time.deltaTime;
            this.portalControl.EnqueueSteeringVector(steeringVector, Space.World);
        }

        return;
        /**
        var interactor = grabInteractable.firstInteractorSelecting;
        var direction = interactor.transform.position - root.position;
        //mapping interactor position to the handle position
        var transferValue = transferFunction.Apply(direction.magnitude / this.maxGrab);
        transferValue = Math.Clamp(transferValue, 0.0f, this.radius);
        var offset = direction.normalized * transferValue;
        targetPose = new Pose(root.position + offset, Quaternion.identity);

        //mapping handle position to the steering position and rotation
        //offset = offset * Time.deltaTime;
        portalControl.ApplySteering(offset);
        **/
        /**
        var currentOffsetMatrix = this.initialMatrix.inverse * interactor.transform.GetMatrix4x4();
        currentOffsetMatrix = currentOffsetMatrix.inverse;
        var rotationOffset = currentOffsetMatrix.rotation.eulerAngles;
        rotationOffset *= 0.01f;
        rotationOffset = rotationOffset * Time.deltaTime;
        portalControl.viewTransform.Rotate(rotationOffset);
        **/
    }

    private void OnSelectEntered(SelectEnterEventArgs args) {
        Debug.Log("Joystick.OnSelectEntered");
        this.initialMatrix = this.interactable.firstInteractorSelecting.transform.GetMatrix4x4();
        //this.portalControl.SwitchToBimanualInteraction(args); //deprecated
    }

    private void OnSelectExited(SelectExitEventArgs args) {
        Debug.Log("Joystick.OnSelectExited");
        //this.portalControl.SwitchToUnimanualInteraction(args); //deprecated
        this.tween = this.interactable.transform.
            DOMove(this.root.position, 0.5f).
            SetEase(Ease.OutBack);
        oldPosition = this.root.position;
    }

    void Update() {
        //the tween should follow the root in case the portal is actually being moved by the other hand
        //http://forum.demigiant.com/index.php?topic=137.0
        if(this.tween != null && oldPosition != this.root.position) {
            this.tween.ChangeEndValue(this.root.position, true).Restart();
            oldPosition = this.root.position;
        }
    }

    void Awake() {
        this.interactable.selectEntered.AddListener(OnSelectEntered);
        this.interactable.selectExited.AddListener(OnSelectExited);
        this.transferFunction.SetMinimum(0.0f);
        this.transferFunction.SetMaximum(this.radius);
        this.transferFunction.SetShape(0.8f);
    }
}

}