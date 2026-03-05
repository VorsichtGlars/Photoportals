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

        [Header("HMD Input Actions for Portal Creation")]
        public InputActionProperty buttonPressRight;

        public InputActionProperty buttonPressLeft;

        public InputActionProperty doubleTapRight;

        public InputActionProperty doubleTapLeft;
        
        [Header("Desktop Input Actions for Portal Creation")]
        public InputActionProperty desktopButtonPress;

        [System.Serializable]
        struct PortalRegistryEntry {
            [SerializeField]
            [Tooltip("The GameObject that resembles the display in the scenegraph")]
            public GameObject display;

            [SerializeField]
            [Tooltip("The GameObject that resembles the view in the scenegraph")]
            public GameObject view;
        }
        
        [SerializeField]
        private List<PortalRegistryEntry> registry;
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
        }

        public void OnRemoteNetworkUserSetup(NetworkUser user) {
            //throw new System.NotImplementedException();
        }
        #endregion
        
        #region Portal Creation Methods
        [ContextMenu("CreatePortal")]
        private void CreatePortal() {
            this.CreatePortalServerRpc(Vector3.zero, Quaternion.identity);
        }

        [ServerRpc(RequireOwnership = false)]
        public void CreatePortalServerRpc(Vector3 position, Quaternion rotation) {
            ExtendedLogger.LogInfo(this.GetType().Name, "CreatePortal", this);
            
            PortalRegistryEntry entry = new PortalRegistryEntry();
            entry.display = Instantiate(this.displayPrefab);
            entry.display.name = $"Portal #{this.registry.Count} Display";
            entry.display.GetComponent<NetworkObject>().Spawn();

            entry.view = Instantiate(this.viewPrefab);
            entry.view.name = $"Portal #{this.registry.Count} View";
            entry.view.GetComponent<NetworkObject>().Spawn();

            this.SetupPortalRpc(
                entry.display.GetComponent<NetworkObject>().NetworkObjectId,
                entry.view.GetComponent<NetworkObject>().NetworkObjectId
            );

            entry.display.transform.position = entry.view.transform.position = position;
            entry.display.transform.rotation = entry.view.transform.rotation = rotation;
            entry.view.transform.Translate(Vector3.forward * 0.01f, Space.Self);

            this.registry.Add(entry);
        }

        [Rpc(SendTo.Everyone, RequireOwnership = false)]
        public void SetupPortalRpc(ulong displayNetworkObjectId, ulong viewNetworkObjectId) {
            var displayNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[displayNetworkObjectId];
            var viewNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[viewNetworkObjectId];
            GameObject display = displayNetworkObject.gameObject;
            GameObject view = viewNetworkObject.gameObject;

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
            

            //setting up interaction stuff
            PortalControl portalControl = display.GetComponent<PortalControl>();
            portalControl.viewTransform = view.transform;

            var displayOwnershipManager = display.GetComponent<OwnershipManager>();
            var viewOwnershipManager = view.GetComponent<OwnershipManager>();
            var grabInteractable = display.GetComponent<XRGrabInteractable>();
            grabInteractable.firstSelectEntered.AddListener(displayOwnershipManager.RequestOwnershipFromThisClient);
            grabInteractable.firstSelectEntered.AddListener(viewOwnershipManager.RequestOwnershipFromThisClient);
            grabInteractable.lastSelectExited.AddListener(displayOwnershipManager.ReturnOwnershipToServer);
            grabInteractable.lastSelectExited.AddListener(viewOwnershipManager.ReturnOwnershipToServer);

            // tweak initial configuration of view so it the portal display is not dark
            view.transform.Translate(view.transform.forward * 0.1f, Space.World);
            var offAxisProjections = view.GetComponentsInChildren<OffAxisProjection>();
            
            //register UI
            Transform toggleClippingGO = display.transform.Find("Poke Interactions Canvas/Clipping Plane Toggle");
            Toggle toggleComponent = toggleClippingGO.GetComponentInChildren<Toggle>();
            if (toggleComponent == null) {
                Debug.LogWarning("No toggle component found for clipping plane toggle in portal UI.");
            }
            //TODO Decide on in-code or in-inspector assignment
            //toggleComponent.onValueChanged.AddListener((value) => {portalControl.SetNearClipPlane(value);});
            portalControl.EnableNearClipPlane();
            toggleComponent.isOn = true;

            Transform teleportGO = display.transform.Find("Poke Interactions Canvas/Teleport Button");
            Button teleportButtonComponent = teleportGO.GetComponentInChildren<Button>();
            if (teleportButtonComponent == null) {
                Debug.LogWarning("No teleportButtonComponent found for teleport button in portal UI.");
            }

            teleportButtonComponent.onClick.AddListener(() =>{
                //TODO encapsulate this into portalcontrol
                void teleport() {
                    portalControl.EnableNearClipPlane();
                    Transform avatar = NetworkUser.LocalInstance.transform;
                    Matrix4x4 relativeOffsetMatrix = display.transform.GetMatrix4x4().inverse * avatar.GetMatrix4x4();
                    Matrix4x4 absoluteWorldPositon = view.transform.GetMatrix4x4() * relativeOffsetMatrix;
                    avatar.transform.SetMatrix4x4(absoluteWorldPositon);
                    //avatar.position = absoluteWorldPositon.GetPosition();
                    //avatar.rotation = absoluteWorldPositon.rotation;
                    display.transform.SetMatrix4x4(view.transform.GetMatrix4x4());
                    view.transform.Translate(Vector3.forward * 0.01f, Space.Self);
                }

                if(view.transform.localScale.x == 1.0f) {
                    teleport();
                }
                else {
                    portalControl.SetScale(1f, 1f, teleport);
                }
            });

            Transform anchoringGO = display.transform.Find("Poke Interactions Canvas/Anchoring Toggle");
            toggleComponent = anchoringGO.GetComponentInChildren<Toggle>();
            toggleComponent.onValueChanged.AddListener(
                (value) => {
                    Debug.Log("TODO: Toggle Anchoring");
                }
            );

            Debug.Log("Adding Listener to Scale Slider for portal scaling");
            Transform scaleGO = display.transform.Find("Poke Interactions Canvas/Scale Slider");
            Slider sliderComponent = scaleGO.GetComponentInChildren<Slider>();
            if (sliderComponent == null) {
                Debug.LogWarning("No slider component found for scale adjustment in portal UI.");
            }

            sliderComponent?.onValueChanged.AddListener(value => {
                float targetScale = value switch {
                    0f => 1f,
                    1f => 10f,
                    2f => 50f,
                    3f => 100f,
                    4f => 500f,
                    _ => 1f
                };
                portalControl.SetScale(targetScale, false);
            });

            /**
            //TODO Decide wether to have this as code or as inspector assignment
            var simpleInteractable = display.GetComponentInChildren<XRSimpleInteractable>();
            grabInteractable.firstSelectEntered.AddListener(portalControl.SwitchToBimanualInteraction);
            grabInteractable.lastSelectExited.AddListener(portalControl.SwitchToUnimanualInteraction);

            GameObject viewInteractionZone = display.transform.Find("PortalViewInteractionZone").gameObject;
            grabInteractable.firstSelectEntered.AddListener(() => viewInteractionZone.SetActive(true));
            grabInteractable.lastSelectExited.AddListener(() => viewInteractionZone.SetActive(false));
            **/

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