#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using SoftFloat;
using Jitter2.Collision.Shapes;
using System.Collections.Generic;
using UnityEngine;

public class JSphereCollider : JCollider
{
    public sfloat radius = 0.5f;

    public override List<RigidBodyShape> OnCreateShape()
    {
        var localScale = transform.localScale;
        var value = libm.Max(localScale.x, localScale.y);
        value = libm.Max(value, localScale.z);
        return new List<RigidBodyShape>()
        {
            new SphereShape(radius * value)
            {
                CenterOffset = center
            }
        };
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = rigidBody != null ? rigidBody.IsActive ? Color.red : Color.green : Color.green;
        var localScale = transform.localScale;
        var value = libm.Max(localScale.x, localScale.y);
        value = libm.Max(value, localScale.z);
        Gizmos.DrawWireSphere(transform.position + center, radius * value);
    }
}
#endif