using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

using DG.Tweening;

using VRSYS.Photoportals;
using VRSYS.Photoportals.Extensions;

public class JoystickGrabTranformer : XRBaseGrabTransformer {
    private Transform root;
    private float sphereRadius;

    public override void OnLink(XRGrabInteractable grabInteractable) {
        base.OnLink(grabInteractable);
        grabInteractable.selectExited.AddListener(() => {
            grabInteractable.transform.DOFollowTransform(this.root,0.5f).
            SetEase(Ease.OutBack);
        });
        this.root = this.transform.parent.Find("Root");
        this.sphereRadius = this.transform.parent.Find("Interaction Volume").localScale.x / 2f;
    }

    public override void Process(XRGrabInteractable grabInteractable, XRInteractionUpdateOrder.UpdatePhase updatePhase, ref Pose targetPose, ref Vector3 localScale) {
        var interactor = grabInteractable.firstInteractorSelecting;
        var difference = interactor.transform.position - root.position;

        if(difference.magnitude < this.sphereRadius)
            return;

        targetPose = new Pose(this.root.position + difference.normalized * this.sphereRadius,targetPose.rotation);
    }
}