using System.Collections.Generic;

using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

using VRSYS.Core.Logging;
using VRSYS.Core.Networking;

using VRSYS.Photoportals;
using VRSYS.Photoportals.Extensions;
using VRSYS.Photoportals.Networking;
using DG.Tweening;

namespace VRSYS.Photoportals {
    public class PortalManager : NetworkBehaviour, INetworkUserCallbacks  {
        #region Portal Creation Members
        [Header("References for Portal Creation")]
        public GameObject displayPrefab;
        public GameObject viewPrefab;
        public Material materialToInstantiate;
        private int portalCount = 0;

        [Header("HMD Input Actions for Portal Creation")]
        public InputActionProperty buttonPressRight;
        public InputActionProperty buttonPressLeft;
        public InputActionProperty doubleTapRight;
        public InputActionProperty doubleTapLeft;
        
        [Header("Desktop Input Actions for Portal Creation")]
        public InputActionProperty desktopButtonPress;

        #endregion

        #region Debugging Utilities Members
        [Header("Debugging Utilities")]
        [SerializeField]
        List<RenderStateController> widgetsAndGizmosRenderStateControllers;
        #endregion

        #region State Handling
        public void OnLocalNetworkUserSetup() {

            if(NetworkUser.LocalInstance.avatarAnatomy is VRSYS.Core.Avatar.AvatarHMDAnatomy) {
                var avatar = NetworkUser.LocalInstance.avatarAnatomy as VRSYS.Core.Avatar.AvatarHMDAnatomy;

                if (this.buttonPressRight.action != null)
                    this.buttonPressRight.action.performed += (ctx) => this.CreatePortalServerRpc(avatar.rightHand.position, avatar.rightHand.rotation);

                if (this.buttonPressLeft.action != null)
                    this.buttonPressLeft.action.performed += (ctx) => this.CreatePortalServerRpc(avatar.leftHand.position, avatar.leftHand.rotation);

                if(this.doubleTapRight.action != null)
                    this.doubleTapRight.action.performed += (ctx) => this.CreatePortalServerRpc(avatar.rightHand.position, avatar.rightHand.rotation);

                if(this.doubleTapLeft.action != null)
                    this.doubleTapLeft.action.performed += (ctx) => this.CreatePortalServerRpc(avatar.leftHand.position, avatar.leftHand.rotation);
            
            }
            else if(NetworkUser.LocalInstance.avatarAnatomy is VRSYS.Core.Avatar.AvatarAnatomy) {
                var avatar = NetworkUser.LocalInstance.avatarAnatomy as VRSYS.Core.Avatar.AvatarAnatomy;

                if(this.desktopButtonPress.action != null) {
                    this.desktopButtonPress.action.performed += (ctx) => {
                        Matrix4x4 spawnMatrix = Matrix4x4.TRS(avatar.head.position + (avatar.head.forward * 0.5f), avatar.head.rotation, Vector3.one);
                        this.CreatePortalServerRpc(spawnMatrix.GetPosition(), spawnMatrix.rotation);
                    };    
                }
                
            }

            // set each camera to render everything, then exclude CameraIgnore
            // this can conflict with other culling mask configurations
            int cameraIgnoreLayer = LayerMask.NameToLayer("CameraIgnore");
            if (cameraIgnoreLayer == -1) {
                Debug.LogWarning("Layer 'CameraIgnore' was not found.");
            }
            else {
                foreach (var camera in Camera.allCameras) {
                    camera.cullingMask = ~0;
                    camera.cullingMask &= ~(1 << cameraIgnoreLayer);
                }
            }
        }

        public void OnRemoteNetworkUserSetup(NetworkUser user) {
            //throw new System.NotImplementedException();
        }
        #endregion
        
        #region Portal Creation Methods
        [ContextMenu("CreatePortal")]
        private void CreatePortalContextMenuCall() {
            this.CreatePortalServerRpc(Vector3.zero, Quaternion.identity);
        }

