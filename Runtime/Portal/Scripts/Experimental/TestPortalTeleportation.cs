using UnityEngine;
using VRSYS.Core.Logging;
using VRSYS.Core.Networking;
using VRSYS.Photoportals.Extensions;
using VRSYS.Photoportals;


[RequireComponent(typeof(TestPortalCreation))]
public class TestPortalTeleportation : MonoBehaviour {

    [SerializeField]
    private Portal selectedPortal;

    [SerializeField]
    private GameObject selectedGO;

    #region Methods

    [ContextMenu("TeleportToSelectedPortalView")]
    public void TeleportToSelectedPortalView() {
        if(this.selectedPortal == null) return;
        ExtendedLogger.LogInfo(this.GetType().Name, "Teleporting to view " + this.selectedPortal.view.name);
        Transform avatar = NetworkUser.LocalInstance.transform;
        Matrix4x4 relativeOffsetMatrix = this.selectedPortal.display.transform.GetMatrix4x4().inverse * avatar.GetMatrix4x4();
        Matrix4x4 absoluteWorldPositon = this.selectedPortal.view.transform.GetMatrix4x4() * relativeOffsetMatrix;
        avatar.position = absoluteWorldPositon.GetPosition();
        avatar.rotation = absoluteWorldPositon.rotation;
        this.selectedPortal.display.transform.SetMatrix4x4(this.selectedPortal.view.transform.GetMatrix4x4());
        this.selectedPortal.view.transform.Translate(this.selectedPortal.view.transform.forward * 0.01f, Space.Self);
    }
    #endregion
}