using Net.Common;
using Net.Entities;

namespace Net.UnityComponent 
{
    public class EntityWorldUnityComponent : SingletonMono<EntityWorldUnityComponent>
    {
        // Update is called once per frame
        void Update()
        {
            EntityWorldSingleton.Instance.DefaultWorld.Simulate(17);
        }
    }
}