        [ServerRpc(RequireOwnership = false)]
        public void CreatePortalServerRpc(Vector3 position, Quaternion rotation) {
            ExtendedLogger.LogInfo(this.GetType().Name, "CreatePortal", this);
            
            GameObject display = Instantiate(this.displayPrefab);
            display.name = $"Portal #{this.portalCount} Display";
            display.GetComponent<NetworkObject>().Spawn();

            GameObject view = Instantiate(this.viewPrefab);
            view.name = $"Portal #{this.portalCount} View";
            view.GetComponent<NetworkObject>().Spawn();

            this.SetupPortalRpc(
                display.GetComponent<NetworkObject>().NetworkObjectId,
                view.GetComponent<NetworkObject>().NetworkObjectId
            );

            display.transform.position = view.transform.position = position;
            display.transform.rotation = view.transform.rotation = rotation;

            //creating some offset between both so the camera does not render the display
            view.transform.Translate(Vector3.forward * 0.01f, Space.Self);

            this.portalCount++;
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        public void SetupPortalRpc(ulong displayNetworkObjectId, ulong viewNetworkObjectId) {
            var displayNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[displayNetworkObjectId];
            var viewNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[viewNetworkObjectId];
            GameObject display = displayNetworkObject.gameObject;
            GameObject view = viewNetworkObject.gameObject;
            PortalControl portalControl = display.GetComponent<PortalControl>();
            portalControl.viewTransform = view.transform;

            //setting up rendering stuff
            Material material = Material.Instantiate(this.materialToInstantiate);
            RenderTexture leftRenderTexture = new RenderTexture(1024, 1024, 16, RenderTextureFormat.ARGB32);
            RenderTexture rightRenderTexture = new RenderTexture(1024, 1024, 16, RenderTextureFormat.ARGB32);
            leftRenderTexture.Create();
            rightRenderTexture.Create();
            var block = new MaterialPropertyBlock();
            block.SetTexture("_LeftEyeTexture", leftRenderTexture);
            block.SetTexture("_RightEyeTexture", rightRenderTexture);
            var quad = display.transform.Find("Stereo Display");
            var renderer = quad.GetComponent<MeshRenderer>();
            renderer.SetPropertyBlock(block);

            view.transform.Find("Cameras/LeftCamera").GetComponent<Camera>().targetTexture = leftRenderTexture;
            view.transform.Find("Cameras/RightCamera").GetComponent<Camera>().targetTexture = rightRenderTexture;

            //setting up head tracking
            var tracking = display.GetComponent<PortalExitHeadTracking>();
            tracking.portalEntranceScreen = quad;
            tracking.portalExitScreen = view.transform.Find("StereoDisplayProxy");
            tracking.portalExitHead = view.transform.Find("Cameras");
            tracking.viewRoot = view.transform;
            tracking.portalEntranceHead = NetworkUser.LocalInstance.avatarAnatomy.head;
            
            //setting up ownership transfer
            var displayOwnershipManager = display.GetComponent<OwnershipManager>();
            var viewOwnershipManager = view.GetComponent<OwnershipManager>();
            var grabInteractable = display.GetComponent<XRGrabInteractable>();
            grabInteractable.firstSelectEntered.AddListener(displayOwnershipManager.RequestOwnershipFromThisClient);
            grabInteractable.firstSelectEntered.AddListener(viewOwnershipManager.RequestOwnershipFromThisClient);
            //maybe this is better as e.g. dotween scaling actions can still run
            //grabInteractable.lastSelectExited.AddListener(displayOwnershipManager.ReturnOwnershipToServer);
            //grabInteractable.lastSelectExited.AddListener(viewOwnershipManager.ReturnOwnershipToServer);

            ExtendedLogger.LogInfo(this.GetType().Name, "CreatePortal Done!", this);
        }
        #endregion

        #region Debugging Utilities Methods
        [ContextMenu("RetrieveAllWidgetsAndGizmos()")]
        private void RetrieveAllWidgetsAndGizmos() {
            foreach (var item in GameObject.FindGameObjectsWithTag("3DUI")) {
                var controller = item.GetComponent<RenderStateController>();
                if (controller == null)
                    return;
                this.widgetsAndGizmosRenderStateControllers.Add(controller);
            }
        }

        [ContextMenu("EnableAllWidgetsAndGizmos()")]
        private void EnableAllWidgetsAndGizmos() {
            foreach (var controller in this.widgetsAndGizmosRenderStateControllers) {
                controller.Activate();
            }
        }

        [ContextMenu("DisableAllWidgetsAndGizmos()")]
        private void DisableAllWidgetsAndGizmos() {
            foreach (var controller in this.widgetsAndGizmosRenderStateControllers) {
                controller.Deactivate();
            }
        }
        #endregion
    }
}