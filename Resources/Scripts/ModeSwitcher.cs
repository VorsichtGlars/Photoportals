using UnityEngine;
using Vrsys.Photoportals;

namespace VRVIS.Photoportals
{
    public enum PortalMode
    {
        Undefined,
        MonoPlane,
        StereoPlane,
        MonoBox,
        StereoBox
    }
    public class ModeSwitcher : MonoBehaviour
    {
        public PortalMode mode;

        public GameObject monoDisplay;
        public GameObject monoDisplayProxy;

        public GameObject monoCamera;

        public PortalExitHeadTracking portalExitHeadTracking;

        public void SwitchMode(PortalMode requestedMode)
        {
            switch (requestedMode)
            {
                case PortalMode.MonoPlane:
                    this.mode = PortalMode.MonoPlane;
                    this.monoDisplay.SetActive(true);
                    this.monoDisplayProxy.SetActive(true);
                    this.monoCamera.SetActive(true);
                    this.portalExitHeadTracking.enabled = true;
                    this.portalExitHeadTracking.portalEntranceScreen = this.monoDisplay.transform;
                    this.portalExitHeadTracking.portalExitScreen = this.monoDisplayProxy.transform;
                    break;
                case PortalMode.StereoPlane:
                    throw new System.NotImplementedException();
                    this.mode = PortalMode.StereoPlane;
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
                    this.mode = PortalMode.Undefined;
                    break;
            }
        }

        #region States
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            this.SwitchMode(PortalMode.MonoPlane);
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        #endregion
    }
   
}