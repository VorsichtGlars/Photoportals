using UnityEngine;
using UnityEngine.InputSystem;

namespace VRVIS.Photoportals
{
    public class BasicScaleInteraction : MonoBehaviour
    {

        public InputActionProperty scaleInput;
        public InputActionProperty scaleResetInput;
        public Transform viewTransform;
        [SerializeField]
        private float scalePerSecond = 1.0f;



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
            //guard for no grabbed portal
            Vector2 scaleVector = (Vector2)this.scaleInput.action?.ReadValue<Vector2>();
            //Debug.Log("scaleVector: " + scaleVector.x + " " + scaleVector.y);
            float scaleValue = this.scalePerSecond * Time.deltaTime * scaleVector.y;
            //is the range from 0 to 1 and from 1 to infinity?
            this.viewTransform.localScale += new Vector3(scaleValue, scaleValue, scaleValue);
        }
    }
}