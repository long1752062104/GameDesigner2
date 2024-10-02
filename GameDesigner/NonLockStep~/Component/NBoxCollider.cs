#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using UnityEngine;


#if JITTER2_PHYSICS
using Jitter2.Collision.Shapes;
using Jitter2.LinearMath;
using System.Collections.Generic;
#else
using BEPUutilities;
using BEPUphysics;
using BEPUphysics.Entities.Prefabs;
#endif

namespace NonLockStep
{
    public class NBoxCollider : NCollider
    {
        public NVector3 size = NVector3.One;

#if JITTER2_PHYSICS
        public override List<RigidBodyShape> OnCreateShape()
        {
            var localScale = transform.localScale;
            return new List<RigidBodyShape>()
            {
                new BoxShape(size.X * -localScale.x, size.Y * localScale.y, size.Z * localScale.z) // size.ToNVector3()); //这个导致相反了
                {
                    CenterOffset = center,
                }
            };
        }
#else
        public override IWorldObject OnCreateEntity(NVector3 position)
        {
            var localScale = transform.localScale;
            return new Box(position, size.X * localScale.x, size.Y * localScale.y, size.Z * localScale.z, isStatic ? 0 : 1);
        }
#endif

        private void OnDrawGizmosSelected()
        {
#if JITTER2_PHYSICS
            Gizmos.color = rigidBody != null ? rigidBody.IsActive ? Color.red : Color.green : Color.green;
#else
            Gizmos.color = Color.green;
#endif
            var localScale = transform.localScale;
            var cubeTransform = Matrix4x4.TRS(transform.position, transform.rotation, new Vector3(-size.X * localScale.x, size.Y * localScale.y, size.Z * localScale.z));
            Gizmos.matrix = cubeTransform;
            Gizmos.DrawWireCube(center, Vector3.one);
        }
    }
}
#endif