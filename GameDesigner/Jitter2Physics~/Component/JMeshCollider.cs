#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using System.Collections.Generic;
using UnityEngine;

public class JMeshCollider : JCollider
{
    public Mesh mesh;

    public override List<RigidBodyShape> OnCreateShape()
    {
        return CreateShapes();
    }

    public List<RigidBodyShape> CreateShapes()
    {
        var indices = mesh.GetIndices(0);
        var vertices = mesh.vertices;

        List<RigidBodyShape> shapesToAdd = new();
        List<JTriangle> triangles = new();

        for (int i = 0; i < indices.Length; i += 3)
        {
            JVector v1 = Conversion.ToJVector(vertices[indices[i + 0]]);
            JVector v2 = Conversion.ToJVector(vertices[indices[i + 1]]);
            JVector v3 = Conversion.ToJVector(vertices[indices[i + 2]]);

            triangles.Add(new JTriangle(v1, v2, v3));
        }

        var jtm = new TriangleMesh(triangles);

        for (int i = 0; i < jtm.Indices.Length; i++)
        {
            var ts = new FatTriangleShape(jtm, i);
            shapesToAdd.Add(ts);
        }

        return shapesToAdd;
    }

    private void OnDrawGizmosSelected()
    {
        //Gizmos.DrawWireMesh(mesh, transform.position, transform.rotation);
    }
}
#endif