#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
namespace Net.Component
{
    using UnityEngine;

    /// <summary>
    /// 网络时间中心控制, 控制发送频率, 不能乱来发送! 一个行为一秒可以发送30次同步
    /// </summary>
    public class NetworkTime : SingleCase<NetworkTime>
    {
        private static bool canSent;
        /// <summary>
        /// 当前是否可以发送数据? 这里可以控制发送次数, 一秒30帧数据左右
        /// </summary>
        public static bool CanSent => canSent;
        /// <summary>
        /// 网络帧率
        /// </summary>
        public static int NetworkFrame;
        public int currentFrame;
        public int frame;
        /// <summary>
        /// 设置可发送时间 默认30次/秒
        /// </summary>
        public float CanSentTime = 1f / 30f;
        private float timePerSecond;
        private float fixedUpdateTimer;

        private void FixedUpdate()
        {
            fixedUpdateTimer += Time.fixedDeltaTime;
            if (fixedUpdateTimer >= CanSentTime)
            {
                fixedUpdateTimer -= CanSentTime;
                canSent = true;
                currentFrame++;
            }
            else
            {
                canSent = false;
            }
            if (Time.fixedTime >= timePerSecond)
            {
                timePerSecond = Time.fixedTime + 1f;
                NetworkFrame = frame = currentFrame;
                currentFrame = 0;
            }
        }
    }
}
#endif