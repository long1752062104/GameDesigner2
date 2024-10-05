#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using System;
using Net.Share;
using UnityEngine;
#if JITTER2_PHYSICS
using Jitter2.Collision.Shapes;
using System.Collections.Generic;
using Jitter2.LinearMath;
#else
using BEPUutilities;
#endif

namespace NonLockStep.Client
{
    [Serializable]
    public class Actor
    {
        public string name;
        public GameObject gameObject;
        public Animation animation;
        public NRigidbody rigidBody;
        internal Operation operation;
        public int Health = 100;
        public int Damage = 20;

        public NVector3 Position => rigidBody.Position;
        public NQuaternion Rotation { get => rigidBody.Rotation; set => rigidBody.Rotation = value; }

        public bool IsDeath => Health <= 0;

        public virtual void Start() { }

        public virtual void Update() { }

        public virtual void OnDamage(Actor attack) { }

        public virtual void Destroy()
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }
}
#endif