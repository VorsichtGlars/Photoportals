using UnityEngine;
using VRSYS.Core.Avatar;
using VRSYS.Core.Networking;

namespace VRVIS.Photoportals
{
    [ExecuteInEditMode]
    public class MonoscopicPlane : MonoBehaviour, INetworkUserCallbacks
    {
        public PortalMode mode = PortalMode.None;
        public PortalExitHeadTracking portalExitHeadTracking;

        [Header("Viewing Setup")]
        public GameObject monoDisplay;
        public GameObject monoDisplayProxy;

        public GameObject monoCamera;

        #region Methods

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
        }

        public void OnRemoteNetworkUserSetup(NetworkUser user)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
   
}