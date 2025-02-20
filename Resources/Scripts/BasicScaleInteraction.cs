using UnityEngine;
using UnityEngine.InputSystem;

namespace VRVIS.Photoportals
{
    public class BasicScaleInteraction : MonoBehaviour
    {

        public InputActionProperty scaleInput;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if(this.scaleInput.action?.WasPerformedThisFrame() == true)
            {
                Debug.Log("Scale performed");
                this.ApplyScaleInput();
            }
        }

        private void ApplyScaleInput()
        {
            float scaleValue = (float)this.scaleInput.action?.ReadValue<float>();
            Debug.Log("scaleValue: " + scaleValue);
        }
    }
}