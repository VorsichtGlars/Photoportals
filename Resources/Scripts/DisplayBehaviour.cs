using Unity.VisualScripting;
using UnityEngine;

namespace VRVIS.Photoportals
{
    enum DisplayType
    {
        MonoWindow,
        StereoWindow,
        MonoBox,
        StereoBox
    }

    public class DisplayBehaviour : MonoBehaviour
    {
        DisplayType displayType;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if(this.displayType != DisplayType.MonoWindow)
            {
                Debug.LogWarning("DisplayBehaviour: DisplayType is not set to MonoWindow. Setting it to MonoWindow.");
                this.displayType = DisplayType.MonoWindow;
            }
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}