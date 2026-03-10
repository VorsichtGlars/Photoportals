using System;
using UnityEngine;
using VRSYS.Core.Logging;

namespace VRSYS.Photoportals.Maths {

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
                ExtendedLogger.LogWarning(this.GetType().Name, "x value for TransferFucntion should be positive");
            }
            if (x < 0.0f || x > 1.0f) {
                ExtendedLogger.LogWarning(this.GetType().Name, "x value for TransferFucntion should be [0.0,1.0]");
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
    public static class Geometry {
        //https://forum.unity.com/threads/projection-of-point-on-plane.855958/
        public static Vector3 ClosestPointOnPlane(Vector3 planeOffset, Vector3 planeNormal, Vector3 point)
            => point + DistanceFromPlane(planeOffset, planeNormal, point) * planeNormal;

        public static float DistanceFromPlane(Vector3 planeOffset, Vector3 planeNormal, Vector3 point)
            => Vector3.Dot(planeOffset - point, planeNormal);
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
}
