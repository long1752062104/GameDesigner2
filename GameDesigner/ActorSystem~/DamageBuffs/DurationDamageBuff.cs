#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using UnityEngine;

namespace ActorSystem
{
    public class DurationDamageBuff : IDamageBuff<Actor>
    {
        public int Health = 20; //这里是减血
        public int Damage = 20; //还能减你伤害
        public float MoveSpeed = 6f; //减你移动速度
        public float AttackSpeed = 1f; //减你攻击速度
        public Actor Self { get; set; }
        public Actor Other { get; set; }
        public string Name { get; set; }
        public float Duration { get; set; }
        private float timeout;

        public void OnBuffBegin()
        {
            Other.Health -= Health;
            Other.Damage -= Damage;
            Other.AttackSpeed -= AttackSpeed;
            Other.MoveSpeed -= MoveSpeed;
            timeout = Time.time + Duration;
        }

        public void OnBuffEnd()
        {
            Other.Damage += Damage;
            Other.AttackSpeed += AttackSpeed;
            Other.MoveSpeed += MoveSpeed;
        }

        public bool OnUpdate()
        {
            if (Time.time > timeout)
            {
                OnBuffEnd();
                return false;
            }
            return true;
        }
    }
}
#endif