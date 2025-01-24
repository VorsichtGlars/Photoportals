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
//   Authors:        Sebastian Muehlhaus, Andr� Kunert, Lucky Chandrautama
//   Date:           2022
//-----------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace VRVIS.Photoportals
{
    [RequireComponent(typeof(Camera))]
    public class OffAxisProjection : MonoBehaviour
    {
        // externals
        public ScreenProperties screen;        

        public Vector3 eyePos{ get; set; }
        
        [SerializeField]
        public Camera cam;

        public bool autoUpdate = false;
        public bool calcNearClipPlane = false;

        #region States
        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (autoUpdate)
                CalcProjection();
        }

        #endregion

        #region Methods
        public void CalcProjection()
        {

            // Q1 What is the purpose of this?
            transform.localRotation = Quaternion.Inverse(transform.parent.localRotation);
            // Q1

            eyePos = transform.position;

            var eyePosSP = screen.transform.worldToLocalMatrix * new Vector4(eyePos.x, eyePos.y, eyePos.z, 1f);
            eyePosSP *= -1f;

            var near = cam.nearClipPlane;
            if(calcNearClipPlane)
            {
                var s1 = screen.transform.position;
                var s2 = screen.transform.position - screen.transform.forward;
                var camOnScreenForward = Vector3.Project((transform.position - s1), (s2 - s1)) + s1;
                near = Vector3.Distance(screen.transform.position, camOnScreenForward);
            }
            var far = cam.farClipPlane;

            var factor = near / eyePosSP.z;
            var l = (eyePosSP.x - screen.width * 0.5f) * factor;
            var r = (eyePosSP.x + screen.width * 0.5f) * factor;
            var b = (eyePosSP.y - screen.height * 0.5f) * factor;
            var t = (eyePosSP.y + screen.height * 0.5f) * factor;

            cam.projectionMatrix = Matrix4x4.Frustum(l, r, b, t, near, far);
        }

        #endregion
        
        #region Editor
        #if UNITY_EDITOR
        private bool showExtraGizmos = false;

        [ContextMenu("Toggle Extra Gizmos")]
        public void ToggleGizmos(){
            this.showExtraGizmos = !this.showExtraGizmos;
        }
        /**
            I'd like to see this in the /Editor folder as a derivative of Editor, but:
            I couldn't find a fitting way to have this render frame based without the object being selected.
            So for now we just have this region in the monobehaviour.
            https://discussions.unity.com/t/keep-my-custom-handle-visible-even-if-object-is-not-selected/97952
        **/
        private void OnDrawGizmos(){
            if(this.showExtraGizmos == false) return;
            Handles.color = Color.white;
            
            //rays from camera to near clip plane corners
            Handles.DrawLine(this.eyePos, this.screen.bottomLeftCorner);
            Handles.DrawLine(this.eyePos, this.screen.bottomRightCorner);
            Handles.DrawLine(this.eyePos, this.screen.topLeftCorner);
            Handles.DrawLine(this.eyePos, this.screen.topRightCorner);

            //near clip plane corners
            Handles.DrawLine(this.screen.bottomLeftCorner, this.screen.topLeftCorner);
            Handles.DrawLine(this.screen.topLeftCorner, this.screen.topRightCorner);
            Handles.DrawLine(this.screen.topRightCorner, this.screen.bottomRightCorner);
            Handles.DrawLine(this.screen.bottomRightCorner, this.screen.bottomLeftCorner);
            
            //rays from near clip plane to far clip plane corners
            //https://discussions.unity.com/t/how-do-i-get-the-world-coordinates-of-corners-of-far-clip-panel-of-camera-in-a-script/899976
            Vector3 farPlaneBottomLeftCorner = this.cam.ViewportToWorldPoint(new Vector3(0,0, this.cam.farClipPlane));
            Vector3 farPlaneBottomRightCorner = this.cam.ViewportToWorldPoint(new Vector3(1,0, this.cam.farClipPlane));
            Vector3 farPlaneTopLeftCorner = this.cam.ViewportToWorldPoint(new Vector3(0,1, this.cam.farClipPlane));
            Vector3 farPlaneTopRightCorner = this.cam.ViewportToWorldPoint(new Vector3(1,1, this.cam.farClipPlane));
            Handles.DrawLine(this.screen.bottomLeftCorner , farPlaneBottomLeftCorner);
            Handles.DrawLine(this.screen.bottomRightCorner, farPlaneBottomRightCorner);
            Handles.DrawLine(this.screen.topLeftCorner, farPlaneTopLeftCorner);
            Handles.DrawLine(this.screen.topRightCorner, farPlaneTopRightCorner);

            //far clip plane corners
            Handles.DrawLine(farPlaneBottomLeftCorner, farPlaneTopLeftCorner);
            Handles.DrawLine(farPlaneTopLeftCorner, farPlaneTopRightCorner);
            Handles.DrawLine(farPlaneTopRightCorner, farPlaneBottomRightCorner);
            Handles.DrawLine(farPlaneBottomRightCorner, farPlaneBottomLeftCorner);
        }
        #endif
        #endregion
    }
}