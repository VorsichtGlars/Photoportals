using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace VRVIS.Photoportals
{
    public class BasicClutchInteraction : MonoBehaviour
    {

        public InputActionProperty clutchInput;
        public Transform viewTransform;
        public XRGrabInteractable grabInteractable;
        private Matrix4x4 offsetMatrix = Matrix4x4.identity;
        private Matrix4x4 clutchingOriginDisplay = Matrix4x4.identity;
        private Matrix4x4 clutchingOriginView = Matrix4x4.identity;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if(this.clutchInput.action?.WasPressedThisFrame() == true){
                Debug.Log("Clutch WasPressed");
                this.OnClutchActionWasPressed();
            }

            if(this.clutchInput.action?.IsPressed() == true){
                Debug.Log("Clutch IsPressed");
                this.OnClutchActionIsPressed();
            }

            if(this.clutchInput.action?.WasReleasedThisFrame() == true){
                Debug.Log("Clutch WasReleased");
                this.OnClutchActionWasReleased();
            }
        }

        void OnClutchActionWasPressed(){
            if(this.grabInteractable.isSelected == false){
                Debug.Log("Nothing Grabbed");
                return;
            }
            //this.offsetMatrix = GetTransformationMatrix(this.grabInteractable.transform, true).inverse *
            //GetTransformationMatrix(this.viewTransform, true);
            this.clutchingOriginDisplay = GetTransformationMatrix(this.grabInteractable.transform, true);
            this.clutchingOriginView = GetTransformationMatrix(this.viewTransform, true);
        }

        void OnClutchActionIsPressed(){
            if(this.clutchingOriginDisplay == Matrix4x4.identity)
                return;
            //if(this.offsetMatrix == Matrix4x4.identity)
            //    return;
            //Matrix4x4 newMatrix = GetTransformationMatrix(this.grabInteractable.transform, true) * this.offsetMatrix;
            //this.viewTransform.position = newMatrix.GetColumn(3);
            //this.viewTransform.rotation = newMatrix.rotation;
            
            //calculate relative offset
            this.offsetMatrix = GetTransformationMatrix(this.grabInteractable.transform,true).inverse * this.clutchingOriginDisplay;          
            this.offsetMatrix = this.offsetMatrix.inverse; //actually i don't know why this is necessary

            //calculate absolute offset
            //potential bug here: what happens when the scale is changed during clutching?
            this.offsetMatrix = this.clutchingOriginView * this.offsetMatrix;

            //apply offset
            this.viewTransform.position = this.offsetMatrix.GetColumn(3);
            this.viewTransform.rotation = this.offsetMatrix.rotation;
        }

        void OnClutchActionWasReleased(){
            this.clutchingOriginDisplay = Matrix4x4.identity;
            this.offsetMatrix = Matrix4x4.identity;
        }

        #region Utilities
        //TODO Move to global utility class
        public Matrix4x4 GetTransformationMatrix(Transform t, bool inWorldSpace = true)
        {
            if (inWorldSpace)
            {
                return Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);
            }
            else
            {
                return Matrix4x4.TRS(t.localPosition, t.localRotation, t.localScale);
            }
        }
        #endregion
    }
}