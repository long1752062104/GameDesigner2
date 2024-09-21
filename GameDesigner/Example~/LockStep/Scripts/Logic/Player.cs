#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using GameDesigner;
using System;
using UnityEngine;

namespace LockStep.Client
{
    [Serializable]
    public class Player : Actor
    {
        public float moveSpeed = 6f;
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
        }

        public override void Update()
        {
            fsm.Execute(StateMachineUpdateMode.Update);
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
            self.jRigidBody.Translate(0, 0, 0);
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
                self.jRigidBody.Translate(0, 0, 0);
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
            if (self.operation.direction == Net.Vector3.zero)
            {
                ChangeState(0);
            }
            else if (self.operation.cmd1 == 1)
            {
                self.operation.cmd1 = 0;
                self.jRigidBody.Translate(0, 0, 0);
                ChangeState(2);
            }
            else
            {
                self.jRigidBody.Rotation = Quaternion.Lerp(self.jRigidBody.Rotation, Quaternion.LookRotation(self.operation.direction, Vector3.up), 0.5f);
                self.jRigidBody.Translate(0, 0, self.moveSpeed * LSTime.deltaTime);
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
            var direction = self.jRigidBody.TransformDirection(Vector3.forward);
            GameWorld.I.CreateBulletActor(self.jRigidBody.TransformPoint(new Vector3(0, 1.5f, 1)), direction);
        }
    }
}
#endif