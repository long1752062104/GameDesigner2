#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
namespace NonLockStep
{
    /// <summary>
    /// 无锁步（Non-lockstep）回滚接口
    /// </summary>
    public interface IRollback
    {
    }

    /// <summary>
    /// 无锁步（Non-lockstep）回滚管理
    /// </summary>
    public class NRollback
    {
    }
}
#endif