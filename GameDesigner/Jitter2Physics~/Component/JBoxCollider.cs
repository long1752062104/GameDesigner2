#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using System.Collections.Generic;
using UnityEngine;

public class JBoxCollider : JCollider
{
    public JVector size = JVector.One;

    public override List<RigidBodyShape> OnCreateShape()
    {
        var localScale = transform.localScale;
        return new List<RigidBodyShape>()
        {
            new BoxShape(size.X * -localScale.x, size.Y * localScale.y, size.Z * localScale.z) // size.ToJVector()); //这个导致相反了
            {
                CenterOffset = center,
            }
        };
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        var localScale = transform.localScale;
        var cubeTransform = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(-size.X * localScale.x, size.Y * localScale.y, size.Z * localScale.z));
        Gizmos.matrix = cubeTransform;
        Gizmos.DrawWireCube(center, Vector3.one);
    }
}
#endif