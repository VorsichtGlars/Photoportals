using UnityEngine;
using VRSYS.Core.Avatar;
using VRSYS.Core.Networking;

namespace VRVIS.Photoportals
{
    public enum PortalMode
    {
        None,
        MonoPlane,
        StereoPlane,
        MonoBox,
        StereoBox
    }

    [ExecuteInEditMode]
    public class ModeSwitcher : MonoBehaviour, INetworkUserCallbacks
    {
        public PortalMode mode = PortalMode.None;
        public PortalExitHeadTracking portalExitHeadTracking;

        [Header("Plane Mode Monoscopic Viewing Setup")]
        public GameObject monoDisplay;
        public GameObject monoDisplayProxy;

        public GameObject monoCamera;

        [Header("Plane Mode Stereoscopic Viewing Setup")]

        public GameObject stereoDisplay;

        public GameObject stereoDisplayProxy;

        public GameObject leftCamera;
        public GameObject rightCamera;


        #region Methods

        [ContextMenu("ResetCurrentMode()")]
        public void ResetCurrentMode(){
            switch (this.mode)
            {
                case PortalMode.MonoPlane:
                    this.monoDisplay.SetActive(false);
                    this.monoDisplayProxy.SetActive(false);
                    this.monoCamera.SetActive(false);
                    break;
                case PortalMode.StereoPlane:
                    this.stereoDisplay.SetActive(false);
                    this.stereoDisplayProxy.SetActive(false);
                    this.leftCamera.SetActive(false);
                    this.rightCamera.SetActive(false);
                    break;
                case PortalMode.MonoBox:
                    Debug.LogWarning("MonoBox mode not implemented yet");
                    break;
                case PortalMode.StereoBox:
                    Debug.LogWarning("StereoBox mode not implemented yet");
                    break;
                case PortalMode.None:
                    break;
            }
        }

        [ContextMenu("SwitchModeToMonoPlane()")]
        public void SwitchModeToMonoPlane()
        {
            this.SwitchMode(PortalMode.MonoPlane);
        }

        [ContextMenu("SwitchModeToSteroPlane()")]
        public void SwitchModeToSteroPlane()
        {
            this.SwitchMode(PortalMode.StereoPlane);
        }

        public void SwitchMode(PortalMode requestedMode)
        {
            if (this.mode == requestedMode)
            {
                return;
            }

            this.ResetCurrentMode();
            
            switch (requestedMode)
            {
                case PortalMode.MonoPlane:
                    this.mode = PortalMode.MonoPlane;
                    this.monoDisplay.SetActive(true);
                    this.monoDisplayProxy.SetActive(true);
                    this.monoCamera.SetActive(true);
                    this.portalExitHeadTracking.portalEntranceScreen = this.monoDisplay.transform;
                    this.portalExitHeadTracking.portalExitScreen = this.monoDisplayProxy.transform;
                    break;
                case PortalMode.StereoPlane:
                    this.mode = PortalMode.StereoPlane;
                    this.stereoDisplay.SetActive(true);
                    this.stereoDisplayProxy.SetActive(true);
                    this.leftCamera.SetActive(true);
                    this.rightCamera.SetActive(true);
                    this.portalExitHeadTracking.portalEntranceScreen = this.stereoDisplay.transform;
                    this.portalExitHeadTracking.portalExitScreen = this.stereoDisplayProxy.transform;
                    break;
                case PortalMode.MonoBox:
                    throw new System.NotImplementedException();
                    this.mode = PortalMode.MonoBox;
                    break;
                case PortalMode.StereoBox:
                    throw new System.NotImplementedException();
                    this.mode = PortalMode.StereoBox;
                    break;
                default:
                    this.mode = PortalMode.None;
                    break;
            }
        }

        #endregion

        #region States
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            //this.SwitchMode(PortalMode.StereoPlane);
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        void OnValidate()
        {
            //this.SwitchMode(this.mode);
        }

        #endregion

        #region Callbacks
        public void OnLocalNetworkUserSetup()
        {
            portalExitHeadTracking.portalEntranceHead = NetworkUser.LocalInstance.head;
            return;
            if(NetworkUser.LocalInstance.avatarAnatomy is AvatarAnatomy){
                Debug.Log("AvatarAnatomy determined: Setting viewing setup to mono.");
                this.SwitchMode(PortalMode.MonoPlane);
            } else if(NetworkUser.LocalInstance.avatarAnatomy is AvatarHMDAnatomy){
                this.SwitchMode(PortalMode.StereoPlane);
                Debug.Log("AvatarHMDAnatomy determined: Setting viewing setup to stereo.");
            } else {
                Debug.LogError("AvatarAnatomy not determined");
                this.SwitchMode(PortalMode.None);
            }
        }

        public void OnRemoteNetworkUserSetup(NetworkUser user)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
   
}