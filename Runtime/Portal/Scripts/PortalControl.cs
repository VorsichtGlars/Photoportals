using System;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

using DG.Tweening;
using TMPro;

using VRSYS.Core.Logging;
using VRSYS.Core.Avatar;
using VRSYS.Core.Networking;

using VRSYS.Photoportals.Extensions;
using Unity.Netcode;
using Unity.XR.CoreUtils;
using System.Collections.Generic;


namespace VRSYS.Photoportals {
    public class PortalControl : MonoBehaviour {
        public Transform viewTransform;
        private XRGrabInteractable grabInteractable;
        private GameObject interaction_root;
        public GameObject interaction_coordinates_prefab;
        private bool isSelected {
            get {
                return this.grabInteractable.isSelected;
            }
        }
        private bool isSelectedWithLeftHand {
            get {
                return this.grabInteractable.firstInteractorSelecting?.handedness == InteractorHandedness.Left;
            }
        }
        private bool isSelectedWithRightHand {
            get {
                return this.grabInteractable.firstInteractorSelecting?.handedness == InteractorHandedness.Right;
            }
        }

        public InputActionProperty leftTriggerInput;
        public InputActionProperty rightTriggerInput;
        #region grabbing members

        private bool portalGrabIsActive = false;
        private Matrix4x4 offsetMatrix = Matrix4x4.identity;
        private Matrix4x4 clutchingOriginDisplay = Matrix4x4.identity;
        private Matrix4x4 clutchingOriginView = Matrix4x4.identity;

        private bool worldGrabIsActive = false;
        private Transform input;
        private Transform input_initial, input_current;
        private Transform display_initial, display_current;
        private Transform view_initial, view_current;
        private Matrix4x4 inital_display_to_controller_offset;
        private bool collisionAtScreenCenter;
        #endregion

        #region steering members
        [SerializeField]
        [Tooltip("Specify the mapping of trigger input to steering speed in meters per second.")]
        private AnimationCurve speedTransferFunction = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [SerializeField]
        [Tooltip("Specify the mapping of trigger input to steering speed in meters per second. Used for bimanual steering and joystick steering.")]
        private AnimationCurve bimanualSpeedTransferFunction = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [SerializeField]
        [Tooltip("Specify the mapping of joystick input to steering speed in meters per second. Used for joystick steering.")]
        private AnimationCurve joystickSpeedTransferFunction = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        #endregion

        #region scaling members
        public InputActionProperty scaleInput;
        public InputActionProperty scaleResetInput;
        [SerializeField]
        [Tooltip("Specify the mapping of joystick input to scaling speed in meters per second.")]
        private AnimationCurve scaleTransferFunction = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private Slider sliderElement;
        #endregion

        #region joystick steering memebers

        private GameObject joystick;
        private GameObject joystickRoot;
        private bool joystickIsSummoned = false;
        private bool joystickSteeringActive = false;
        private XRBaseInteractable joystickInteractable;
        private JoystickValues joystickValues;

        #endregion

        #region anchoring and locking members

        private bool rotationLock = true;
        private bool anchoringIsActive = false;

        #endregion

