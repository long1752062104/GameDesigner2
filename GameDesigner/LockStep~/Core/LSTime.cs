#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using SoftFloat;

namespace LockStep
{
    public class LSTime
    {
        public static sfloat time;
        public static sfloat deltaTime = 0.033f;//1 / 30(一秒30次) = 每秒0.033值
    }
}
#endif