using System;

using UnityEngine;

using UnityEngine.Events;


namespace VRSYS.Photoportals.Extensions {

    public static class TransformExtensions {
        public static Matrix4x4 GetMatrix4x4(this Transform t, bool inWorldSpace = true) {
            if (inWorldSpace) {
                return Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);
            }
            else {
                return Matrix4x4.TRS(t.localPosition, t.localRotation, t.localScale);
            }
        }
        public static void SetMatrix4x4(this Transform transform, Matrix4x4 matrix) {
            transform.position = matrix.GetColumn(3);
            transform.rotation = matrix.rotation;
            //potentially not what we want
            transform.localScale = matrix.lossyScale;
        }

        public static void ResetMatrix4x4(this Transform transform) {
            transform.SetMatrix4x4(Matrix4x4.identity);
        }
    }
    public static class Matrix4x4Extensions {
        public static Vector3 GetPosition(this Matrix4x4 matrix) {
            return matrix.GetColumn(3);
        }

        public static Vector3 GetScale(this Matrix4x4 matrix) {
            return new Vector3(matrix.GetColumn(0).magnitude,
                                matrix.GetColumn(1).magnitude,
                                matrix.GetColumn(2).magnitude);
        }
    }

    public static class FloatExtensions {
        public static float Map(this float from, float fromMin, float fromMax, float toMin,  float toMax)
        {
            var fromAbs  =  from - fromMin;
            var fromMaxAbs = fromMax - fromMin;       
        
            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;
        
            return to;
        }
    }

    public static class Geometry {
        //https://forum.unity.com/threads/projection-of-point-on-plane.855958/
        public static Vector3 ClosestPointOnPlane(Vector3 planeOffset, Vector3 planeNormal, Vector3 point)
            => point + DistanceFromPlane(planeOffset, planeNormal, point) * planeNormal;

        public static float DistanceFromPlane(Vector3 planeOffset, Vector3 planeNormal, Vector3 point)
            => Vector3.Dot(planeOffset - point, planeNormal);
    }

    public static class UnityEventExtensions {

    /**
    So in Unity there is this difference between assigning function calls to an event in the editor gui
    and assigning function calls to an event in code.
    In code i do need to have the event being passed in the function signature.
    This kinda sucks because i need to have a reference to that specific eventdata in a class.
    That just does not need to know of this signature at all.
    It increases code cohesion and i dont want this.
    Using lambdas is a way to pass these calls in code without having the event being passed in the signature.
    https://chatgpt.com/share/e/69035dc7-8618-8000-8218-33d9eba54863
    **/
    public static void AddListener<T>(this UnityEvent<T> evt, System.Action action)
        => evt.AddListener(_ => action());
    }


        [System.Serializable]
    internal class TestTransferfunction {

        [SerializeField]
        [Tooltip("Specify the minimum value.")]
        private float minimum;
        [SerializeField]
        [Tooltip("Specify the maximum value.")]
        private float maximum;
        [SerializeField]
        [Tooltip("Specify the curve shape (0.0,1.0) = EaseOut, 1.0 = Linear, (1.0,inf) = EaseIn.")]
        private float shape;

        //f(x) = a+b*x^c
        public float Apply(float x) {
            if (x < 0.0f) {
                //ExtendedLogger.LogWarning(this.GetType().Name, "x value for TransferFucntion should be positive");
            }
            if (x < 0.0f || x > 1.0f) {
                //ExtendedLogger.LogWarning(this.GetType().Name, "x value for TransferFucntion should be [0.0,1.0]");
            }
            return (float)(this.minimum + this.maximum * Math.Pow(x, this.shape));
        }


        //todo: check for validity of values
        public void SetMinimum(float value) {
            this.minimum = value;
        }

        public void SetMaximum(float value) {
            this.maximum = value;
        }

        public void SetShape(float value) {
            this.shape = value;
        }
    }
}