        #region Event Processing
        void Start() {
            this.grabInteractable = this.transform.GetComponent<XRGrabInteractable>();
            GameObject viewInteractionZone = this.transform.Find("Free Interaction Zone").gameObject;

            this.sliderElement = this.transform.GetComponentInChildren<Slider>();
            if(this.sliderElement == null) {
                ExtendedLogger.LogError(this.GetType().Name, "No slider element found in children.", this);
            }
            this.UpdateScaleUI();
            this.CreateInteractionHelpers();
            this.InitJoystickSteering();

            ColliderEvents colliderEvents = this.transform.GetComponentInChildren<ColliderEvents>();
            colliderEvents.OnEnter.AddListener(() => this.collisionAtScreenCenter = true);
            colliderEvents.OnExit.AddListener(() => this.collisionAtScreenCenter = false);

            this.grabInteractable.firstSelectEntered.AddListener(() => {
                if (this.collisionAtScreenCenter) {
                    this.UpdateComponentStatus("Starting portal grab triggered from screen grab");
                    this.portalGrabIsActive = true;
                    this.StartPortalGrab();
                }
                else{
                    viewInteractionZone.SetActive(true);
                }
            });

            this.grabInteractable.lastSelectExited.AddListener(() => {
                //TODO check if this reset is necessary and actually happening
                if (this.portalGrabIsActive) {
                    this.StopPortalGrab();
                }
                this.portalGrabIsActive = false;
                this.worldGrabIsActive = false;
                viewInteractionZone.SetActive(false);
            });

            var simpleInteractable = this.GetComponentInChildren<XRSimpleInteractable>(includeInactive: true);
            simpleInteractable.firstSelectEntered.AddListener((args) => {
                this.UpdateComponentStatus("InteractionZoneSelectEnter triggered, switching to bimanual interaction");
                this.input = args.interactorObject.transform;
                this.SetupBimanualInteractionHelpers();
                this.worldGrabIsActive = true;
            });

            simpleInteractable.lastSelectExited.AddListener((args) => {
                this.UpdateComponentStatus("InteractionZoneSelectExit triggered, switching to unimanual interaction");
                this.worldGrabIsActive = false;
                this.inital_display_to_controller_offset = Matrix4x4.identity;
            });

            simpleInteractable.activated.AddListener((args) => {
                this.joystickIsSummoned = !this.joystickIsSummoned;

                if(this.joystickIsSummoned == true){
                    this.joystick.transform.DOFollowTransform(args.interactorObject.transform, 0.25f).
                    OnComplete(() => {
                        this.joystickInteractable.enabled = true;
                    });
                }

                if(this.joystickIsSummoned == false){
                    this.joystick.transform.DOFollowTransform(this.joystickRoot.transform, 0.25f).
                    OnStart(() => {
                        this.joystickInteractable.enabled = false;
                    });
                }
            });

            //register UI
            Transform toggleClippingGO = this.transform.Find("Poke Interactions Canvas/Clipping Plane Toggle");
            Toggle toggleClippingComponent = toggleClippingGO.GetComponentInChildren<Toggle>();
            if (toggleClippingComponent == null)
                Debug.LogWarning("No toggle component found for clipping plane toggle in portal UI.");
            toggleClippingComponent.onValueChanged.AddListener((value) => {this.SetNearClipPlane(value);});
            this.EnableNearClipPlane();
            toggleClippingComponent.isOn = true;

            Transform teleportGO = this.transform.Find("Poke Interactions Canvas/Teleport Button");
            Button teleportButtonComponent = teleportGO.GetComponentInChildren<Button>();
            if (teleportButtonComponent == null)
                Debug.LogWarning("No teleportButtonComponent found for teleport button in portal UI.");
            teleportButtonComponent.onClick.AddListener(() =>{
                void teleport() {
                    this.EnableNearClipPlane();
                    Transform avatar = NetworkUser.LocalInstance.transform;
                    Matrix4x4 relativeOffsetMatrix = this.transform.GetMatrix4x4().inverse * avatar.GetMatrix4x4();
                    Matrix4x4 absoluteWorldPositon = this.viewTransform.GetMatrix4x4() * relativeOffsetMatrix;
                    avatar.transform.SetMatrix4x4(absoluteWorldPositon);
                    this.transform.SetMatrix4x4(this.viewTransform.GetMatrix4x4());
                    this.viewTransform.Translate(Vector3.forward * 0.01f, Space.Self);
                }

                if(this.viewTransform.localScale.x == 1.0f) {
                    teleport();
                }
                else {
                    this.viewTransform.DOScale(1f, 1f).
                    OnUpdate(() => this.UpdateScaleUI()).
                    OnComplete(() => teleport());
                }
            });

            Transform anchoringGO = this.transform.Find("Poke Interactions Canvas/Anchoring Toggle");
            Toggle toggleAnchoringComponent = anchoringGO.GetComponentInChildren<Toggle>();
            if (toggleAnchoringComponent == null)
                Debug.LogWarning("No toggle component found for anchoring toggle in portal UI.");
            toggleAnchoringComponent.onValueChanged.AddListener((value) => this.SetRotationLock(value));
            toggleAnchoringComponent.isOn = false;
            this.SetRotationLock(false);

            Transform scaleGO = this.transform.Find("Poke Interactions Canvas/Scale Slider");
            Slider sliderComponent = scaleGO.GetComponentInChildren<Slider>();
            if (sliderComponent == null)
                Debug.LogWarning("No slider component found for scale adjustment in portal UI.");
            sliderComponent?.onValueChanged.AddListener(value => {
                float targetScale = value switch {
                    0f => 1f,
                    1f => 10f,
                    2f => 50f,
                    3f => 100f,
                    4f => 500f,
                    _ => 1f
                };
                this.viewTransform.DOScale(targetScale, 1f);
            });
        }

