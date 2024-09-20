#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Jitter2.Collision.Shapes;
using System.Collections.Generic;
using UnityEngine;

public class JCapsuleCollider : JCollider
{
    public float radius = 0.5f;
    public float height = 1f;

    public override List<RigidBodyShape> OnCreateShape()
    {
        return new List<RigidBodyShape>() { new CapsuleShape
        {
            Radius = radius,
            Length = height,
        }};
    }

    private void OnDrawGizmosSelected()
    {
        var offsetOne = new Vector3(center.x, center.y + 0.5f * height + radius, center.z);
        var offsetTwo = new Vector3(center.x, center.y - 0.5f * height - radius, center.z);
        var centerOneWorld = transform.position + transform.TransformDirection(offsetOne.ToVector3());
        var centerTwoWorld = transform.position + transform.TransformDirection(offsetTwo.ToVector3());
        ColliderDraw.DrawCapsule(centerOneWorld, centerTwoWorld, Color.green, radius);
    }
}
#endif