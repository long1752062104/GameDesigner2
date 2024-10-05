#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
namespace NonLockStep
{
    public enum InitializeMode
    {
        None = 0,
        Awake,
        Start,
        OnEnable,
    }

    public enum DeinitializeMode
    {
        None = 0,
        OnDisable,
        OnDestroy,
    }
}
#endif