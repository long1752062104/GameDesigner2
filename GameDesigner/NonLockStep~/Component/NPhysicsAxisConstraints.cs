#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using System;

namespace NonLockStep
{
    [Serializable]
    public struct NPhysicsAxisConstraints
    {
        public bool X;
        public bool Y;
        public bool Z;

        public readonly bool Any => X || Y || Z;
    }
}
#endif