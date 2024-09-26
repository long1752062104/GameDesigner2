#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Jitter2.Dynamics;
using UnityEngine;

namespace LockStep.Client
{
    public class Bullet : Actor
    {
        public Actor This;
        public float speed = 20f;
        public Vector3 direction;

        public override void Update()
        {
            rigidBody.Velocity = direction * speed;
        }

        internal void Init()
        {
            rigidBody.onCollisionEnter = OnCollisionEnter;
        }

        private void OnCollisionEnter(JCollider collider, Arbiter arbiter)
        {
            if (collider.rigidBody == This.rigidBody.rigidBody)
                return;
            if (collider.Tag is Actor actor) 
            {
                actor.OnDamage(This);
            }
            GameWorld.I.RemoveActor(this);
        }
    }
}
#endif