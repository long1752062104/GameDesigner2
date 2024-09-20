using Jitter2.Collision.Shapes;
using System.Collections.Generic;
using UnityEngine;

public class JSphereCollider : JCollider
{
    public float radius = 0.5f;

    public override List<RigidBodyShape> OnCreateShape()
    {
        return new List<RigidBodyShape>() { new SphereShape(radius) };
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}