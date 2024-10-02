#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using UnityEngine;
#if JITTER2_PHYSICS
using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using System.Collections.Generic;
#else
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUutilities;
#endif

namespace NonLockStep
{
    public class NMeshCollider : NCollider
    {
        public Mesh mesh;

#if JITTER2_PHYSICS
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
                NVector3 v1 = Conversion.ToNVector3(vertices[indices[i + 0]]);
                NVector3 v2 = Conversion.ToNVector3(vertices[indices[i + 1]]);
                NVector3 v3 = Conversion.ToNVector3(vertices[indices[i + 2]]);

                v1 = new NVector3(v1.X * localScale.x, v1.Y * localScale.y, v1.Z * localScale.z);
                v2 = new NVector3(v2.X * localScale.x, v2.Y * localScale.y, v2.Z * localScale.z);
                v3 = new NVector3(v3.X * localScale.x, v3.Y * localScale.y, v3.Z * localScale.z);

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
#else
        public override IWorldObject OnCreateEntity(NVector3 position)
        {
            var localScale = transform.localScale;
            var rotation = transform.rotation;
            var staticTriangleIndices = mesh.GetIndices(0);
            var vertices = mesh.vertices;
            var staticTriangleVertices = new NVector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                staticTriangleVertices[i] = vertices[i];
            return new StaticMesh(staticTriangleVertices, staticTriangleIndices, new AffineTransform(localScale, rotation, position));
        }
#endif

        private void OnValidate()
        {
            if (gameObject.TryGetComponent<MeshFilter>(out var mf))
                mesh = mf.sharedMesh;
        }

        private void OnDrawGizmosSelected()
        {
#if JITTER2_PHYSICS
            Gizmos.color = rigidBody != null ? rigidBody.IsActive ? Color.red : Color.green : Color.green;
#else
            Gizmos.color = Color.green;
#endif
            Gizmos.DrawWireMesh(mesh, transform.position + center, transform.rotation, transform.localScale);
        }
    }
}
#endif