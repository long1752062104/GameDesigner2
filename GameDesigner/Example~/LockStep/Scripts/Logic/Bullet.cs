#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using UnityEngine;

namespace LockStep.Client
{
    public class Bullet : Actor
    {
        public float speed = 20f;
        public Vector3 direction;

        public override void Update()
        {
            jRigidBody.Velocity = direction * speed;
        }
    }
}
#endif