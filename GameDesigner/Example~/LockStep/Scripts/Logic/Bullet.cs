#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using BEPUutilities;
using System;
using UnityEngine;

namespace NonLockStep.Client
{
    [Serializable]
    public class Bullet : Actor, ICollisionEnterListener
    {
        public Actor This;
        public float speed = 20f;
        public Vector3 direction;
        public GameObject explosionPrefab;

        public override void Update()
        {
            rigidBody.Velocity = direction * speed;
        }

        internal void Init()
        {
            rigidBody.AddListener(this);
        }

        public void OnNCollisionEnter(NRigidbody other, object collisionInfo)
        {
            if (other == This.rigidBody)
                return;
            if (other.Tag is Actor actor)
                actor.OnDamage(This);

            var collision = NCollision.GetCollision(collisionInfo);
            var contact = collision.contacts[0];
            var rot = Quaternion.FromToRotation(NVector3.Up, contact.Normal);
            var pos = contact.Point;
            var explosion = UnityEngine.Object.Instantiate(explosionPrefab, pos, rot);
            UnityEngine.Object.Destroy(explosion, 1f);

            GameWorld.I.RemoveActor(this);
        }
    }
}
#endif