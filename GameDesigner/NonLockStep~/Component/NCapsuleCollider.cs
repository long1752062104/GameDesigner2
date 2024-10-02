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
    public class NCapsuleCollider : NCollider
    {
        public sfloat radius = 0.5f;
        public sfloat height = 1f;

#if JITTER2_PHYSICS
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
#else
        public override IWorldObject OnCreateEntity(NVector3 position)
        {
            var localScale = transform.localScale;
            var value = libm.Max(localScale.x, localScale.z);
            return new Capsule(position, height * localScale.y, radius * value, isStatic ? 0 : 1);
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
            var value = libm.Max(localScale.x, localScale.z);
            var offsetOne = new Vector3(center.x, center.y + height * localScale.y, center.z);
            var offsetTwo = new Vector3(center.x, center.y - height * localScale.y, center.z);
            var centerOneWorld = transform.position + transform.TransformDirection(offsetOne.ToVector3());
            var centerTwoWorld = transform.position + transform.TransformDirection(offsetTwo.ToVector3());
            ColliderDraw.DrawCapsule(centerOneWorld, centerTwoWorld, radius * value);
        }
    }
}
#endif