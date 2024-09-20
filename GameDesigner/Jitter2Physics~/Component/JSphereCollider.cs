#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Jitter2.Collision.Shapes;
using System.Collections.Generic;
using UnityEngine;

public class JSphereCollider : JCollider
{
    public float radius = 0.5f;

    public override List<RigidBodyShape> OnCreateShape()
    {
        var localScale = transform.localScale;
        var value = MathF.Max(localScale.x, localScale.y);
        value = Mathf.Max(value, localScale.z);
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
        Gizmos.color = Color.green;
        var localScale = transform.localScale;
        var value = MathF.Max(localScale.x, localScale.y);
        value = Mathf.Max(value, localScale.z);
        Gizmos.DrawWireSphere(transform.position + center, radius * value);
    }
}
#endif