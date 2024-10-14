#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using GameDesigner;
using UnityEngine;

namespace ActorSystem
{
    public class Actor : ActorBase<Actor>
    {
        public int Health = 100;
        public int HealthMax = 100;
        public int Damage = 20;
        public float MoveSpeed = 6f;
        public float AttackSpeed = 1f;
        public bool IsDeath => Health <= 0;
    }

    public class Player : Actor
    {
        public AnimatorStateMachineView view;

        public override void Awake()
        {
            view.Init();
        }

        public override void OnDamage(Actor target, IDamageBuff<Actor> damageBuff)
        {
            if (target.IsDeath)
                return;
            damageBuff.Other = this;
            damageBuff.OnBuffBegin();
            if (target.IsDeath)
            {
                OnDeath(target);
                OnGetExp(target);
            }
        }

        public override void OnDeath(Actor target)
        {

        }

        public override void OnGetExp(Actor target)
        {

        }

        private void OnEnable()
        {
            StateMachineSystem.AddStateMachine(view.stateMachine);
        }

        private void OnDisable()
        {
            StateMachineSystem.RemoveStateMachine(view.stateMachine);
        }
    }

    public class PlayerIdle : StateBehaviour
    {
        private Actor self;

        public override void OnInit()
        {
            self = transform.GetComponent<Actor>();
        }

        public override void OnUpdate()
        {
            var dir = InputEx.Direction;
            if (dir != Vector3.zero)
            {
                ChangeState(1);
            }
        }
    }

    public class PlayerRun : StateBehaviour
    {
        private Actor self;

        public override void OnInit()
        {
            self = transform.GetComponent<Actor>();
        }

        public override void OnUpdate()
        {
            var dir = Camera.main.transform.Transform3Dir();
            if (dir == Vector3.zero)
            {
                ChangeState(0);
            }
            else
            {
                transform.LerpRotation(dir);
                transform.Translate(0, 0, self.MoveSpeed * Time.deltaTime);
            }
        }
    }

    public class PlayerIdle2D : StateBehaviour
    {
        private Actor self;

        public override void OnInit()
        {
            self = transform.GetComponent<Actor>();
        }

        public override void OnUpdate()
        {
            var dir = InputEx.Direction2D;
            if (dir != Vector2.zero)
            {
                ChangeState(1);
            }
        }
    }

    public class PlayerRun2D : StateBehaviour
    {
        private Actor self;
        private Rigidbody2D rigidbody;

        public override void OnInit()
        {
            self = transform.GetComponent<Actor>();
            rigidbody = transform.GetComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0; // 禁用重力
        }

        public override void OnUpdate()
        {
            var dir = InputEx.Direction2D;
            if (dir == Vector2.zero)
            {
                ChangeState(0);
                rigidbody.velocity = Vector2.zero;
            }
            else
            {
                Vector2 moveDirection = dir.normalized * self.MoveSpeed;
                rigidbody.velocity = new Vector2(moveDirection.x, moveDirection.y);
                if (dir.x > 0)
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                else if (dir.x < 0)
                    transform.rotation = Quaternion.Euler(0, 180f, 0);
            }
        }
    }
}
#endif