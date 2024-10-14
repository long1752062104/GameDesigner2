#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL

namespace ActorSystem
{
    public interface IActorSystem
    {
        void Initialize();

        void Update();
    }
}
#endif