using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

/**
https://github.com/Demigiant/dotween/issues/565
http://forum.demigiant.com/index.php?topic=225.0
**/

public static class DOTweenExtensions
{
    public static TweenerCore<Vector3, Vector3, VectorOptions> DOFollowPosition(this Transform transform, Transform target, float duration)
    {
        var t = DOTween.To(
            () => transform.position - target.transform.position, // Value getter
            x => transform.position = x + target.transform.position, // Value setter
            Vector3.zero, 
            duration);
        t.SetTarget(transform);
        return t;
    }

    public static Sequence DOFollowTransform(this Transform transform, Transform target, float duration)
    {
        var seq = DOTween.Sequence();

        var move = DOTween.To(
            () => transform.position - target.position,
            x  => transform.position = x + target.position,
            Vector3.zero,
            duration
        );

        var startLocalEuler = (Quaternion.Inverse(target.rotation) * transform.rotation).eulerAngles;
        if (startLocalEuler.x > 180f) startLocalEuler.x -= 360f;
        if (startLocalEuler.y > 180f) startLocalEuler.y -= 360f;
        if (startLocalEuler.z > 180f) startLocalEuler.z -= 360f;

        var rot = DOTween.To(
            () => startLocalEuler,
            x  => transform.rotation = target.rotation * Quaternion.Euler(x),
            Vector3.zero,
            duration
        );

        seq.Join(move);
        seq.Join(rot);
        seq.SetTarget(transform);
        return seq;
    }

}