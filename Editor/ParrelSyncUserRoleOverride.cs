
#if UNITY_EDITOR

//TODO needs assemblydefinition (i assume)

using UnityEngine;
using VRSYS.Core.Networking;

using ParrelSync;

namespace VRSYS.Photoportals {

public class ParrelSyncUserRoleOverride : MonoBehaviour {
    
    public NetworkUserSpawnInfo infoToOverride;

    [UserRoleSelector]
    public UserRole serverRoleToSet;

    void Awake() {
        #if UNITY_EDITOR
        if (ClonesManager.IsClone()) {
            Debug.Log("Starting clone as server");
            // Automatically connect to local host if this is the clone editor
            Debug.Log($"ClonesManager.DefaultArgument {ClonesManager.DefaultArgument}");
            this.infoToOverride.userRole = this.serverRoleToSet;
            this.infoToOverride.userName = "Server";
        }else{
            // Automatically start server if this is the original editor
        }
        #endif
    }
}

}
#endif