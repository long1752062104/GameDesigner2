#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using System;
using SoftFloat;
using GameDesigner;
using UnityEngine;

#if JITTER2_PHYSICS
using Jitter2.LinearMath;
#else
using BEPUutilities;
#endif

namespace NonLockStep.Client
{
    [Serializable]
    public class Player : Actor
    {
        public sfloat moveSpeed = 10f;
        public StateMachineCore fsm = new();

        public override void Start()
        {
            fsm.Handler = new TimeStateMachine();
            fsm.Init();
            fsm.AddState("idle", new PlayerIdle(this)).animLoop = true;
            fsm.AddState("run", new PlayerRun(this)).animLoop = true;
            var fireState = fsm.AddState("fire");
            fireState.Action.AddComponent(new PlayerFire(this));
            fireState.AddTransition(0);
            fireState.isExitState = true;
            fireState.DstStateID = 0;
            fireState.actionSystem = true;
            fireState.animSpeed = 6f;
            fsm.AddState("die", new PlayerDie(this));
        }

        public override void Update()
        {
            fsm.Execute(StateMachineUpdateMode.Update);
        }

        public override void OnDamage(Actor attack)
        {
            if (IsDeath)
                return;
            Health -= attack.Damage;
            if (Health <= 0)
            {
                fsm.ChangeState(3, 0, true);
            }
        }
    }

    public class PlayerIdle : StateBehaviour
    {
        private readonly Player self;

        public PlayerIdle(Player player)
        {
            self = player;
        }

        public override void OnEnter()
        {
            self.animation.Play("soldierIdleRelaxed");
            self.rigidBody.Translate(0, 0, 0);
        }

        public override void OnUpdate()
        {
            if (self.operation.direction != Net.Vector3.zero)
            {
                ChangeState(1);
            }
            else if (self.operation.cmd1 == 1)
            {
                self.operation.cmd1 = 0;
                self.rigidBody.Translate(0, 0, 0);
                ChangeState(2);
            }
        }
    }

    public class PlayerRun : StateBehaviour
    {
        private readonly Player self;

        public PlayerRun(Player player)
        {
            self = player;
        }

        public override void OnEnter()
        {
            self.animation.Play("soldierSprint");
        }

        public override void OnUpdate()
        {
            var direction = (Vector3)self.operation.direction;
            if (direction == Vector3.zero)
            {
                ChangeState(0);
            }
            else if (self.operation.cmd1 == 1)
            {
                self.operation.cmd1 = 0;
                self.rigidBody.Translate(0, 0, 0);
                ChangeState(2);
            }
            else
            {
                self.rigidBody.Rotation = NQuaternion.Lerp(self.rigidBody.Rotation, NQuaternion.LookRotation(direction, NVector3.UnitY), 0.5f);
                self.rigidBody.Translate(0, 0, self.moveSpeed * NTime.deltaTime);
            }
        }
    }

    public class PlayerFire : ActionCoreBase
    {
        private readonly Player self;

        public PlayerFire(Player player)
        {
            self = player;
        }

        public override void OnEnter(StateAction action)
        {
            self.animation.Play("soldierIdle");
        }

        public override void OnAnimationEvent(StateAction action)
        {
            var direction = self.rigidBody.TransformDirection(NVector3.UnitZ);
            GameWorld.I.CreateBulletActor(self, self.rigidBody.TransformPoint(new NVector3(0, 0.5f, 0.5f)), direction);
        }
    }

    public class PlayerDie : StateBehaviour
    {
        private readonly Player self;

        public PlayerDie(Player player)
        {
            self = player;
        }

        public override void OnEnter()
        {
            self.animation.Play(NRandom.Range(0, 2) == 0 ? "soldierDieBack" : "soldierDieFront");
        }
    }
}
#endif