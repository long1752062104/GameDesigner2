#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL

namespace ActorSystem
{
    public interface IDamageBuff<TActor>
    {
        string Name { get; set; }
        /// <summary>
        /// 伤害效果持续时间
        /// </summary>
        float Duration { get; set; }
        /// <summary>
        /// 自身对象
        /// </summary>
        TActor Self { get; set; }
        /// <summary>
        /// 被攻击的对象
        /// </summary>
        TActor Other { get; set; }
        /// <summary>
        /// 当进入伤害效果
        /// </summary>
        void OnBuffBegin();
        /// <summary>
        /// 当结束伤害效果
        /// </summary>
        void OnBuffEnd();
        /// <summary>
        /// 伤害效果每帧更新
        /// </summary>
        /// <returns>是否还继续更新，true继续更新，false不再更新</returns>
        bool OnUpdate();
    }
}
#endif