using UnityEngine;
using VRSYS.Core.Networking;
using VRVIS.Photoportals;

namespace VRVIS.Photoportals.Samples.MultiuserHMDExample
{
   public class PortalNetworkUserCallbacks : MonoBehaviour, INetworkUserCallbacks
    {
        public PortalExitHeadTracking behaviour;
        public void OnLocalNetworkUserSetup()
        {
            behaviour.portalEntranceHead = NetworkUser.LocalInstance.head;
            //throw new System.NotImplementedException();
        }

        public void OnRemoteNetworkUserSetup(NetworkUser user)
        {
            throw new System.NotImplementedException();
        }

    }   
}