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

using System.Collections;
using System.Collections.Generic;
using Codice.Client.BaseCommands.Merge.Xml;
using UnityEngine;

namespace Vrsys.Photoportals
{
    public class PortalExitHeadTracking : MonoBehaviour
    {
        public Transform portalEntranceHead;
        public Transform portalEntranceScreen;
        public Transform portalExitScreen;

        // Update is called once per frame
        void Update()
        {
            if(portalEntranceHead == null) return;
            if(portalEntranceScreen == null) return;
            if(portalExitScreen == null) return;

            Matrix4x4 headMatrix = Matrix4x4.TRS(portalEntranceHead.position, portalEntranceHead.rotation, Vector3.one);
            Matrix4x4 entranceMat = Matrix4x4.TRS(portalEntranceScreen.position, portalEntranceScreen.rotation, Vector3.one);
            Matrix4x4 entranceToHeadOffset = Matrix4x4.Inverse(entranceMat) * headMatrix;
            Matrix4x4 portalExitMat = Matrix4x4.TRS(portalExitScreen.position, portalExitScreen.rotation, Vector3.one);
            Matrix4x4 portalHeadMat = portalExitMat * entranceToHeadOffset;
            transform.position = portalHeadMat.GetColumn(3);
            transform.rotation = portalHeadMat.rotation;
        }
    }
}
