using UnityEngine;
using UnityEditor;
using VRSYS.Core.Networking;
using VRSYS.Core.Avatar;

namespace VRSYS.Photoportals {
    public class PortalHeadTracking : MonoBehaviour, INetworkUserCallbacks {
        public Transform portalDisplayHead;
        public Transform portalDisplayScreen;
        public Transform portalViewScreen;

        public Transform portalViewHead;

        public Transform viewRoot;

        #region States
        // Update is called once per frame
        void Update() {
            if (this.portalDisplayHead == null) return;
            if (this.portalDisplayScreen == null) return;
            if (this.portalViewScreen == null) return;
            if (this.portalViewHead == null) return;
            if (this.viewRoot == null) return;

            this.ApplyHeadTracking();
        }
        #endregion

        #region Methods
        private void ApplyHeadTracking() {
            Matrix4x4 headMatrix = Matrix4x4.TRS(portalDisplayHead.position, portalDisplayHead.rotation, Vector3.one);
            Matrix4x4 entranceMat = Matrix4x4.TRS(portalDisplayScreen.position, portalDisplayScreen.rotation, Vector3.one);
            Matrix4x4 entranceToHeadOffset = Matrix4x4.Inverse(entranceMat) * headMatrix;
            Matrix4x4 portalExitMat = Matrix4x4.TRS(portalViewScreen.position, portalViewScreen.rotation, this.viewRoot.transform.localScale);
            Matrix4x4 portalHeadMat = portalExitMat * entranceToHeadOffset;
            this.portalViewHead.transform.position = portalHeadMat.GetColumn(3);
            this.portalViewHead.transform.rotation = portalHeadMat.rotation;
        }
        #endregion

        #region Networking

        public void OnLocalNetworkUserSetup() {
            if (NetworkUser.LocalInstance.avatarAnatomy is AvatarAnatomy) {
                Debug.LogWarning("AvatarAnatomy determined but viewing setup switch not yet implemented.");
            }
            else if (NetworkUser.LocalInstance.avatarAnatomy is AvatarHMDAnatomy) {
                Debug.LogWarning("AvatarHMDAnatomy determined but viewing setup switch not yet implemented.");
            }
            else {
                Debug.LogError("AvatarAnatomy not found");
            }

            this.portalDisplayHead = NetworkUser.LocalInstance.head;
        }

        public void OnRemoteNetworkUserSetup(NetworkUser user) {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Editor
        /**
            Manuel Hartmann
            I'd like to see this in the /Editor folder as a derivative of Editor, but:
            I couldn't find a fitting way to have this render frame based without the object being selected.
            So for now we just have this region in the monobehaviour.
            https://discussions.unity.com/t/keep-my-custom-handle-visible-even-if-object-is-not-selected/97952
        **/
#if UNITY_EDITOR
        private void OnDrawGizmos() {
            if (this.portalDisplayHead == null) return;
            if (this.portalDisplayScreen == null) return;
            if (this.portalViewScreen == null) return;
            if (this.portalViewHead == null) return;

            Vector3 displayOffset = this.portalDisplayScreen.position - this.portalDisplayHead.position;
            Vector3 relativeDisplayOffset = this.portalDisplayScreen.InverseTransformDirection(displayOffset);
            Vector3 viewOffset = this.portalViewScreen.position - this.portalViewHead.transform.position;
            Vector3 relativeViewOffset = this.portalViewScreen.InverseTransformDirection(viewOffset);
            Handles.color = Color.white;
            Handles.DrawLine(this.portalDisplayHead.position, this.portalDisplayScreen.position);
            Handles.DrawLine(this.portalViewHead.transform.position, this.portalViewScreen.position);
            Handles.Label(Vector3.Lerp(this.portalDisplayHead.position, this.portalDisplayScreen.position, 0.5f), relativeDisplayOffset.ToString());
            Handles.Label(Vector3.Lerp(this.portalViewScreen.position, this.portalViewHead.transform.position, 0.5f), relativeViewOffset.ToString());
        }
#endif
        #endregion
    }
}