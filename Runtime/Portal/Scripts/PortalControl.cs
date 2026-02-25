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

namespace VRSYS.Photoportals {
    public class PortalControl : MonoBehaviour {
        [Header("Component Status")]
        [SerializeField]
        private string currentStatusMessage;

        [SerializeField]
        public InteractionState interactionState;

        [Header("General Properties")]
        public Transform viewTransform;
        public XRGrabInteractable grabInteractable;
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

        #region clutching members
        [Header("Clutching Properties")]
        public InputActionProperty clutchInput;
        public InputActionProperty leftClutchInput;
        public InputActionProperty rightClutchInput;

        private bool portalGrabIsActive = false;
        private Matrix4x4 offsetMatrix = Matrix4x4.identity;
        private Matrix4x4 clutchingOriginDisplay = Matrix4x4.identity;
        private Matrix4x4 clutchingOriginView = Matrix4x4.identity;

        public bool worldGrabIsActive = false;
        public Transform input;
        public Transform input_initial, input_current;
        public Transform display_initial, display_current;
        public Transform view_initial, view_current;
        public Transform input_initial_in_view_current;
        public Transform input_current_in_view_current;
        public Matrix4x4 inital_display_to_controller_offset;
        private Vector3 worldGrabSteeringVector = Vector3.zero;

        //one handed portalview grab

        private bool collisionAtScreenCenter;
        public void SetCollisionAtScrenCenter(bool value) {
            this.collisionAtScreenCenter = value;
        }

        #endregion

        #region steering members
        [Header("Controller Steering Properties")]
        public InputActionProperty steeringInput;
        public InputActionProperty leftTriggerInput;
        public InputActionProperty rightTriggerInput;

        private float steeringValue {
            get {
                float steeringValue = (float)this.steeringInput.action?.ReadValue<float>();
                steeringValue = this.speedTransferFunction.Apply(steeringValue);
                steeringValue *= Time.deltaTime;
                return steeringValue;
            }
        }
        [SerializeField]
        [Tooltip("Specify the mapping of trigger input to steering speed in meters per second.")]
        private TestTransferfunction speedTransferFunction = new TestTransferfunction();
        
        [Header("Joystick Steering Properties")]
        private bool joystickSteeringIsActive = false;

        #endregion

        #region scaling members
        [Header("Scaling Properties")]
        public InputActionProperty scaleInput;
        public InputActionProperty scaleResetInput;
        [SerializeField]
        [Tooltip("Specify the mapping of joystick input to scaling speed in meters per second.")]
        private TestTransferfunction scaleTransferFunction = new TestTransferfunction();
        private float scalingValue {
            get {
                Vector2 scaleVector = (Vector2)this.scaleInput.action?.ReadValue<Vector2>();

                float scaleValue = Math.Abs(scaleVector.y);

                //apply transfer function f(x) = a+b*x^c
                scaleValue = this.scaleTransferFunction.Apply(scaleValue);

                //determining the sign
                scaleValue = Math.Sign(scaleVector.y) * scaleValue;

                //frame independence
                scaleValue = Time.deltaTime * scaleValue;
                return scaleValue;
            }
        }

        [SerializeField]
        private string status;

        [SerializeField]
        private float scaleResetTime = 1.5f;

        public TextMeshProUGUI guiElement;
        public Slider sliderElement;

        [SerializeField]
        private string displayedText = "1:1";
        #endregion

        #region Event Processing
        void Start() {
            this.UpdateScaleUI();
            this.CreateInteractionHelpers();
            this.grabInteractable.selectExited.AddListener(() => {
                //TODO check if this reset is necessary and actually happening
                if (this.portalGrabIsActive) {
                    this.StopPortalGrab();
                }
                this.portalGrabIsActive = false;
                this.worldGrabIsActive = false;
            });

            this.grabInteractable.selectEntered.AddListener(() => {
                if (this.collisionAtScreenCenter) {
                    this.portalGrabIsActive = true;
                    this.StartPortalGrab();
                }
            });
        }

