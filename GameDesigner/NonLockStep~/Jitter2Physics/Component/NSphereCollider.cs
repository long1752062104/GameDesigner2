#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using SoftFloat;
using UnityEngine;
#if JITTER2_PHYSICS
using Jitter2.Collision.Shapes;
using System.Collections.Generic;
#else
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.Entities.Prefabs;
#endif

namespace NonLockStep
{
    public class NSphereCollider : NCollider
    {
        public sfloat radius = 0.5f;

#if JITTER2_PHYSICS
        public override List<RigidBodyShape> OnCreateShape()
        {
            var localScale = transform.localScale;
            var value = libm.Max(localScale.x, localScale.y);
            value = libm.Max(value, localScale.z);
            return new List<RigidBodyShape>()
            {
                new SphereShape(radius * value)
                {
                    CenterOffset = center
                }
            };
        }
#else
        public override IWorldObject OnCreateEntity(NVector3 position)
        {
            var localScale = transform.localScale;
            var value = libm.Max(localScale.x, localScale.y);
            value = libm.Max(value, localScale.z);
            return new Sphere(position, radius * value, isStatic ? 0 : 1);
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
            var value = libm.Max(localScale.x, localScale.y);
            value = libm.Max(value, localScale.z);
            Gizmos.DrawWireSphere(transform.position + center, radius * value);
        }
    }
}
#endif