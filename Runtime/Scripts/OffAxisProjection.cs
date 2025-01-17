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

namespace Vrsys.Photoportals
{
    [RequireComponent(typeof(Camera))]
    public class OffAxisProjection : MonoBehaviour
    {
        // externals
        public ScreenProperties screen;        

        private Vector3 eyePos;
        private Camera cam;

        public bool autoUpdate = false;
        public bool calcNearClipPlane = false;

        public bool magic = true;

        public bool drawGizmos = true;

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

        public void CalcProjection()
        {

            // Q1 What is the purpose of this?
            if(this.magic){
                transform.localRotation = Quaternion.Inverse(transform.parent.localRotation);
            }
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

        //TODO: Move to Editor Script
        private void OnDrawGizmos(){
            if(drawGizmos == false) return;
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(eyePos, 0.05f);

            //rays from camera to near clip plane corners
            Gizmos.DrawLine(eyePos, screen.bottomLeftCorner);
            Gizmos.DrawLine(eyePos, screen.bottomRightCorner);
            Gizmos.DrawLine(eyePos, screen.topLeftCorner);
            Gizmos.DrawLine(eyePos, screen.topRightCorner);

            //near clip plane corners
            Gizmos.DrawLine(screen.bottomLeftCorner, screen.topLeftCorner);
            Gizmos.DrawLine(screen.topLeftCorner, screen.topRightCorner);
            Gizmos.DrawLine(screen.topRightCorner, screen.bottomRightCorner);
            Gizmos.DrawLine(screen.bottomRightCorner, screen.bottomLeftCorner);
            
            //rays from near clip plane to far clip plane corners
            //https://discussions.unity.com/t/how-do-i-get-the-world-coordinates-of-corners-of-far-clip-panel-of-camera-in-a-script/899976
            Vector3 farPlaneBottomLeftCorner = this.cam.ViewportToWorldPoint(new Vector3(0,0, cam.farClipPlane));
            Vector3 farPlaneBottomRightCorner = this.cam.ViewportToWorldPoint(new Vector3(1,0, cam.farClipPlane));
            Vector3 farPlaneTopLeftCorner = this.cam.ViewportToWorldPoint(new Vector3(0,1, cam.farClipPlane));
            Vector3 farPlaneTopRightCorner = this.cam.ViewportToWorldPoint(new Vector3(1,1, cam.farClipPlane));
            Gizmos.DrawLine(screen.bottomLeftCorner , farPlaneBottomLeftCorner);
            Gizmos.DrawLine(screen.bottomRightCorner, farPlaneBottomRightCorner);
            Gizmos.DrawLine(screen.topLeftCorner, farPlaneTopLeftCorner);
            Gizmos.DrawLine(screen.topRightCorner, farPlaneTopRightCorner);

            //far clip plane corners
            Gizmos.DrawLine(farPlaneBottomLeftCorner, farPlaneTopLeftCorner);
            Gizmos.DrawLine(farPlaneTopLeftCorner, farPlaneTopRightCorner);
            Gizmos.DrawLine(farPlaneTopRightCorner, farPlaneBottomRightCorner);
            Gizmos.DrawLine(farPlaneBottomRightCorner, farPlaneBottomLeftCorner);
        }
    }
}