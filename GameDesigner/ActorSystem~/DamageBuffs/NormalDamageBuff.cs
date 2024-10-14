#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
namespace ActorSystem
{
    public class NormalDamageBuff : IDamageBuff<Actor>
    {
        public int Damage = 20;
        public string Name { get; set; }
        public float Duration { get; set; }
        public Actor Self { get; set; }
        public Actor Other { get; set; }

        public void OnBuffBegin()
        {
            Other.Health -= Damage;
        }

        public void OnBuffEnd()
        {
        }

        public bool OnUpdate()
        {
            return false;
        }
    }
}
#endif