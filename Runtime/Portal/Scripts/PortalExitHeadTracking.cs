// VRSYS plugin of Virtual Reality and Visualization Research Group (Bauhaus University Weimar)
//  _    ______  _______  _______
// | |  / / __ \/ ___/\ \/ / ___/
// | | / / /_/ /\__ \  \  /\__ \ 
// | |/ / _, _/___/ /  / /___/ / 
// |___/_/ |_|/____/  /_//____/  
//
//  __                            __                       __   __   __    ___ .  . ___
// |__)  /\  |  | |__|  /\  |  | /__`    |  | |\ | | \  / |__  |__) /__` |  |   /\   |  
// |__) /~~\ \__/ |  | /~~\ \__/ .__/    \__/ | \| |  \/  |___ |  \ .__/ |  |  /~~\  |  
//
//       ___               __                                                           
// |  | |__  |  |\/|  /\  |__)                                                          
// |/\| |___ |  |  | /~~\ |  \                                                                                                                                                                                     
//
// Copyright (c) 2022 Virtual Reality and Visualization Research Group
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//-----------------------------------------------------------------
//   Authors:        Sebastian Muehlhaus, Manuel Hartmann
//   Date:           2022, 2024
//-----------------------------------------------------------------

using UnityEngine;
using UnityEditor;
using VRSYS.Core.Networking;
using VRSYS.Core.Avatar;

namespace VRVIS.Photoportals {
    public class PortalExitHeadTracking : MonoBehaviour, INetworkUserCallbacks {
        public Transform portalEntranceHead;
        public Transform portalEntranceScreen;
        public Transform portalExitScreen;

        public Transform portalExitHead;

        public Transform viewRoot;

        #region States
        // Update is called once per frame
        void Update() {
            if (this.portalEntranceHead == null) return;
            if (this.portalEntranceScreen == null) return;
            if (this.portalExitScreen == null) return;
            if (this.portalExitHead == null) return;
            if (this.viewRoot == null) return;

            this.ApplyHeadTracking();
        }
        #endregion

        #region Methods
        private void ApplyHeadTracking() {
            Matrix4x4 headMatrix = Matrix4x4.TRS(portalEntranceHead.position, portalEntranceHead.rotation, Vector3.one);
            Matrix4x4 entranceMat = Matrix4x4.TRS(portalEntranceScreen.position, portalEntranceScreen.rotation, Vector3.one);
            Matrix4x4 entranceToHeadOffset = Matrix4x4.Inverse(entranceMat) * headMatrix;
            Matrix4x4 portalExitMat = Matrix4x4.TRS(portalExitScreen.position, portalExitScreen.rotation, this.viewRoot.transform.localScale);
            Matrix4x4 portalHeadMat = portalExitMat * entranceToHeadOffset;
            this.portalExitHead.transform.position = portalHeadMat.GetColumn(3);
            this.portalExitHead.transform.rotation = portalHeadMat.rotation;
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

            this.portalEntranceHead = NetworkUser.LocalInstance.head;
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
            if (this.portalEntranceHead == null) return;
            if (this.portalEntranceScreen == null) return;
            if (this.portalExitScreen == null) return;
            if (this.portalExitHead == null) return;

            Vector3 displayOffset = this.portalEntranceScreen.position - this.portalEntranceHead.position;
            Vector3 relativeDisplayOffset = this.portalEntranceScreen.InverseTransformDirection(displayOffset);
            Vector3 viewOffset = this.portalExitScreen.position - this.portalExitHead.transform.position;
            Vector3 relativeViewOffset = this.portalExitScreen.InverseTransformDirection(viewOffset);
            Handles.color = Color.white;
            Handles.DrawLine(this.portalEntranceHead.position, this.portalEntranceScreen.position);
            Handles.DrawLine(this.portalExitHead.transform.position, this.portalExitScreen.position);
            Handles.Label(Vector3.Lerp(this.portalEntranceHead.position, this.portalEntranceScreen.position, 0.5f), relativeDisplayOffset.ToString());
            Handles.Label(Vector3.Lerp(this.portalExitScreen.position, this.portalExitHead.transform.position, 0.5f), relativeViewOffset.ToString());
        }
#endif
        #endregion
    }
}