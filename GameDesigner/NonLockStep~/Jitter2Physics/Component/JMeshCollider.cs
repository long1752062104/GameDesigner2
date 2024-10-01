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
        var localScale = transform.localScale;

        var indices = mesh.GetIndices(0);
        var vertices = mesh.vertices;

        List<RigidBodyShape> shapesToAdd = new();
        List<JTriangle> triangles = new();

        for (int i = 0; i < indices.Length; i += 3)
        {
            JVector v1 = Conversion.ToJVector(vertices[indices[i + 0]]);
            JVector v2 = Conversion.ToJVector(vertices[indices[i + 1]]);
            JVector v3 = Conversion.ToJVector(vertices[indices[i + 2]]);

            v1 = new JVector(v1.X * localScale.x, v1.Y * localScale.y, v1.Z * localScale.z);
            v2 = new JVector(v2.X * localScale.x, v2.Y * localScale.y, v2.Z * localScale.z);
            v3 = new JVector(v3.X * localScale.x, v3.Y * localScale.y, v3.Z * localScale.z);

            triangles.Add(new JTriangle(v1, v2, v3));
        }

        var jtm = new TriangleMesh(triangles);

        for (int i = 0; i < jtm.Indices.Length; i++)
        {
            var ts = new FatTriangleShape(jtm, i, 0.2f)
            {
                CenterOffset = center
            };
            shapesToAdd.Add(ts);
        }

        return shapesToAdd;
    }

    private void OnValidate()
    {
        if (gameObject.TryGetComponent<MeshFilter>(out var mf))
            mesh = mf.sharedMesh;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = rigidBody != null ? rigidBody.IsActive ? Color.red : Color.green : Color.green;
        Gizmos.DrawWireMesh(mesh, transform.position + center, transform.rotation, transform.localScale);
    }
}
#endif