        void Update() {
            //evaluate input
            /**
            this.clutchInput
            this.leftClutchInput
            this.rightClutchInput

            this.steeringInput

            this.scaleInput
            this.scaleResetInput

            this.input
            **/

            //apply input
            //if (this.isSelected == false)
            //    return;

            //one handed controller view grab init (needs to be before steering as steering uses variables here)
            //todos:
            //1) handle the event where one releases the grab button but still holds the clutch input button
            //currently this would lead to the situation that StopPortalGrab is not being called
            //one could think of going from one to two bool states: portalGrabIsActive -> leftPortalGrabIsActive, rightPortalGrabIsActive
            //2) also, what if both are pressed in this frame?
            if (this.worldGrabIsActive == false && this.leftClutchInput.action?.WasPressedThisFrame() == true && this.isSelectedWithLeftHand) {
                ExtendedLogger.LogInfo(this.GetType().Name, "Starting portal grab with left hand", this);
                this.portalGrabIsActive = true;
                this.StartPortalGrab();
            }

            if (this.portalGrabIsActive == true && this.leftClutchInput.action?.WasReleasedThisFrame() == true && this.isSelectedWithLeftHand) {
                ExtendedLogger.LogInfo(this.GetType().Name, "Stopping portal grab with left hand", this);
                this.portalGrabIsActive = false;
                this.StopPortalGrab();
            }

            if (this.worldGrabIsActive == false && this.rightClutchInput.action?.WasPressedThisFrame() == true && this.isSelectedWithRightHand) {
                ExtendedLogger.LogInfo(this.GetType().Name, "Starting portal grab with right hand", this);
                this.portalGrabIsActive = true;
                this.StartPortalGrab();
            }

            if (this.portalGrabIsActive == true && this.rightClutchInput.action?.WasReleasedThisFrame() == true && this.isSelectedWithRightHand) {
                ExtendedLogger.LogInfo(this.GetType().Name, "Stopping portal grab with right hand", this);
                this.portalGrabIsActive = false;
                this.StopPortalGrab();
            }

            if (this.isSelectedWithLeftHand == true && this.rightClutchInput.action?.IsPressed() == true){
                ExtendedLogger.LogInfo(this.GetType().Name, "Bimanual righthanded steering", this);
                //var avatar = NetworkUser.LocalInstance.avatarAnatomy as Photoportals.Avatar.AvatarHMDAnatomy;
                var avatar = NetworkUser.LocalInstance.avatarAnatomy as VRSYS.Core.Avatar.AvatarHMDAnatomy;
                var localRepr = this.transform.GetMatrix4x4().inverse * avatar.rightHand.transform.forward;
                var viewRepr = this.viewTransform.GetMatrix4x4() * localRepr;
                this.ApplySteeringVector(viewRepr * this.steeringValue, Space.Self);
            }

            if ( this.isSelectedWithRightHand == true && this.leftClutchInput.action?.IsPressed() == true){
                ExtendedLogger.LogInfo(this.GetType().Name, "Bimanual lefthanded steering", this);
                //var avatar = NetworkUser.LocalInstance.avatarAnatomy as Photoportals.Avatar.AvatarHMDAnatomy;
                var avatar = NetworkUser.LocalInstance.avatarAnatomy as VRSYS.Core.Avatar.AvatarHMDAnatomy;

                var localRepr = this.transform.GetMatrix4x4().inverse * avatar.leftHand.transform.forward;
                var viewRepr = this.viewTransform.GetMatrix4x4() * localRepr;
                this.ApplySteeringVector(viewRepr * this.steeringValue, Space.Self);
            }

            //debug log
            if (this.portalGrabIsActive == true && this.worldGrabIsActive == true) {
                ExtendedLogger.LogWarning(this.GetType().Name, "This shouldn't happen", this);
            }

            //one handed view steering
            if (this.portalGrabIsActive == true && this.steeringInput.action?.IsInProgress() == true && this.clutchInput.action?.IsPressed() == true) {
                ExtendedLogger.LogInfo(this.GetType().Name, "Performing steering (with clutching)", this);
                this.clutchingOriginView = Matrix4x4.Translate(this.viewTransform.forward * this.steeringValue) * this.clutchingOriginView;
            }

            //can i remove this?
            /**
            if (this.worldGrabIsActive == false && this.portalGrabIsActive == false && this.steeringInput.action?.IsInProgress() == true) {
                ExtendedLogger.LogInfo(this.GetType().Name, "Performing steering (without clutching)", this);
                this.viewTransform.position += this.viewTransform.forward * this.steeringValue;
            }
            **/

            //one handed controller view grab frame update
            if (this.portalGrabIsActive == true) {
                ExtendedLogger.LogInfo(this.GetType().Name, "Updating portal grab", this);
                this.UpdatePortalGrab();
            }

            //two handed controller/hand world grab (triggered by two selections one on portal display, one on portal view)
            if (this.worldGrabIsActive == true) {
                ExtendedLogger.LogInfo(this.GetType().Name, "Updating world grab", this);
                this.UpdateWorldGrab();
            }

            //scaling options via controller (both inputs are allowed simultaneously)
            if (this.isSelected && this.scaleInput.action?.WasPerformedThisFrame() == true) {
                ExtendedLogger.LogInfo(this.GetType().Name, "Scaling portal view with controller input", this);
                if ((this.viewTransform.localScale.x + this.scalingValue) < 1.0f) {
                    this.status = "Clamping";
                    this.viewTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                }
                else {
                    this.status = $"Applying {this.scalingValue}";
                    this.viewTransform.localScale += new Vector3(this.scalingValue, this.scalingValue, this.scalingValue);
                }
                this.UpdateScaleUI();
            }

            if (this.isSelected && this.scaleResetInput.action?.WasPerformedThisFrame() == true) {
                ExtendedLogger.LogInfo(this.GetType().Name, "Resetting portal view scale with controller input", this);
                this.status = $"Scale reset to 1.0";
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
            this.currentStatusMessage = message;
        }
        #endregion
        #region Scaling
        private void UpdateScaleUI() {
            //gui element
            float scaleValue = this.viewTransform.lossyScale.x;
            this.displayedText = $"1:{(int)scaleValue}";
            //this.guiElement.text = this.displayedText;

            //slider positioning
            float mappedScaleValue = 1f;
            switch (scaleValue) {
                case float n when (0f < n && n < 10f):
                    mappedScaleValue = 0f;
                    break;
                case float   n when (10f < n && n < 50f):
                    mappedScaleValue = 1f;
                    break;
                case float n when (50f < n && n < 100f):
                    mappedScaleValue = 2f;
                    break;
                case float n when (100f < n && n < 500f):
                    mappedScaleValue = 3f;
                    break;
                case float n when (500f < n && n < 1000f):
                    mappedScaleValue = 4f;
                    break;
                default:
                    mappedScaleValue = 0f;
                    break;
            }
            this.sliderElement.SetValueWithoutNotify(mappedScaleValue);
        }

        public void SetScale(float value) {
            this.SetScale(value, 2f, null, true);
        }

        public void SetScale(float value, bool uiUpdate = true) {
            this.SetScale(value, 2f, null, uiUpdate);
        }

        public void SetScale(float value, float time = 1f, System.Action onComplete = null, bool uiUpdate = true) {
            this.viewTransform.DOScale(value, time)
                .OnStart(() => this.UpdateComponentStatus($"Scaling to {value}"))
                .OnUpdate(() => {
                    if (uiUpdate) {
                        this.UpdateScaleUI();
                    }
                })
                .OnComplete(() => {
                    this.UpdateComponentStatus($"Finished Scaling to {value}");
                    onComplete?.Invoke();
                });
        }

        public float GetScale() {
            return this.viewTransform.lossyScale.x;
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
            this.interaction_root.transform.SetParent(GameObject.Find("Portal Interactions").transform);
            this.input_initial = this.interaction_root.transform.Find("input_initial").transform;
            this.input_current = this.interaction_root.transform.Find("input_current").transform;
            this.display_initial = this.interaction_root.transform.Find("display_initial").transform;
            this.display_current = this.interaction_root.transform.Find("display_current").transform;
            this.view_initial = this.interaction_root.transform.Find("view_initial").transform;
            this.view_current = this.interaction_root.transform.Find("view_current").transform;
            this.input_initial_in_view_current = this.interaction_root.transform.Find("input_initial_in_view_current").transform;
            this.input_current_in_view_current = this.interaction_root.transform.Find("input_current_in_view_current").transform;
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

        //these are being called by the SelectEnter and SelectExit events from the interactables
        public void SwitchToBimanualInteraction(SelectEnterEventArgs args) {
            ExtendedLogger.LogInfo(this.GetType().Name, "Switching to bimanual interaction", this);
            this.input = args.interactorObject.transform;
            /**
            if (args.interactorObject.handedness == InteractorHandedness.Left)
                this.input = (NetworkUser.LocalInstance.avatarAnatomy as AvatarHMDAnatomy).leftHand;
            if (args.interactorObject.handedness == InteractorHandedness.Right)
                this.input = (NetworkUser.LocalInstance.avatarAnatomy as AvatarHMDAnatomy).rightHand;
            **/
            this.SetupBimanualInteractionHelpers();
            this.worldGrabIsActive = true;
            this.portalGrabIsActive = false;
            this.UpdateComponentStatus("Bimanual");
        }

        public void SwitchToUnimanualInteraction(SelectExitEventArgs args) {
            ExtendedLogger.LogInfo(this.GetType().Name, "Switching to unimanual interaction", this);
            this.worldGrabIsActive = false;
            this.portalGrabIsActive = true;
            this.inital_display_to_controller_offset = Matrix4x4.identity;
            this.UpdateComponentStatus("Unimanual");
        }

        public void UpdateWorldGrab() {
            this.input_current.position = this.input.position;
            this.input_current.rotation = this.input.rotation;
            this.display_current.position = this.transform.position;
            this.display_current.rotation = this.transform.rotation;

            //apply steering that is provided by BubbleTechniqueWorldGrabTransformer
            if(this.worldGrabSteeringVector != Vector3.zero) {
                this.view_initial.position += this.worldGrabSteeringVector;
                this.worldGrabSteeringVector = Vector3.zero;
            }

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
        public void ApplySteering(Vector3 vector, bool inWorldSpace = false) {
            //how do i want the steering to look like?
            //is it meters per second, so shall i make it frame independent
            this.viewTransform.SetMatrix4x4(this.viewTransform.GetMatrix4x4() * Matrix4x4.Translate(vector));
            //this.viewTransform.position += vector;
            //this.viewTransform.position += this.viewTransform.forward * this.steeringValue;
        }
        public void EnqueueSteeringVector(Vector3 vector, Space space) {
            if(space == Space.World) {
                //TODO: view_initial is only used for world grab
                this.worldGrabSteeringVector = this.viewTransform.TransformVector(vector);
            }
            if(space == Space.Self) {
                this.worldGrabSteeringVector = vector;
            }
        }

        public void ApplySteeringVector(Vector3 direction, float value, Space space) {
            value = this.speedTransferFunction.Apply(value);
            this.ApplySteeringVector(direction * value, space);
            
        }

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
            ExtendedLogger.LogInfo(this.GetType().Name, $"Despawning Portal {this.name}", this);
            this.viewTransform.GetComponent<NetworkObject>().Despawn();
            this.GetComponent<NetworkObject>().Despawn();
        }

        #endregion

        #region Rotation Lock

        private bool rotationLock = true;
        public void EnableRotationLock() {
            this.rotationLock = true;
        }
        public void DisableRotationLock() {
            this.rotationLock = false;
        }
        public void ToggleRotationLock() {
            this.rotationLock = !this.rotationLock;
        }

        #endregion
    }
}