        void Update() {
            //evaluate input

            //one handed controller view grab init (needs to be before steering as steering uses variables here)
            //TODO
            //1) handle the event where one releases the grab button but still holds the clutch input button
            //currently this would lead to the situation that StopPortalGrab is not being called
            //one could think of going from one to two bool states: portalGrabIsActive -> leftPortalGrabIsActive, rightPortalGrabIsActive
            //2) also, what if both are pressed in this frame?
            float leftTriggerValue = (float)this.leftTriggerInput.action?.ReadValue<float>();
            float rightTriggerValue = (float)this.rightTriggerInput.action?.ReadValue<float>();
            leftTriggerValue = (float)Math.Round(leftTriggerValue, 2);
            rightTriggerValue = (float)Math.Round(rightTriggerValue, 2);

            if (this.worldGrabIsActive == false && this.portalGrabIsActive == false && leftTriggerValue > 0.1f && this.isSelectedWithLeftHand) {
                this.UpdateComponentStatus("Starting portal grab with left hand");
                this.portalGrabIsActive = true;
                this.StartPortalGrab();
            }

            if (this.portalGrabIsActive == true && leftTriggerValue < 0.1f && this.isSelectedWithLeftHand && this.collisionAtScreenCenter == false) {
                this.UpdateComponentStatus("Stopping portal grab with left hand");
                this.portalGrabIsActive = false;
                this.StopPortalGrab();
            }

            if (this.worldGrabIsActive == false && this.portalGrabIsActive == false && rightTriggerValue > 0.1f && this.isSelectedWithRightHand) {
                this.UpdateComponentStatus("Starting portal grab with right hand");
                this.portalGrabIsActive = true;
                this.StartPortalGrab();
            }

            if (this.portalGrabIsActive == true && rightTriggerValue < 0.1f && this.isSelectedWithRightHand && this.collisionAtScreenCenter == false) {
                this.UpdateComponentStatus("Stopping portal grab with right hand");
                this.portalGrabIsActive = false;
                this.StopPortalGrab();
            }

            //unimanual action
            //one handed view steering
            if (this.portalGrabIsActive == true && leftTriggerValue > 0.1f && this.isSelectedWithLeftHand) {
                this.UpdateComponentStatus("Performing unimanual left handed controller based steering");
                var oneHandedSteeringValue = this.speedTransferFunction.Evaluate(leftTriggerValue);
                oneHandedSteeringValue *= Time.deltaTime;
                this.clutchingOriginView = Matrix4x4.Translate(this.viewTransform.forward * oneHandedSteeringValue) * this.clutchingOriginView;
            }
            
            if (this.portalGrabIsActive == true && rightTriggerValue > 0.1f && this.isSelectedWithRightHand) {
                this.UpdateComponentStatus("Performing unimanual right handed controller based steering");
                var oneHandedSteeringValue = this.speedTransferFunction.Evaluate(rightTriggerValue);
                oneHandedSteeringValue *= Time.deltaTime;
                this.clutchingOriginView = Matrix4x4.Translate(this.viewTransform.forward * oneHandedSteeringValue) * this.clutchingOriginView;
            }

            //bimanual action
            //TODO encapsulate logic and then call for left and right hand

            if (this.isSelectedWithLeftHand == true && rightTriggerValue > 0.1f){
                this.UpdateComponentStatus("Performing bimanual controller based steering with right hand");
                //var avatar = NetworkUser.LocalInstance.avatarAnatomy as Photoportals.Avatar.AvatarHMDAnatomy;
                var avatar = NetworkUser.LocalInstance.avatarAnatomy as VRSYS.Core.Avatar.AvatarHMDAnatomy;
                var localRepr = this.transform.GetMatrix4x4().inverse * avatar.rightHand.transform.forward;
                var viewRepr = this.viewTransform.GetMatrix4x4() * localRepr;
                var rightSteeringValue = (float)this.rightTriggerInput.action?.ReadValue<float>();
                rightSteeringValue = this.bimanualSpeedTransferFunction.Evaluate(rightSteeringValue);
                rightSteeringValue *= Time.deltaTime;
                this.ApplySteeringVector(viewRepr * rightSteeringValue, Space.Self);
            }

            if ( this.isSelectedWithRightHand == true && leftTriggerValue > 0.1f){
                this.UpdateComponentStatus("Performing bimanual controller based steering with left hand");
                //var avatar = NetworkUser.LocalInstance.avatarAnatomy as Photoportals.Avatar.AvatarHMDAnatomy;
                var avatar = NetworkUser.LocalInstance.avatarAnatomy as VRSYS.Core.Avatar.AvatarHMDAnatomy;

                var localRepr = this.transform.GetMatrix4x4().inverse * avatar.leftHand.transform.forward;
                var viewRepr = this.viewTransform.GetMatrix4x4() * localRepr;
                var leftSteeringValue = (float)this.leftTriggerInput.action?.ReadValue<float>();
                leftSteeringValue = this.bimanualSpeedTransferFunction.Evaluate(leftSteeringValue);
                leftSteeringValue *= Time.deltaTime;
                this.ApplySteeringVector(viewRepr * leftSteeringValue, Space.Self);
            }

            if (this.isSelected && this.joystickSteeringActive == true) {
                this.UpdateComponentStatus("Performing joystick based steering");
                var joystickDirection = this.joystickValues.Direction();

                var localRepr = this.transform.GetMatrix4x4().inverse * joystickDirection;
                var viewRepr = this.viewTransform.GetMatrix4x4() * localRepr;
                var steeringValue = this.joystickSpeedTransferFunction.Evaluate(this.joystickValues.Magnitude());
                steeringValue *= Time.deltaTime;
                this.ApplySteeringVector(viewRepr * steeringValue, Space.Self);
            }

            //debug log
            if (this.portalGrabIsActive == true && this.worldGrabIsActive == true) {
                this.UpdateComponentStatus("Both grab modes active, this shouldn't happen.");
            }

            //one handed controller view grab frame update
            if (this.portalGrabIsActive == true) {
                this.UpdateComponentStatus("Updating portal grab");
                this.UpdatePortalGrab();
            }

            //two handed controller/hand world grab (triggered by two selections one on portal display, one on portal view)
            if (this.worldGrabIsActive == true) {
                this.UpdateComponentStatus("Updating world grab");
                this.UpdateWorldGrab();
            }

            //scaling options via controller (both inputs are allowed simultaneously)
            if (this.isSelected && this.scaleInput.action?.WasPerformedThisFrame() == true) {
                this.UpdateComponentStatus("Scaling portal view with controller input");
                var scalingValue = this.scaleTransferFunction.Evaluate(this.scaleInput.action.ReadValue<Vector2>().y);
                scalingValue *= Time.deltaTime;

                if ((this.viewTransform.localScale.x + scalingValue) < 1.0f) {
                    this.viewTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                }
                else {
                    this.viewTransform.localScale += new Vector3(scalingValue, scalingValue, scalingValue);
                }
                this.UpdateScaleUI();
            }

            if (this.isSelected && this.scaleResetInput.action?.WasPerformedThisFrame() == true) {
                this.UpdateComponentStatus("Resetting portal view scale with controller input");
                this.viewTransform.DOScale(1.0f, 2.0f)
                .OnStart(() => this.UpdateComponentStatus($"Scaling to 1.0"))
                .OnUpdate(() => this.UpdateScaleUI())
                .OnComplete(() => this.UpdateComponentStatus($"Finished Scaling to 1.0"));
            }

            if(this.rotationLock == true) {
                Vector3 currentRotation = this.viewTransform.rotation.eulerAngles;
                Vector3 constrainedRotation = new Vector3(0f, currentRotation.y, 0f);
                this.viewTransform.rotation = Quaternion.Euler(constrainedRotation);
            }
        }
        #endregion

