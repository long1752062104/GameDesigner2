#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using System.Collections.Generic;

namespace ActorSystem
{
    public class DamageBuffSystem : IActorSystem
    {
        public virtual void Initialize()
        {
        }

        public virtual void Update()
        {
        }
    }

    public class DamageBuffSystem<TActor> : DamageBuffSystem
    {
        public List<IDamageBuff<TActor>> buffs;

        public override void Initialize()
        {
            buffs = new List<IDamageBuff<TActor>>();
        }

        public override void Update()
        {
            for (int i = 0; i < buffs.Count; i++)
            {
                if (!buffs[i].OnUpdate())
                {
                    buffs.RemoveAt(i);
                    if (i >= 0) i--;
                }
            }
        }
    }
}
#endif