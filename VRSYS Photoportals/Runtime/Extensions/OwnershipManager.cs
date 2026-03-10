using Unity.Netcode;
using UnityEngine;

namespace VRSYS.Photoportals.Networking {

    public class OwnershipManager : NetworkBehaviour {

        private NetworkVariable<bool> isOwnershipLocked = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        #region Custom Methods
        [ContextMenu("RequestOwnershipFromThisClient")]
        public void RequestOwnershipFromThisClient() {
            if (this.IsOwner == true) {
                Debug.LogWarning("Already the owner of this object.");
                return;
            }
            if (this.IsOwnershipLocked()) {
                Debug.LogWarning("Ownership is locked for this object.");
                return;
            }
            this.SwitchOwnerServerRpc(this.NetworkManager.LocalClientId);
        }

        [ContextMenu("ReturnOwnershipToServer")]
        public void ReturnOwnershipToServer() {
            if (this.IsOwner == false) {
                Debug.LogWarning("Not the owner of this object.");
                return;
            }
            if (this.IsOwnershipLocked()) { // or do i want the ownership lock to be set to false implicitly?
                Debug.LogWarning("Ownership is locked for this object.");
                return;
            }
            this.SwitchOwnerServerRpc(NetworkManager.ServerClientId);
        }

        [ContextMenu("LockOwnership")]
        public void LockOwnership() {
            if (this.IsOwner == false) {
                Debug.LogWarning("Not the owner of this object.");
                return;
            }
            this.SetLockingStateServerRpc(true);
        }

        [ContextMenu("UnlockOwnership")]
        public void UnlockOwnership() {
            if (this.IsOwner == false) {
                Debug.LogWarning("Not the owner of this object.");
                return;
            }
            this.SetLockingStateServerRpc(false);
        }

        public bool IsOwnershipLocked() {
            return this.isOwnershipLocked.Value;
        }
        #endregion

        #region RPCs

        [ServerRpc(RequireOwnership = false)]
        private void SwitchOwnerServerRpc(ulong newOwner) {
            var name = this.NetworkObject.gameObject.name;
            Debug.Log($"Switching ownership of {name} to client {newOwner}");
            this.NetworkObject.ChangeOwnership(newOwner);
        }

        [ServerRpc(RequireOwnership = true)]
        private void SetLockingStateServerRpc(bool lockingState) {
            Debug.Log($"Setting ownership locking state to {lockingState}");
            this.isOwnershipLocked.Value = lockingState;           
        }

        #endregion

        #region Callbacks
/*        
        override public void OnGainedOwnership() {
            Debug.Log("OnGainedOwnership");            
            base.OnGainedOwnership();
        }

        override public void OnLostOwnership() {
            Debug.Log("OnLostOwnership");            
            base.OnLostOwnership();
        }
*/
        #endregion
    }
}