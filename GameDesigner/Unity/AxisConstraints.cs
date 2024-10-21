#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using System;

namespace Net.Unity
{
    [Serializable]
    public struct AxisConstraints
    {
        public bool X;
        public bool Y;
        public bool Z;

        public readonly bool Any => X || Y || Z;
        public readonly bool All => X && Y && Z;
    }

    [Serializable]
    public struct Axis2Constraints
    {
        public bool X;
        public bool Y;
        public bool Z;
        public bool W;

        public readonly bool Any => X || Y || Z || W;
        public readonly bool All => X && Y && Z && W;
    }
}
#endif