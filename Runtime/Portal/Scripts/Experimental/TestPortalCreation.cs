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

    [System.Serializable]
    public class Portal {
        [SerializeField]
        [Tooltip("The GameObject that resembles the display in the scenegraph")]
        public GameObject display;

        [SerializeField]
        [Tooltip("The GameObject that resembles the view in the scenegraph")]
        public GameObject view;

        public Material material;
        public RenderTexture leftRenderTexture;
        public RenderTexture rightRenderTexture;

        public void DeleteAllGameObjects() {
            GameObject.Destroy(this.display);
            GameObject.Destroy(this.view);
        }
    }

    public class TestPortalCreation : NetworkBehaviour {
        public GameObject portalPrefab;
        public GameObject displayPrefab;
        public GameObject viewPrefab;
        public Material materialToInstantiate;
        public Transform spawnPosition;
        public InputActionProperty buttonPress;

        [SerializeField]
        public List<Portal> registry;

        private int counter = 0;

        override public void OnNetworkSpawn() {
            this.buttonPress.action.performed += (ctx) => {
                var spawnMatrix = GetSpawnMatrix();
                this.CreatePortalServerRpc(spawnMatrix.GetPosition(), spawnMatrix.rotation);
            };
        }

        private void Update() {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb[Key.P].wasPressedThisFrame) {
                
                var spawnMatrix = GetSpawnMatrix();
                this.CreatePortalServerRpc(spawnMatrix.GetPosition(), spawnMatrix.rotation);
            }
        }

        public Matrix4x4 GetSpawnMatrix() {
            if(ConnectionManager.Instance.userSpawnInfo.userRole.Name == "HMD") {
                Debug.Log("Setting right hand as spawn postioon for HMD User");
                var avatar = NetworkUser.LocalInstance.avatarAnatomy as VRSYS.Photoportals.Avatar.AvatarHMDAnatomy;
                return avatar.rightHand.GetMatrix4x4();
            }

            if(ConnectionManager.Instance.userSpawnInfo.userRole.Name == "Desktop") {
                Debug.Log("Setting head as spawn postioon for Desktop User");
                var avatar = NetworkUser.LocalInstance.avatarAnatomy;
                //return avatar.head.GetMatrix4x4() * Matrix4x4.Translate(avatar.head.forward * 0.5f); //this is in world space but we need the translation it in local
                return Matrix4x4.TRS(avatar.head.position + (avatar.head.forward * 0.5f), avatar.head.rotation, Vector3.one);
            }
            return Matrix4x4.identity;
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
            Transform toggleClippingGO = display.transform.Find("UI Poke Components/Clipping Plane Toggle");
            Toggle toggleComponent = toggleClippingGO.GetComponentInChildren<Toggle>();
            if (toggleComponent == null) {
                Debug.LogWarning("No toggle component found for clipping plane toggle in portal UI.");
            }
            //TODO Decide on in-code or in-inspector assignment
            //toggleComponent.onValueChanged.AddListener((value) => {portalControl.SetNearClipPlane(value);});
            portalControl.EnableNearClipPlane();
            toggleComponent.isOn = true;

            Transform teleportGO = display.transform.Find("UI Poke Components/Teleport Button");
            Button teleportButtonComponent = teleportGO.GetComponentInChildren<Button>();
            if (teleportButtonComponent == null) {
                Debug.LogWarning("No teleportButtonComponent found for teleport button in portal UI.");
            }

            teleportButtonComponent.onClick.AddListener(() =>{

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

            /**
            Transform deleteGO = display.transform.Find("UI Poke Components/Delete Button");
            Button buttonComponent = deleteGO.GetComponentInChildren<Button>();
            buttonComponent.onClick.AddListener(
                () => {
                    ExtendedLogger.LogInfo(this.GetType().Name, "Deleting portal via UI button", this);
                    displayNetworkObject.Despawn();
                    viewNetworkObject.Despawn();
                    //GameObject.Destroy(display);
                    //GameObject.Destroy(view);
                    //this.registry.Remove(newPortal);
                }
            );
            **/

            /*          
            Transform anchoringGO = display.transform.Find("UI Poke Components/Anchoring Toggle");
            toggleComponent = anchoringGO.GetComponentInChildren<Toggle>();
            toggleComponent.onValueChanged.AddListener(
                (value) => {
                    Debug.Log("TODO: Toggle Anchoring");
                }
            );
            */

            /*
            Transform firstScaleOptionGO = display.transform.Find("UI Poke Components/Scale Option A Toggle");
            Transform secondScaleOptionGO = display.transform.Find("UI Poke Components/Scale Option B Toggle");
            Transform thirdScaleOptionGO = display.transform.Find("UI Poke Components/Scale Option C Toggle");

            Toggle toggleComponentOptionA = firstScaleOptionGO.GetComponentInChildren<Toggle>();
            Toggle toggleComponentOptionB = secondScaleOptionGO.GetComponentInChildren<Toggle>();
            Toggle toggleComponentOptionC = thirdScaleOptionGO.GetComponentInChildren<Toggle>();

            toggleComponentOptionA.onValueChanged.AddListener(
                (value) => {
                    Debug.Log("toggleComponentOptionA.onValueChanged");
                    if(value == true) {
                        toggleComponentOptionB.SetIsOnWithoutNotify(false);
                        toggleComponentOptionB.graphic.SetAllDirty();
                        toggleComponentOptionC.SetIsOnWithoutNotify(false);
                        toggleComponentOptionC.graphic.SetAllDirty();
                        portalControl.SetScale(1f);
                    }
                    if(value == false) {
                        toggleComponentOptionA.SetIsOnWithoutNotify(true);
                        toggleComponentOptionA.graphic.SetAllDirty();
                    }
                }
            );

            toggleComponentOptionB.onValueChanged.AddListener(
                (value) => {
                    Debug.Log("toggleComponentOptionB.onValueChanged");
                    if(value == true) {
                        toggleComponentOptionA.SetIsOnWithoutNotify(false);
                        toggleComponentOptionA.graphic.SetAllDirty();
                        toggleComponentOptionC.SetIsOnWithoutNotify(false);
                        toggleComponentOptionC.graphic.SetAllDirty();
                        portalControl.SetScale(10f);
                    }
                    if(value == false) {
                        toggleComponentOptionA.SetIsOnWithoutNotify(true);
                        toggleComponentOptionA.graphic.SetAllDirty();
                        portalControl.SetScale(1f);
                    }
                }
            );
            
            toggleComponentOptionC.onValueChanged.AddListener(
                (value) => {
                    Debug.Log("toggleComponentOptionC.onValueChanged");
                    if(value == true) {
                        toggleComponentOptionA.SetIsOnWithoutNotify(false);
                        toggleComponentOptionA.graphic.SetAllDirty();
                        toggleComponentOptionB.SetIsOnWithoutNotify(false);
                        toggleComponentOptionB.graphic.SetAllDirty();
                        portalControl.SetScale(500f);
                    }
                    if(value == false) {
                        toggleComponentOptionA.SetIsOnWithoutNotify(true);
                        toggleComponentOptionA.graphic.SetAllDirty();
                        portalControl.SetScale(1f);

                    }
                }
            );

            toggleComponentOptionA.isOn = true;
            */

            Debug.Log("Adding Listener to Scale Slider for portal scaling");
            Transform scaleGO = display.transform.Find("UI Poke Components/Scale Slider");
            Slider sliderComponent = scaleGO.GetComponentInChildren<Slider>();
            //viewNetworkObject.GetComponent<NetworkTransform>().
            if (sliderComponent == null) {
                Debug.LogWarning("No slider component found for scale adjustment in portal UI.");
            }

            sliderComponent?.onValueChanged.AddListener(
                (value) => {
                    switch (value) {
                        case 0f:
                            portalControl.SetScale(1f, false);
                            break;

                        case 1f:
                            portalControl.SetScale(10f, false);
                            break;

                        case 2f:
                            portalControl.SetScale(50f, false);
                            break;
                        
                        case 3f:
                            portalControl.SetScale(100f, false);                            
                            break;
                  
                        case 4f:
                            portalControl.SetScale(500f, false);
                            break;

                        default:
                            Debug.LogWarning("No scale value found for slider value.");
                            break;
                    }
                }
            );

            /**
            var simpleInteractable = display.GetComponentInChildren<XRSimpleInteractable>();
            grabInteractable.firstSelectEntered.AddListener(portalControl.SwitchToBimanualInteraction);
            grabInteractable.lastSelectExited.AddListener(portalControl.SwitchToUnimanualInteraction);

            GameObject viewInteractionZone = display.transform.Find("PortalViewInteractionZone").gameObject;
            grabInteractable.firstSelectEntered.AddListener(() => viewInteractionZone.SetActive(true));
            grabInteractable.lastSelectExited.AddListener(() => viewInteractionZone.SetActive(false));
            **/

            //TODO Teleport on Button Press
            /**
            Transform avatar = NetworkUser.LocalInstance.transform;
            Matrix4x4 relativeOffsetMatrix = this.selectedPortal.display.transform.GetMatrix4x4().inverse * avatar.GetMatrix4x4();
            Matrix4x4 absoluteWorldPositon = this.selectedPortal.view.transform.GetMatrix4x4() * relativeOffsetMatrix;
            avatar.position = absoluteWorldPositon.GetPosition();
            avatar.rotation = absoluteWorldPositon.rotation;
            **/

            ExtendedLogger.LogInfo(this.GetType().Name, "CreatePortal Done!", this);
        }

        [ContextMenu("CreatePortal")]
        private void CreatePortal() {
            var spawnMatrix = GetSpawnMatrix();
            this.CreatePortalServerRpc(spawnMatrix.GetPosition(), spawnMatrix.rotation);
        }

        [ServerRpc(RequireOwnership = false)]
        public void CreatePortalServerRpc(Vector3 position, Quaternion rotation) {
            ExtendedLogger.LogInfo(this.GetType().Name, "CreatePortal", this);
            
            Portal newPortal = new Portal();
            this.counter++;
            this.registry.Add(newPortal);

            newPortal.display = Instantiate(this.displayPrefab);
            newPortal.display.name = $"Portal #{this.counter} Display";
            newPortal.display.GetComponent<NetworkObject>().Spawn();

            newPortal.view = Instantiate(this.viewPrefab);
            newPortal.view.name = $"Portal #{this.counter} View";
            newPortal.view.GetComponent<NetworkObject>().Spawn();

            this.SetupPortalRpc(
                newPortal.display.GetComponent<NetworkObject>().NetworkObjectId,
                newPortal.view.GetComponent<NetworkObject>().NetworkObjectId
            );

            //spawn position
            
            //todo: distringuish between desktop and hmd user
        /* if(typeof(NetworkUser.LocalInstance.avatarAnatomy) is AvatarAnatomy) {
                this.spawnPosition = Transform.Zero;
            }

            if(typeof(NetworkUser.LocalInstance.avatarAnatomy) is AvatarHMDAnatomy)
            {
                var avatar = NetworkUser.LocalInstance.avatarAnatomy as AvatarHMDAnatomy;
                this.spawnPosition = avatar.rightHand;
            } */
            newPortal.display.transform.position = newPortal.view.transform.position = position;
            newPortal.display.transform.rotation = newPortal.view.transform.rotation = rotation;
            newPortal.view.transform.Translate(Vector3.forward * 0.01f, Space.Self);
        }

        public Portal GetPortalByDisplayName(string displayName) {
            foreach (Portal item in this.registry) {
                if (item.display.name != displayName)
                    continue;
                return item;
            }
            return null;
        }
        public Portal GetPortalByViewName(string viewName) {
            foreach (Portal item in this.registry) {
                if (item.view.name != viewName)
                    continue;
                return item;
            }
            return null;
        }
    }
}