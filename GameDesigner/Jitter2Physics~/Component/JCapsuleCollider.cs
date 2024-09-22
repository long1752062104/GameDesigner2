#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using SoftFloat;
using Jitter2.Collision.Shapes;
using System.Collections.Generic;
using UnityEngine;

public class JCapsuleCollider : JCollider
{
    public float radius = 0.5f;
    public float height = 1f;

    public override List<RigidBodyShape> OnCreateShape()
    {
        var localScale = transform.localScale;
        var value = libm.Max(localScale.x, localScale.z);
        return new List<RigidBodyShape>() { new CapsuleShape(radius * value, height * localScale.y)
        {
            Radius = radius * value,
            Length = height * localScale.y,
            CenterOffset = center,
        }};
    }

    private void OnDrawGizmosSelected()
    {
        var localScale = transform.localScale;
        var value = libm.Max(localScale.x, localScale.z);
        var offsetOne = new Vector3(center.x, center.y + height * localScale.y, center.z);
        var offsetTwo = new Vector3(center.x, center.y - height * localScale.y, center.z);
        var centerOneWorld = transform.position + transform.TransformDirection(offsetOne.ToVector3());
        var centerTwoWorld = transform.position + transform.TransformDirection(offsetTwo.ToVector3());
        ColliderDraw.DrawCapsule(centerOneWorld, centerTwoWorld, Color.green, radius * value);
    }
}
#endif