        #region Editor Stuff
        private void UpdateComponentStatus(string message) {
            //TODO make this a separete component and optimize it for performance
            return;
        }

        #endregion

        #region Scaling
        private void UpdateScaleUI() {
            float mappedScaleValue = this.viewTransform.lossyScale.x switch {
                >= 500f  => 4f,
                >= 100f and < 500f => 3f,
                >= 50f and < 100f => 2f,
                >= 10f and < 50f => 1f,
                _ => 0f
            };
            this.sliderElement.SetValueWithoutNotify(mappedScaleValue);
        }
        #endregion

        #region PortalGrab
        private void StartPortalGrab() {
            this.clutchingOriginDisplay = this.grabInteractable.transform.GetMatrix4x4();
            this.clutchingOriginView = this.viewTransform.GetMatrix4x4();
        }

        private void UpdatePortalGrab() {
            if (this.clutchingOriginDisplay == Matrix4x4.identity) {
                ExtendedLogger.LogError(this.GetType().Name, "Matrix should not be identity.", this);
                return;
            }

            //calculate relative offset
            this.offsetMatrix = this.grabInteractable.transform.GetMatrix4x4().inverse * this.clutchingOriginDisplay;

            this.offsetMatrix = this.offsetMatrix.inverse;

            //calculate absolute offset
            this.offsetMatrix = this.clutchingOriginView * this.offsetMatrix;

            //apply offset
            this.viewTransform.position = this.offsetMatrix.GetColumn(3);
            this.viewTransform.rotation = this.offsetMatrix.rotation;
        }

