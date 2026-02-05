using DG.Tweening;
using Mu.UnityExtensions;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using VRVIS.Photoportals;

public class JoystickGrabTranformer : XRBaseGrabTransformer {
    [SerializeField]
    private string status;
    [SerializeField]
    private Transform root;
    private float sphereRadius = 0.075f;
    private XRGrabInteractable handle;

    //hacky area

    public PortalControl portalControl;
    public float steeringSpeed = 1.0f;

    //

    public override void OnLink(XRGrabInteractable grabInteractable) {
        base.OnLink(grabInteractable);
        this.handle = grabInteractable;
        this.handle.selectExited.AddListener(ResetHandle);
    }
    public override void OnUnlink(XRGrabInteractable grabInteractable) {
        base.OnUnlink(grabInteractable);
    }

    public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale) {
        var interactor = grabInteractable.firstInteractorSelecting;
        var difference = interactor.transform.position - root.position;

        var steeringVector = difference;

        var localRepr = this.portalControl.gameObject.transform.GetMatrix4x4().inverse * steeringVector;
        var viewRepr = this.portalControl.viewTransform.GetMatrix4x4() * localRepr;
        viewRepr *= this.steeringSpeed * Time.deltaTime;
        this.portalControl.ApplySteeringVector(viewRepr, Space.Self);

        if(difference.magnitude < this.sphereRadius)
            return;

        targetPose = new Pose(this.root.position + difference.normalized * this.sphereRadius,targetPose.rotation);
    }

    public void ResetHandle() {
        // call from outside might collide with the Process function still being executed
        this.handle.transform.DOFollowTransform(this.root,0.5f).
            SetEase(Ease.OutBack);
    }
}