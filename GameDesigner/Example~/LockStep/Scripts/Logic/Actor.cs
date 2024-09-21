#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
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
        public Rigidbody rigidBody;
        public JCollider jRigidBody;
        internal Operation operation;

        public virtual void Start() { }

        public virtual void Update() { }

        public virtual void Destroy()
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }
}
#endif