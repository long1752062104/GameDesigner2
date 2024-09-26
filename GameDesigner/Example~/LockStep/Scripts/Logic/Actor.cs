#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Jitter2.LinearMath;
using Net.Share;
using System;
using UnityEngine;

namespace LockStep.Client
{
    [Serializable]
    public class Actor
    {
        public string name;
        public GameObject gameObject;
        public Animation animation;
        public JCollider rigidBody;
        internal Operation operation;
        public int Health = 100;
        public int Damage = 20;

        public JVector Position => rigidBody.Position;
        public JQuaternion Rotation { get => rigidBody.Rotation; set => rigidBody.Rotation = value; }

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