using UnityEngine;
using VRSYS.Core.Avatar;
using VRSYS.Core.Networking;
using VRVIS.Photoportals;

namespace VRVIS.Photoportals.Samples.MultiuserHMDExample
{
   public class PortalNetworkUserCallbacks : MonoBehaviour, INetworkUserCallbacks
    {
        public PortalExitHeadTracking behaviour;
        public void OnLocalNetworkUserSetup()
        {
            if(NetworkUser.LocalInstance.avatarAnatomy is AvatarAnatomy){
                Debug.LogWarning("AvatarAnatomy determined but viewing setup switch not yet implemented.");
            } else if(NetworkUser.LocalInstance.avatarAnatomy is AvatarHMDAnatomy){
                Debug.LogWarning("AvatarHMDAnatomy determined but viewing setup switch not yet implemented.");
            } else {
                Debug.LogError("AvatarAnatomy not found");
            }

            behaviour.portalEntranceHead = NetworkUser.LocalInstance.head;
        }

        public void OnRemoteNetworkUserSetup(NetworkUser user)
        {
            throw new System.NotImplementedException();
        }

    }   
}