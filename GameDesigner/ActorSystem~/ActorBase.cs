#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using UnityEngine;

namespace ActorSystem
{
    public abstract class ActorBase : MonoBehaviour
    {
        public virtual void Awake()
        {
        }

        public virtual void Start()
        {
        }

        public virtual void Update()
        {
        }
    }

    public abstract class ActorBase<TActor> : ActorBase
    {
        public virtual void OnDamage(TActor target, IDamageBuff<TActor> damageBuff)
        {
        }

        public virtual void OnDeath(TActor target)
        {
        }

        public virtual void OnGetExp(TActor target)
        {
        }
    }
}
#endif