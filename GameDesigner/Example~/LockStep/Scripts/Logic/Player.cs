#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using System;
using SoftFloat;
using GameDesigner;
using UnityEngine;
#if JITTER2_PHYSICS
using Jitter2.LinearMath;
#else
using BEPUutilities;
using BEPUphysics;
using Ray = BEPUutilities.Ray;
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
            fsm.AddState("jump", new PlayerJump(this));
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
            else if (self.operation.cmd1 == 2)
            {
                self.operation.cmd1 = 0;
                //self.rigidBody.Translate(0, 0, 0);
                ChangeState(4);
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
            else if (self.operation.cmd1 == 2)
            {
                self.operation.cmd1 = 0;
                ChangeState(4);
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

    public class PlayerJump : StateBehaviour
    {
        private readonly Player self;
        private readonly NCharacterController characterController;
        private sfloat jumpCheckTime;

        public PlayerJump(Player player)
        {
            self = player;
            characterController = player.rigidBody as NCharacterController;
        }

        public override void OnEnter()
        {
            self.animation.Play("soldierFalling");
            characterController.Jump();
            jumpCheckTime = NTime.time + 0.2f;
        }

        public override void OnUpdate()
        {
            var direction = (Vector3)self.operation.direction;
            var targetRotation = NQuaternion.LookRotation(direction, NVector3.UnitY);
            var newRotation = new NQuaternion(0, targetRotation.Y, 0, targetRotation.W); //解决角色跳跃时，没有输入方向，就会翻转的问题
            self.rigidBody.Rotation = NQuaternion.Lerp(self.rigidBody.Rotation, newRotation, 0.5f);
            self.rigidBody.Translate(0, 0, self.moveSpeed * NTime.deltaTime);
            if (NTime.time < jumpCheckTime)
                return;
            var ray = new Ray(characterController.Position, new Vector3(0, -1, 0)); // 向下的射线
            if (NPhysics.RayCast(ray, 1f, (entity) => entity != characterController.Entity.CollisionInformation, out RayCastResult result))
            {
                ChangeState(0);
            }
        }
    }
}
#endif