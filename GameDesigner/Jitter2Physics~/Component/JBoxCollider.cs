using Jitter2.Collision.Shapes;
using System.Collections.Generic;
using UnityEngine;

public class JBoxCollider : JCollider
{
    public Vector3 size = Vector3.one;

    public override List<RigidBodyShape> OnCreateShape()
    {
        return new List<RigidBodyShape>() { new BoxShape(size.x, size.y, size.z) }; // size.ToJVector()); //这个导致相反了
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Matrix4x4 cubeTransform = Matrix4x4.TRS(transform.position, transform.rotation, size);
        Gizmos.matrix = cubeTransform;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}