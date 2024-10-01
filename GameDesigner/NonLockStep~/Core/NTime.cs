#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using SoftFloat;

namespace NonLockStep
{
    /// <summary>
    /// 无锁步（Non-lockstep）游戏步骤时间
    /// </summary>
    public class NTime
    {
        public static sfloat time;
        public static sfloat deltaTime = 0.033f;//1 / 30(一秒30次) = 每秒0.033值

        public static void Init()
        {
            time = 0;
        }

        public static void Update()
        {
            time += deltaTime;
        }
    }
}
#endif