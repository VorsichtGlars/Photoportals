using UnityEngine;

namespace VRVIS.Photoportals.Maths
{
    class Maths
    {
        //https://forum.unity.com/threads/projection-of-point-on-plane.855958/
        public static Vector3 ClosestPointOnPlane(Vector3 planeOffset, Vector3 planeNormal, Vector3 point)
            => point + DistanceFromPlane(planeOffset, planeNormal, point) * planeNormal;

        public static float DistanceFromPlane(Vector3 planeOffset, Vector3 planeNormal, Vector3 point)
            => Vector3.Dot(planeOffset - point, planeNormal);
        
        public static Matrix4x4 GetTransformationMatrix(Transform t, bool inWorldSpace = true)
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

    }
}