        private void StopPortalGrab() {
            this.clutchingOriginDisplay = Matrix4x4.identity;
            this.offsetMatrix = Matrix4x4.identity;
        }

        #endregion

        #region WorldGrab

        private void CreateInteractionHelpers() {
            this.interaction_root = Instantiate(this.interaction_coordinates_prefab);
            this.interaction_root.name = "interaction_gizmos for " + this.gameObject.name;
            this.interaction_root.transform.SetParent(GameObject.Find("Portal Manager").transform);
            this.input_initial = this.interaction_root.transform.Find("input_initial").transform;
            this.input_current = this.interaction_root.transform.Find("input_current").transform;
            this.display_initial = this.interaction_root.transform.Find("display_initial").transform;
            this.display_current = this.interaction_root.transform.Find("display_current").transform;
            this.view_initial = this.interaction_root.transform.Find("view_initial").transform;
            this.view_current = this.interaction_root.transform.Find("view_current").transform;
        }
        private void SetupBimanualInteractionHelpers() {
            this.display_initial.position = this.grabInteractable.transform.position;
            this.display_initial.rotation = this.grabInteractable.transform.rotation;
            this.view_initial.position = this.viewTransform.position;
            this.view_initial.rotation = this.viewTransform.rotation;
            this.input_initial.position = this.input.position;
            this.input_initial.rotation = this.input.rotation;
            this.inital_display_to_controller_offset = this.display_initial.GetMatrix4x4().inverse * this.input_initial.GetMatrix4x4();
        }

