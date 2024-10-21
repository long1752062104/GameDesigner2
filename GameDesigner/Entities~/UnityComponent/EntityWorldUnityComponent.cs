#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using Net.Common;
using Net.Entities;

namespace Net.UnityComponent 
{
    public class EntityWorldUnityComponent : SingletonMono<EntityWorldUnityComponent>
    {
        // Update is called once per frame
        void Update()
        {
            WorldSingleton.Instance.DefaultWorld.Simulate(17);
        }
    }
}
#endif