        public void UpdateWorldGrab() {
            this.input_current.position = this.input.position;
            this.input_current.rotation = this.input.rotation;
            this.display_current.position = this.transform.position;
            this.display_current.rotation = this.transform.rotation;

            //calculate relative offset
            Matrix4x4 current_display_to_controller_offset = this.display_current.GetMatrix4x4().inverse * this.input_current.GetMatrix4x4();
            current_display_to_controller_offset = current_display_to_controller_offset.inverse;

            Matrix4x4 test = this.view_initial.GetMatrix4x4() * Matrix4x4.Scale(this.viewTransform.lossyScale) * this.inital_display_to_controller_offset * current_display_to_controller_offset;

            //apply offset
            this.view_current.position = test.GetPosition();
            this.view_current.rotation = test.rotation;
            this.viewTransform.position = this.view_current.position;
            this.viewTransform.rotation = this.view_current.rotation;
        }

        #endregion

        #region Steering
        public void ApplySteeringVector(Vector3 vector, Space space) {
            if(space == Space.World)
                this.viewTransform.position += this.viewTransform.TransformVector(vector);

            if(space == Space.Self)
                this.viewTransform.position += vector;
        }

        #endregion

        #region Rendering

        [ContextMenu("Enable Near Clip")]
        public void EnableNearClipPlane() {
            this.SetNearClipPlane(true);   
        }

        [ContextMenu("Disable Near Clip")]
        public void DisableNearClipPlane() {
            this.SetNearClipPlane(false);   
        }

        public void SetNearClipPlane(bool value) {
            var offAxisProjections = this.viewTransform.gameObject.GetComponentsInChildren<OffAxisProjection>();
            foreach (var oap in offAxisProjections) {
                oap.SetNearClipPlane(value);
            }
        }
        #endregion


        #region Creation and Deletion
        [ContextMenu("Despawn")]
        public void Despawn() {
            this.UpdateComponentStatus($"Despawning Portal {this.name}");
            this.viewTransform.GetComponent<NetworkObject>().Despawn();
            this.GetComponent<NetworkObject>().Despawn();
            Destroy(this.interaction_root);
        }

        #endregion

        #region Rotation Lock
        public void SetRotationLock(bool value) {
            this.rotationLock = value;
        }
        #endregion

        #region Joystick Steering
        //TODOs
        // - register activate event in code
        // - distribute active state of XRBaseInteractable
        private void InitJoystickSteering() {
            this.joystick = this.transform.Find("Joystick").gameObject;
            if(this.joystick == null) {
                ExtendedLogger.LogError(this.GetType().Name, "No joystick found as child of portal.", this);
            }
            this.joystickRoot = this.transform.Find("JoystickRoot").gameObject;
            if(this.joystickRoot == null) {
                ExtendedLogger.LogError(this.GetType().Name, "No joystick root found as child of portal.", this);
            }
            this.joystickInteractable = this.joystick.GetComponentInChildren<XRBaseInteractable>();
            this.joystickInteractable.enabled = this.joystickIsSummoned;
            this.joystickValues = this.joystick.GetComponent<JoystickValues>();
            this.joystickValues.OnJoystickGrabbed.AddListener(()=>{
                this.UpdateComponentStatus("Joystick Grabbed");
                this.joystickSteeringActive = true;
            });
            this.joystickValues.OnJoystickReleased.AddListener(()=>{
                this.UpdateComponentStatus("Joystick Released");
                this.joystickSteeringActive = false;
            });
        }
        #endregion
    }
}